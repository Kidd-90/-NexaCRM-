using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Common.Options;
using BuildingBlocks.Common.Supabase;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Supabase;
using Supabase.Gotrue;
using Xunit;

namespace BuildingBlocks.Common.UnitTests;

public class SupabaseAdminClientProviderTests
{
    [Fact]
    public async Task GetClientAsync_ReturnsCachedInstance()
    {
        var optionsMonitor = new OptionsMonitorStub<SupabaseServerOptions>(new SupabaseServerOptions
        {
            Url = "https://example.supabase.co",
            ServiceRoleKey = "service-role",
            JwtSecret = "jwt-secret"
        });

        var factory = new FakeAdminClientFactory();
        var provider = new SupabaseAdminClientProvider(optionsMonitor, factory, NullLogger<SupabaseAdminClientProvider>.Instance);

        var first = await provider.GetClientAsync();
        var second = await provider.GetClientAsync();

        Assert.Same(first, second);
        Assert.Equal(1, factory.InvocationCount);
        Assert.False(factory.CapturedOptions.AutoRefreshToken);
        Assert.False(factory.CapturedOptions.AutoConnectRealtime);
        Assert.Equal(1, factory.Client.InitializeCallCount);
    }

    [Fact]
    public async Task GetClientAsync_PropagatesValidationErrors()
    {
        var optionsMonitor = new OptionsMonitorStub<SupabaseServerOptions>(new SupabaseServerOptions());
        var factory = new SupabaseAdminClientFactory(NullLogger<SupabaseAdminClientFactory>.Instance);
        var provider = new SupabaseAdminClientProvider(optionsMonitor, factory, NullLogger<SupabaseAdminClientProvider>.Instance);

        await Assert.ThrowsAsync<ValidationException>(() => provider.GetClientAsync());
    }

    private sealed class FakeAdminClientFactory : ISupabaseAdminClientFactory
    {
        public int InvocationCount { get; private set; }
        public SupabaseOptions CapturedOptions { get; private set; } = new SupabaseOptions();
        public TestSupabaseClient Client { get; } = new();

        public async Task<Client> CreateClientAsync(SupabaseServerOptions options, SupabaseOptions supabaseOptions, CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            CapturedOptions = supabaseOptions;
            await Client.InitializeAsync().ConfigureAwait(false);
            return Client;
        }
    }

    private sealed class TestSupabaseClient : Client
    {
        public int InitializeCallCount { get; private set; }

        public TestSupabaseClient()
            : base("https://example.supabase.co", "key", new SupabaseOptions())
        {
        }

        public override Task InitializeAsync()
        {
            InitializeCallCount++;
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
