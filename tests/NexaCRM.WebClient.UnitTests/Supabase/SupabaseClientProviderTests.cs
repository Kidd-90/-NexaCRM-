using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NexaCRM.WebClient.Options;
using NexaCRM.WebClient.Services.Supabase;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using Xunit;

namespace NexaCRM.WebClient.UnitTests.Supabase;

public class SupabaseClientProviderTests
{
    [Fact]
    public async Task GetClientAsync_ReturnsCachedClient()
    {
        var optionsMonitor = new OptionsMonitorStub<SupabaseClientOptions>(new SupabaseClientOptions
        {
            Url = "https://example.supabase.co",
            AnonKey = "anon-key"
        });

        var sessionPersistence = new FakeSessionPersistence();
        var factory = new FakeSupabaseClientFactory();
        var provider = new SupabaseClientProvider(optionsMonitor, sessionPersistence, factory, NullLogger<SupabaseClientProvider>.Instance);

        var first = await provider.GetClientAsync();
        var second = await provider.GetClientAsync();

        Assert.Same(first, second);
        Assert.Equal(1, factory.InvocationCount);
        Assert.True(factory.CapturedOptions.AutoRefreshToken);
        Assert.True(factory.CapturedOptions.AutoConnectRealtime);
        Assert.Same(sessionPersistence, factory.CapturedOptions.SessionHandler);
        Assert.Equal(1, factory.Client.InitializeCount);
    }

    [Fact]
    public async Task GetClientAsync_InvalidConfiguration_Throws()
    {
        var optionsMonitor = new OptionsMonitorStub<SupabaseClientOptions>(new SupabaseClientOptions
        {
            Url = "",
            AnonKey = null
        });

        var provider = new SupabaseClientProvider(optionsMonitor, new FakeSessionPersistence(), new FakeSupabaseClientFactory(), NullLogger<SupabaseClientProvider>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetClientAsync());
    }

    private sealed class FakeSessionPersistence : IGotrueSessionPersistence<Session>
    {
        public Session? StoredSession { get; private set; }

        public void SaveSession(Session session)
        {
            StoredSession = session;
        }

        public Session? LoadSession()
        {
            return StoredSession;
        }

        public void DestroySession()
        {
            StoredSession = null;
        }
    }

    private sealed class FakeSupabaseClientFactory : ISupabaseClientFactory
    {
        public int InvocationCount { get; private set; }

        public SupabaseOptions CapturedOptions { get; private set; } = new();

        public TestSupabaseClient Client { get; } = new();

        public async Task<Client> CreateClientAsync(SupabaseClientOptions configuration, SupabaseOptions supabaseOptions, CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            CapturedOptions = supabaseOptions;
            await Client.InitializeAsync().ConfigureAwait(false);
            return Client;
        }
    }

    private sealed class TestSupabaseClient : Client
    {
        public int InitializeCount { get; private set; }

        public TestSupabaseClient()
            : base("https://example.supabase.co", "anon", new SupabaseOptions())
        {
        }

        public override Task InitializeAsync()
        {
            InitializeCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class OptionsMonitorStub<T> : IOptionsMonitor<T> where T : class
    {
        private readonly T _value;

        public OptionsMonitorStub(T value)
        {
            _value = value;
        }

        public T CurrentValue => _value;

        public T Get(string? name) => _value;

        public IDisposable OnChange(Action<T, string?> listener) => new NoopDisposable();

        private sealed class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
