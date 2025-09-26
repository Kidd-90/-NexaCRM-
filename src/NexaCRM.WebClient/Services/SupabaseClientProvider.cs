using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Supabase.Realtime.Exceptions;
using Websocket.Client.Exceptions;

namespace NexaCRM.WebClient.Services;

/// <summary>
/// Coordinates initialization of the Supabase client so that it is executed only once.
/// </summary>
public sealed class SupabaseClientProvider
{
    private readonly Supabase.Client _client;
    private readonly ILogger<SupabaseClientProvider> _logger;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _initialized;

    public SupabaseClientProvider(Supabase.Client client, ILogger<SupabaseClientProvider> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Supabase.Client> GetClientAsync()
    {
        if (_initialized)
        {
            return _client;
        }

        await _initializationLock.WaitAsync();
        try
        {
            if (!_initialized)
            {
                try
                {
                    await _client.InitializeAsync();
                }
                catch (Exception ex) when (IsRealtimePlatformNotSupported(ex))
                {
                    _logger.LogWarning(ex, "Supabase realtime sockets are not supported in this environment. Continuing without realtime subscriptions.");
                    await _client.Auth.RetrieveSessionAsync();
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
