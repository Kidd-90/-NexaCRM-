using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace NexaCRM.WebServer.Services;

public sealed class SupabaseServerSessionPersistence : IGotrueSessionPersistence<Session>
{
    private readonly ILogger<SupabaseServerSessionPersistence> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    // Singleton으로 등록된 경우를 위한 전역 세션 저장소
    private static readonly ConcurrentDictionary<string, Session> _globalSessions = new();
    
    // Scoped로 등록된 경우를 위한 인스턴스 세션
    private Session? _session;

    public SupabaseServerSessionPersistence(
        ILogger<SupabaseServerSessionPersistence> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor;
    }

    public void SaveSession(Session session)
    {
        ArgumentNullException.ThrowIfNull(session);
        
        // 1. 인스턴스에 저장
        _session = session;
        
        // 2. 전역 저장소에 저장 (Circuit 간 공유를 위해)
        if (!string.IsNullOrEmpty(session.User?.Id))
        {
            _globalSessions[session.User.Id] = session;
            _logger.LogInformation("✅ Supabase session saved to global memory for user {UserId}", session.User?.Id);
        }
        
        _logger.LogDebug("Supabase session persisted for user {UserId}.", session.User?.Id);
    }

    public void DestroySession()
    {
        var userId = _session?.User?.Id;
        _session = null;
        
        // 전역 저장소에서 제거
        if (!string.IsNullOrEmpty(userId))
        {
            _globalSessions.TryRemove(userId, out _);
            _logger.LogInformation("🗑️ Supabase session removed from global memory for user {UserId}", userId);
        }
        
        _logger.LogDebug("Supabase session destroyed.");
    }

    public Session? LoadSession()
    {
        // 현재 Circuit의 세션 반환
        if (_session != null)
        {
            _logger.LogDebug("Returning existing Supabase session from memory.");
            return _session;
        }
        
        // 다른 Circuit에서 저장된 세션이 있는지 확인 (제한적)
        // 참고: forceLoad 사용 시 새 Circuit이 생성되어 복원 불가
        _logger.LogDebug("No Supabase session found in current Circuit.");
        return null;
    }
}
