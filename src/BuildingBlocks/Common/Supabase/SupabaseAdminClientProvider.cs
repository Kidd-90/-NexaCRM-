using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Supabase;
using Supabase.Gotrue;

namespace BuildingBlocks.Common.Supabase;

/// <summary>
/// Provides a lazily initialized Supabase client configured with the service role key for admin scenarios.
/// </summary>
public sealed class SupabaseAdminClientProvider
{
    private readonly IOptionsMonitor<SupabaseServerOptions> _optionsMonitor;
    private readonly ISupabaseAdminClientFactory _clientFactory;
    private readonly ILogger<SupabaseAdminClientProvider>? _logger;
    private readonly SemaphoreSlim _clientLock = new(1, 1);

    private Client? _client;

    public SupabaseAdminClientProvider(
        IOptionsMonitor<SupabaseServerOptions> optionsMonitor,
        ISupabaseAdminClientFactory clientFactory,
        ILogger<SupabaseAdminClientProvider>? logger = null)
    {
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _logger = logger;
    }

    /// <summary>
    /// Returns a cached Supabase client instance or initializes one if necessary.
    /// </summary>
    public async Task<Client> GetClientAsync(CancellationToken cancellationToken = default)
    {
        if (_client is not null)
        {
            return _client;
        }

        await _clientLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_client is not null)
            {
                return _client;
            }

            var options = _optionsMonitor.CurrentValue ?? throw new InvalidOperationException("Supabase configuration is not bound.");

            var supabaseOptions = new SupabaseOptions
            {
                AutoRefreshToken = false,
                AutoConnectRealtime = false
            };

            _client = await _clientFactory.CreateClientAsync(options, supabaseOptions, cancellationToken).ConfigureAwait(false);
            return _client;
        }
        finally
        {
            _clientLock.Release();
        }
    }
}
