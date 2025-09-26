using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace NexaCRM.WebServer.Services;

/// <summary>
/// Provides a session-backed implementation of <see cref="IGotrueSessionPersistence{TSession}"/> for server-hosted environments.
/// </summary>
public sealed class ServerSupabaseSessionPersistence : IGotrueSessionPersistence<Session>
{
    private const string SessionItemName = "Supabase.Session";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServerSupabaseSessionPersistence(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public void SaveSession(Session session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var context = GetHttpContext();
        var payload = JsonSerializer.Serialize(session, SerializerOptions);
        context.Session.SetString(SessionItemName, payload);
    }

    public Session? LoadSession()
    {
        var context = GetHttpContext();
        var payload = context.Session.GetString(SessionItemName);
        if (string.IsNullOrEmpty(payload))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Session>(payload, SerializerOptions);
        }
        catch (JsonException)
        {
            context.Session.Remove(SessionItemName);
            return null;
        }
    }

    public void DestroySession()
    {
        var context = GetHttpContext();
        context.Session.Remove(SessionItemName);
    }

    private HttpContext GetHttpContext()
    {
        return _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("An active HttpContext is required to access the session.");
    }
}
