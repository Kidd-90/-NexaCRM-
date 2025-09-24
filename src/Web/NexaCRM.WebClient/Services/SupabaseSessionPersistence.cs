using System;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace NexaCRM.WebClient.Services;

/// <summary>
/// Provides persistence for Supabase sessions using the browser's local storage.
/// </summary>
public sealed class SupabaseSessionPersistence : IGotrueSessionPersistence<Session>
{
    private const string StorageKey = "nexacrm.supabase.session";

    private readonly IJSRuntime _jsRuntime;
    private readonly IJSInProcessRuntime? _jsInProcessRuntime;
    private readonly ILogger<SupabaseSessionPersistence> _logger;

    public SupabaseSessionPersistence(IJSRuntime jsRuntime, ILogger<SupabaseSessionPersistence> logger)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _jsInProcessRuntime = jsRuntime as IJSInProcessRuntime;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void SaveSession(Session session)
    {
        if (session is null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        try
        {
            var payload = JsonConvert.SerializeObject(session);

            if (_jsInProcessRuntime is not null)
            {
                _jsInProcessRuntime.InvokeVoid("localStorage.setItem", StorageKey, payload);
            }
            else
            {
                _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, payload).AsTask().GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist Supabase session.");
        }
    }

    public void DestroySession()
    {
        try
        {
            if (_jsInProcessRuntime is not null)
            {
                _jsInProcessRuntime.InvokeVoid("localStorage.removeItem", StorageKey);
            }
            else
            {
                _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey).AsTask().GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove Supabase session from local storage.");
        }
    }

    public Session? LoadSession()
    {
        try
        {
            string? rawSession;

            if (_jsInProcessRuntime is not null)
            {
                rawSession = _jsInProcessRuntime.Invoke<string?>("localStorage.getItem", StorageKey);
            }
            else
            {
                rawSession = _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey).AsTask().GetAwaiter()
                    .GetResult();
            }

            if (string.IsNullOrWhiteSpace(rawSession))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Session>(rawSession);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to restore Supabase session from local storage.");
            return null;
        }
    }
}
