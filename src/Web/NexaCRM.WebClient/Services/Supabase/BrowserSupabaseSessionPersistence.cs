using System;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace NexaCRM.WebClient.Services.Supabase;

/// <summary>
/// Persists Supabase authentication sessions to browser storage.
/// </summary>
public sealed class BrowserSupabaseSessionPersistence : IGotrueSessionPersistence<Session>
{
    private const string StorageKey = "supabase.auth.session";

    private readonly IJSInProcessRuntime? _inProcessRuntime;
    private readonly ILogger<BrowserSupabaseSessionPersistence>? _logger;

    public BrowserSupabaseSessionPersistence(IJSRuntime jsRuntime, ILogger<BrowserSupabaseSessionPersistence>? logger = null)
    {
        _inProcessRuntime = jsRuntime as IJSInProcessRuntime;
        _logger = logger;
    }

    public void SaveSession(Session session)
    {
        if (_inProcessRuntime is null)
        {
            _logger?.LogDebug("In-process JavaScript runtime not available. Skipping session persistence.");
            return;
        }

        ArgumentNullException.ThrowIfNull(session);

        try
        {
            var payload = JsonConvert.SerializeObject(session);
            _inProcessRuntime.InvokeVoid("localStorage.setItem", StorageKey, payload);
        }
        catch (JSException ex)
        {
            _logger?.LogWarning(ex, "Failed to persist Supabase session to localStorage.");
        }
    }

    public Session? LoadSession()
    {
        if (_inProcessRuntime is null)
        {
            return null;
        }

        try
        {
            var raw = _inProcessRuntime.Invoke<string?>("localStorage.getItem", StorageKey);
            if (string.IsNullOrWhiteSpace(raw) || string.Equals(raw, "null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Session>(raw);
        }
        catch (JsonException ex)
        {
            _logger?.LogWarning(ex, "Stored Supabase session payload was invalid. Clearing the cached session.");
            DestroySession();
            return null;
        }
        catch (JSException ex)
        {
            _logger?.LogWarning(ex, "Failed to read Supabase session from localStorage.");
            return null;
        }
    }

    public void DestroySession()
    {
        if (_inProcessRuntime is null)
        {
            return;
        }

        try
        {
            _inProcessRuntime.InvokeVoid("localStorage.removeItem", StorageKey);
        }
        catch (JSException ex)
        {
            _logger?.LogWarning(ex, "Failed to remove Supabase session from localStorage.");
        }
    }
}
