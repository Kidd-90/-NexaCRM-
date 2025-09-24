using System.Reflection;
using System.Threading;
using BuildingBlocks.Common.Supabase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Supabase;

namespace BuildingBlocks.Common.UnitTests;

public class SupabaseClientFactoryTests
{
    private static ServiceProvider BuildProvider(Action<SupabaseSettings> configure)
    {
        var services = new ServiceCollection();
        services.AddSupabaseCore(options =>
        {
            options.Database.ConnectionString = "Host=localhost;Username=postgres;Password=pass";
            configure(options);
        });
        return services.BuildServiceProvider();
    }

    [Fact]
    public void GetServiceClient_ReturnsCachedClientWithConfiguredKey()
    {
        using var provider = BuildProvider(options =>
        {
            options.Url = " https://unit-test.supabase.co ";
            options.ServiceRoleKey = " service-key ";
        });

        var factory = provider.GetRequiredService<ISupabaseClientFactory>();

        var first = factory.GetServiceClient();
        var second = factory.GetServiceClient();

        Assert.Same(first, second);
        Assert.Equal("service-key", GetPrivateField<string>(first, "_supabaseKey"));
        Assert.Equal("https://unit-test.supabase.co", GetPrivateField<string>(first, "_supabaseUrl"));

        var options = GetPrivateField<global::Supabase.SupabaseOptions>(first, "_options");
        Assert.False(options.AutoConnectRealtime);
        Assert.True(options.AutoRefreshToken);
    }

    [Fact]
    public void GetServiceClient_ThrowsWhenServiceKeyMissing()
    {
        using var provider = BuildProvider(options =>
        {
            options.Url = "https://unit-test.supabase.co";
        });

        var factory = provider.GetRequiredService<ISupabaseClientFactory>();

        Assert.Throws<InvalidOperationException>(() => factory.GetServiceClient());
    }

    [Fact]
    public void GetAnonClient_ThrowsWhenAnonKeyMissing()
    {
        using var provider = BuildProvider(options =>
        {
            options.Url = "https://unit-test.supabase.co";
        });

        var factory = provider.GetRequiredService<ISupabaseClientFactory>();

        Assert.Throws<InvalidOperationException>(() => factory.GetAnonClient());
    }

    [Fact]
    public void CreateClient_UsesCustomOptionsAndDoesNotCache()
    {
        using var provider = BuildProvider(options =>
        {
            options.Url = "https://unit-test.supabase.co";
            options.AnonKey = "anon-key";
            options.Client.AutoConnectRealtime = true;
        });

        var factory = provider.GetRequiredService<ISupabaseClientFactory>();

        var first = factory.CreateClient("custom-key", o =>
        {
            o.AutoConnectRealtime = false;
            o.AutoRefreshToken = false;
        });
        var second = factory.CreateClient("custom-key");

        Assert.NotSame(first, second);
        Assert.Equal("custom-key", GetPrivateField<string>(first, "_supabaseKey"));

        var firstOptions = GetPrivateField<global::Supabase.SupabaseOptions>(first, "_options");
        Assert.False(firstOptions.AutoConnectRealtime);
        Assert.False(firstOptions.AutoRefreshToken);

        var secondOptions = GetPrivateField<global::Supabase.SupabaseOptions>(second, "_options");
        Assert.True(secondOptions.AutoConnectRealtime);
        Assert.True(secondOptions.AutoRefreshToken);
    }

    [Fact]
    public void GetServiceClient_RecreatesCacheWhenSettingsChange()
    {
        var monitor = new TestOptionsMonitor<SupabaseSettings>(new SupabaseSettings
        {
            Url = "https://unit-test.supabase.co",
            ServiceRoleKey = "initial-key",
            Database = new SupabaseDatabaseSettings { ConnectionString = "Host=localhost;Username=postgres;Password=pass" },
            Client = new SupabaseClientOptions()
        });

        using var factory = new SupabaseClientFactory(monitor);

        var first = factory.GetServiceClient();

        monitor.Update(new SupabaseSettings
        {
            Url = "https://unit-test.supabase.co",
            ServiceRoleKey = "rotated-key",
            Database = new SupabaseDatabaseSettings { ConnectionString = "Host=localhost;Username=postgres;Password=pass" },
            Client = new SupabaseClientOptions()
        });

        var second = factory.GetServiceClient();

        Assert.NotSame(first, second);
        Assert.Equal("rotated-key", GetPrivateField<string>(second, "_supabaseKey"));
    }

    [Fact]
    public void CreateClient_TrimsSuppliedValues()
    {
        var monitor = new TestOptionsMonitor<SupabaseSettings>(new SupabaseSettings
        {
            Url = " https://unit-test.supabase.co ",
            ServiceRoleKey = " service-key ",
            Database = new SupabaseDatabaseSettings { ConnectionString = "Host=localhost;Username=postgres;Password=pass" },
            Client = new SupabaseClientOptions()
        });

        using var factory = new SupabaseClientFactory(monitor);

        var client = factory.CreateClient("  custom-key  ");

        Assert.Equal("custom-key", GetPrivateField<string>(client, "_supabaseKey"));
        Assert.Equal("https://unit-test.supabase.co", GetPrivateField<string>(client, "_supabaseUrl"));
    }

    private static T GetPrivateField<T>(Client client, string fieldName)
    {
        var field = client.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        var value = field!.GetValue(client);
        Assert.IsType<T>(value);
        return (T)value;
    }

    private sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
    {
        private readonly object _gate = new();
        private event Action<T, string?>? _listeners;
        private T _currentValue;

        public TestOptionsMonitor(T currentValue)
        {
            _currentValue = currentValue;
        }

        public T CurrentValue => _currentValue;

        public T Get(string? name) => _currentValue;

        public IDisposable OnChange(Action<T, string?> listener)
        {
            ArgumentNullException.ThrowIfNull(listener);

            lock (_gate)
            {
                _listeners += listener;
            }

            return new Subscription(this, listener);
        }

        public void Update(T value)
        {
            _currentValue = value;

            Action<T, string?>? listeners;
            lock (_gate)
            {
                listeners = _listeners;
            }

            listeners?.Invoke(value, null);
        }

        private void Unsubscribe(Action<T, string?> listener)
        {
            lock (_gate)
            {
                _listeners -= listener;
            }
        }

        private sealed class Subscription : IDisposable
        {
            private TestOptionsMonitor<T>? _monitor;
            private Action<T, string?>? _listener;

            public Subscription(TestOptionsMonitor<T> monitor, Action<T, string?> listener)
            {
                _monitor = monitor;
                _listener = listener;
            }

            public void Dispose()
            {
                var monitor = Interlocked.Exchange(ref _monitor, null);
                var listener = Interlocked.Exchange(ref _listener, null);
                if (monitor is not null && listener is not null)
                {
                    monitor.Unsubscribe(listener);
                }
            }
        }
    }
}
