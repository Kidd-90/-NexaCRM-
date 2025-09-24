using Microsoft.Extensions.Options;
using Supabase;

namespace BuildingBlocks.Common.Supabase;

/// <summary>
/// Default implementation of <see cref="ISupabaseClientFactory"/> that caches service and anon clients
/// while still allowing ad-hoc client creation for specialised workloads.
/// </summary>
public sealed class SupabaseClientFactory : ISupabaseClientFactory, IDisposable
{
    private readonly IOptionsMonitor<SupabaseSettings> _settingsMonitor;
    private readonly IDisposable? _reloadToken;
    private Client? _cachedServiceClient;
    private Client? _cachedAnonClient;
    private readonly object _serviceSync = new();
    private readonly object _anonSync = new();

    public SupabaseClientFactory(IOptionsMonitor<SupabaseSettings> settingsMonitor)
    {
        _settingsMonitor = settingsMonitor ?? throw new ArgumentNullException(nameof(settingsMonitor));
        _reloadToken = _settingsMonitor.OnChange(_ => ResetCache());
    }

    /// <inheritdoc />
    public Client GetServiceClient()
    {
        if (_cachedServiceClient is { } serviceClient)
        {
            return serviceClient;
        }

        lock (_serviceSync)
        {
            if (_cachedServiceClient is not null)
            {
                return _cachedServiceClient;
            }

            var settings = _settingsMonitor.CurrentValue;
            var apiKey = settings.ServiceRoleKey;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Supabase ServiceRoleKey must be configured before requesting the service client.");
            }

            _cachedServiceClient = CreateClientInternal(apiKey, settings, null);
            return _cachedServiceClient;
        }
    }

    /// <inheritdoc />
    public Client GetAnonClient()
    {
        if (_cachedAnonClient is { } anonClient)
        {
            return anonClient;
        }

        lock (_anonSync)
        {
            if (_cachedAnonClient is not null)
            {
                return _cachedAnonClient;
            }

            var settings = _settingsMonitor.CurrentValue;
            var apiKey = settings.AnonKey;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Supabase AnonKey must be configured before requesting the anon client.");
            }

            _cachedAnonClient = CreateClientInternal(apiKey, settings, null);
            return _cachedAnonClient;
        }
    }

    /// <inheritdoc />
    public Client CreateClient(string apiKey, Action<global::Supabase.SupabaseOptions>? configure = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("API key must be provided when creating a Supabase client.", nameof(apiKey));
        }

        var settings = _settingsMonitor.CurrentValue;
        return CreateClientInternal(apiKey, settings, configure);
    }

    private static Client CreateClientInternal(string apiKey, SupabaseSettings settings, Action<global::Supabase.SupabaseOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var url = settings.Url;
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException("Supabase Url must be configured before creating a client.");
        }

        var normalizedUrl = url.Trim();
        var normalizedApiKey = apiKey.Trim();
        var clientSettings = settings.Client ?? new SupabaseClientOptions();

        var options = new global::Supabase.SupabaseOptions
        {
            AutoConnectRealtime = clientSettings.AutoConnectRealtime,
            AutoRefreshToken = clientSettings.AutoRefreshToken
        };

        configure?.Invoke(options);

        return new Client(normalizedUrl, normalizedApiKey, options);
    }

    private void ResetCache()
    {
        lock (_serviceSync)
        {
            _cachedServiceClient = null;
        }

        lock (_anonSync)
        {
            _cachedAnonClient = null;
        }
    }

    public void Dispose()
    {
        _reloadToken?.Dispose();
        GC.SuppressFinalize(this);
    }
}
