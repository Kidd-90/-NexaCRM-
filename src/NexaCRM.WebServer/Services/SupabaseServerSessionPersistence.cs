using System;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace NexaCRM.WebServer.Services;

public sealed class SupabaseServerSessionPersistence : IGotrueSessionPersistence<Session>
{
    private readonly ILogger<SupabaseServerSessionPersistence> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string SessionCookieName = "NexaCRM.SupabaseSession";
    
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
        
        // 3. 쿠키에 저장 (브라우저 새로고침 대응)
        SaveSessionToCookie(session);
        
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
        
        // 쿠키에서 제거
        DeleteSessionCookie();
        
        _logger.LogDebug("Supabase session destroyed.");
    }

    public Session? LoadSession()
    {
        // 1. 현재 Circuit의 세션 반환
        if (_session != null)
        {
            _logger.LogDebug("Returning existing Supabase session from memory.");
            return _session;
        }
        
        // 2. 쿠키에서 세션 복원 시도 (브라우저 새로고침 대응)
        var sessionFromCookie = LoadSessionFromCookie();
        if (sessionFromCookie != null)
        {
            _session = sessionFromCookie;
            
            // 전역 저장소에도 복원
            if (!string.IsNullOrEmpty(sessionFromCookie.User?.Id))
            {
                _globalSessions[sessionFromCookie.User.Id] = sessionFromCookie;
                _logger.LogInformation("🔄 Supabase session restored from cookie for user {UserId}", sessionFromCookie.User?.Id);
            }
            
            return sessionFromCookie;
        }
        
        _logger.LogDebug("No Supabase session found in current Circuit or cookie.");
        return null;
    }
    
    private void SaveSessionToCookie(Session session)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogWarning("⚠️ HttpContext is null, cannot save session to cookie");
                return;
            }
            
            // Session을 JSON으로 직렬화
            var sessionJson = JsonSerializer.Serialize(session);
            
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = session.ExpiresAt(),
                Path = "/"
            };
            
            httpContext.Response.Cookies.Append(SessionCookieName, sessionJson, cookieOptions);
            _logger.LogDebug("✅ Session saved to cookie for user {UserId}", session.User?.Id);
        }
        catch (InvalidOperationException ex)
        {
            // Headers already sent - 이 경우는 무시 (Circuit 재연결 시 발생 가능)
            _logger.LogDebug("⚠️ Could not save session to cookie (headers already sent): {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to save session to cookie");
        }
    }
    
    private Session? LoadSessionFromCookie()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogDebug("HttpContext is null, cannot load session from cookie");
                return null;
            }

            if (!httpContext.Request.Cookies.TryGetValue(SessionCookieName, out var sessionJson) ||
                string.IsNullOrEmpty(sessionJson))
            {
                _logger.LogDebug("No session cookie found");
                return null;
            }

            // Attempt to parse cookie safely
            try
            {
                var toParse = sessionJson;

                // Cookie value may be a quoted JSON string — try to unwrap first
                if (!string.IsNullOrEmpty(toParse) && toParse[0] == '"')
                {
                    try
                    {
                        toParse = JsonSerializer.Deserialize<string>(toParse) ?? toParse;
                    }
                    catch
                    {
                        // ignore and use original cookie string
                    }
                }

                var session = JsonSerializer.Deserialize<Session>(toParse);

                if (session == null)
                {
                    _logger.LogWarning("Failed to deserialize session from cookie. Raw: {RawCookie}", sessionJson);
                    return null;
                }

                // 세션 만료 확인
                if (session.ExpiresAt() < DateTimeOffset.UtcNow)
                {
                    _logger.LogInformation("Session cookie expired for user {UserId}", session.User?.Id);
                    DeleteSessionCookie();
                    return null;
                }

                _logger.LogDebug("✅ Session loaded from cookie for user {UserId}", session.User?.Id);
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to load session from cookie; raw cookie: {RawCookie}", sessionJson);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to load session from cookie");
            return null;
        }
    }
    
    private void DeleteSessionCookie()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogDebug("HttpContext is null, cannot delete session cookie");
                return;
            }
            
            httpContext.Response.Cookies.Delete(SessionCookieName);
            _logger.LogDebug("🗑️ Session cookie deleted");
        }
        catch (InvalidOperationException ex)
        {
            // Headers already sent - 무시
            _logger.LogDebug("⚠️ Could not delete session cookie (headers already sent): {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to delete session cookie");
        }
    }
}
