using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace NexaCRM.WebClient.ServerHost.Services;

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

    public Task PersistSession(Session session)
    {
        ArgumentNullException.ThrowIfNull(session);
        var context = GetHttpContext();
        var payload = JsonSerializer.Serialize(session, SerializerOptions);
        context.Session.SetString(SessionItemName, payload);
        return Task.CompletedTask;
    }

    public Task<Session?> RetrieveSession()
    {
        var context = GetHttpContext();
        var payload = context.Session.GetString(SessionItemName);
        if (string.IsNullOrEmpty(payload))
        {
            return Task.FromResult<Session?>(null);
        }

        try
        {
            var session = JsonSerializer.Deserialize<Session>(payload, SerializerOptions);
            return Task.FromResult(session);
        }
        catch (JsonException)
        {
            context.Session.Remove(SessionItemName);
            return Task.FromResult<Session?>(null);
        }
    }

    public Task DestroySession()
    {
        var context = GetHttpContext();
        context.Session.Remove(SessionItemName);
        return Task.CompletedTask;
    }

    private HttpContext GetHttpContext()
    {
        return _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("An active HttpContext is required to access the session.");
    }
}
