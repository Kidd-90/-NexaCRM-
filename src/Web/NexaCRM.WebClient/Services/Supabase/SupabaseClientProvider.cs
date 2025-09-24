using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexaCRM.WebClient.Options;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace NexaCRM.WebClient.Services.Supabase;

public sealed class SupabaseClientProvider
{
    private readonly IOptionsMonitor<SupabaseClientOptions> _optionsMonitor;
    private readonly IGotrueSessionPersistence<Session> _sessionPersistence;
    private readonly ISupabaseClientFactory _clientFactory;
    private readonly ILogger<SupabaseClientProvider>? _logger;
    private readonly SemaphoreSlim _clientLock = new(1, 1);

    private Client? _client;

    public SupabaseClientProvider(
        IOptionsMonitor<SupabaseClientOptions> optionsMonitor,
        IGotrueSessionPersistence<Session> sessionPersistence,
        ISupabaseClientFactory clientFactory,
        ILogger<SupabaseClientProvider>? logger = null)
    {
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        _sessionPersistence = sessionPersistence ?? throw new ArgumentNullException(nameof(sessionPersistence));
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _logger = logger;
    }

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

            var configuration = _optionsMonitor.CurrentValue ?? throw new InvalidOperationException("Supabase client configuration is missing.");
            ValidateOptions(configuration);

            var supabaseOptions = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true,
                SessionHandler = _sessionPersistence
            };

            _client = await _clientFactory.CreateClientAsync(configuration, supabaseOptions, cancellationToken).ConfigureAwait(false);
            return _client;
        }
        finally
        {
            _clientLock.Release();
        }
    }

    private static void ValidateOptions(SupabaseClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Url))
        {
            throw new InvalidOperationException("Supabase URL must be configured.");
        }

        if (string.IsNullOrWhiteSpace(options.AnonKey))
        {
            throw new InvalidOperationException("Supabase anon key must be configured.");
        }
    }
}
