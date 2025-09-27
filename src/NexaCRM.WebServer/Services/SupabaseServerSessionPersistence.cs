using System;
using Microsoft.Extensions.Logging;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace NexaCRM.WebServer.Services;

public sealed class SupabaseServerSessionPersistence : IGotrueSessionPersistence<Session>
{
    private readonly ILogger<SupabaseServerSessionPersistence> _logger;
    private Session? _session;

    public SupabaseServerSessionPersistence(ILogger<SupabaseServerSessionPersistence> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void SaveSession(Session session)
    {
        ArgumentNullException.ThrowIfNull(session);
        _session = session;
        _logger.LogDebug("Supabase session persisted for user {UserId}.", session.User?.Id);
    }

    public void DestroySession()
    {
        _session = null;
        _logger.LogDebug("Supabase session destroyed.");
    }

    public Session? LoadSession() => _session;
}
