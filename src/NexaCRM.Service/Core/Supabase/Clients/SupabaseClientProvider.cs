using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using global::Supabase.Realtime.Exceptions;
using Websocket.Client.Exceptions;

namespace NexaCRM.Service.Supabase;

public sealed class SupabaseClientProvider
{
    private readonly global::Supabase.Client _client;
    private readonly ILogger<SupabaseClientProvider> _logger;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _initialized;

    public SupabaseClientProvider(global::Supabase.Client client, ILogger<SupabaseClientProvider> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<global::Supabase.Client> GetClientAsync()
    {
        if (_initialized)
        {
            return _client;
        }

        await _initializationLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!_initialized)
            {
                try
                {
                    await _client.InitializeAsync().ConfigureAwait(false);
                }
                catch (Exception ex) when (IsRealtimePlatformNotSupported(ex))
                {
                    _logger.LogWarning(ex, "Supabase realtime sockets are not supported in this environment. Continuing without realtime subscriptions.");
                    await _client.Auth.RetrieveSessionAsync().ConfigureAwait(false);
                }

                _initialized = true;
            }
        }
        finally
        {
            _initializationLock.Release();
        }

        return _client;
    }

    private static bool IsRealtimePlatformNotSupported(Exception exception)
    {
        switch (exception)
        {
            case PlatformNotSupportedException:
                return true;
            case RealtimeException realtime when realtime.InnerException is not null:
                return IsRealtimePlatformNotSupported(realtime.InnerException);
            case WebsocketException websocket when websocket.InnerException is not null:
                return IsRealtimePlatformNotSupported(websocket.InnerException);
            default:
                return false;
        }
    }
}