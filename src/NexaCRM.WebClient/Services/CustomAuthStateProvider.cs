using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using NexaCRM.UI.Models.Supabase;
using NexaCRM.UI.Services.Interfaces;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;
using Supabase.Postgrest.Exceptions;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using SupabaseAuthState = Supabase.Gotrue.Constants.AuthState;
using LoginFailureReason = NexaCRM.UI.Models.LoginFailureReason;
using LoginResult = NexaCRM.UI.Models.LoginResult;

namespace NexaCRM.WebClient.Services;

public sealed class CustomAuthStateProvider : AuthenticationStateProvider, IAuthenticationService, IAsyncDisposable
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<CustomAuthStateProvider> _logger;
    private readonly SemaphoreSlim _stateLock = new(1, 1);

    private AuthenticationState _currentState =
        new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

    private bool _initialized;
    private IGotrueClient<User, Session>.AuthEventHandler? _authEventHandler;

    public CustomAuthStateProvider(SupabaseClientProvider clientProvider, ILogger<CustomAuthStateProvider> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public bool IsAuthenticated => _currentState.User.Identity?.IsAuthenticated ?? false;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await EnsureInitializedAsync();
        return _currentState;
    }

    public async Task<LoginResult> SignInAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return LoginResult.Failed(LoginFailureReason.MissingUsername, GetFailureMessage(LoginFailureReason.MissingUsername));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return LoginResult.Failed(LoginFailureReason.MissingPassword, GetFailureMessage(LoginFailureReason.MissingPassword));
        }

        var client = await _clientProvider.GetClientAsync();

        try
        {
            var session = await client.Auth.SignInWithPassword(email, password);
            if (session is null)
            {
                var fallbackReason = await DetermineCredentialFailureAsync(client, email);
                return LoginResult.Failed(fallbackReason, GetFailureMessage(fallbackReason));
            }

            var isApproved = await IsUserApprovedAsync(client, session);
            if (!isApproved)
            {
                await client.Auth.SignOut();
                return LoginResult.Failed(LoginFailureReason.RequiresApproval, GetFailureMessage(LoginFailureReason.RequiresApproval));
            }

            await UpdateAuthenticationStateAsync(session);
            return LoginResult.Success();
        }
        catch (GotrueException ex)
        {
            _logger.LogWarning(ex, "Supabase rejected login for {Email}.", email);
            var reason = await MapGotrueExceptionAsync(client, ex, email);
            return LoginResult.Failed(reason, GetFailureMessage(reason));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected Supabase login failure for {Email}.", email);
            return LoginResult.Failed(LoginFailureReason.Unknown, GetFailureMessage(LoginFailureReason.Unknown));
        }
    }

    public async Task LogoutAsync()
    {
        var client = await _clientProvider.GetClientAsync();
        await client.Auth.SignOut();
        await UpdateAuthenticationStateAsync(null);
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _stateLock.WaitAsync();
        try
        {
            if (_initialized)
            {
                return;
            }

            var client = await _clientProvider.GetClientAsync();
            _authEventHandler = async (sender, state) => await HandleAuthStateChangedAsync(sender, state);
            client.Auth.AddStateChangedListener(_authEventHandler);

            await UpdateAuthenticationStateAsync(client.Auth.CurrentSession);
            _initialized = true;
        }
        finally
        {
            _stateLock.Release();
        }
    }

    private async Task HandleAuthStateChangedAsync(IGotrueClient<User, Session> sender, SupabaseAuthState state)
    {
        try
        {
            await UpdateAuthenticationStateAsync(sender.CurrentSession);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle Supabase auth state change: {AuthState}.", state);
        }
    }

    private async Task UpdateAuthenticationStateAsync(Session? session)
    {
        await _stateLock.WaitAsync();
        try
        {
            var principal = await BuildPrincipalAsync(session);
            _currentState = new AuthenticationState(principal);
        }
        finally
        {
            _stateLock.Release();
        }

        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    private async Task<ClaimsPrincipal> BuildPrincipalAsync(Session? session)
    {
        if (session?.User?.Id is null)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, session.User.Id)
            };

            if (!string.IsNullOrEmpty(session.User.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, session.User.Email));
                claims.Add(new Claim(ClaimTypes.Name, session.User.Email));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Name, session.User.Id));
            }

            foreach (var role in await LoadRolesAsync(client, session.User.Id))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Supabase"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to construct authentication principal from Supabase session.");
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }

    private async Task<IEnumerable<string>> LoadRolesAsync(Supabase.Client client, string userId)
    {
        var response = await client.From<UserRoleRecord>()
            .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
            .Get();

        var roles = new List<string>();
        foreach (var record in response.Models)
        {
            if (!string.IsNullOrWhiteSpace(record.RoleCode))
            {
                roles.Add(record.RoleCode);
            }
        }

        return roles;
    }

    private static string GetFailureMessage(LoginFailureReason reason)
    {
        return reason switch
        {
            LoginFailureReason.MissingUsername => "아이디를 입력해주세요.",
            LoginFailureReason.MissingPassword => "비밀번호를 입력해주세요.",
            LoginFailureReason.UserNotFound => "입력하신 아이디를 찾을 수 없습니다.",
            LoginFailureReason.InvalidPassword => "비밀번호가 일치하지 않습니다. 다시 확인해주세요.",
            LoginFailureReason.RequiresApproval => "관리자 승인 대기 중인 계정입니다. 승인이 완료된 후 다시 시도해주세요.",
            _ => "로그인 중 오류가 발생했습니다. 다시 시도해주세요."
        };
    }

    private async Task<LoginFailureReason> MapGotrueExceptionAsync(Supabase.Client client, GotrueException exception, string email)
    {
        if (IsUserNotFoundError(exception))
        {
            return LoginFailureReason.UserNotFound;
        }

        if (IsInvalidPasswordError(exception))
        {
            return LoginFailureReason.InvalidPassword;
        }

        var fallback = await DetermineCredentialFailureAsync(client, email);
        return fallback == LoginFailureReason.Unknown
            ? LoginFailureReason.Unknown
            : fallback;
    }

    private static bool IsUserNotFoundError(GotrueException exception)
    {
        if (exception is null)
        {
            return false;
        }

        var errorText = ExtractErrorText(exception);
        return ContainsAny(errorText, "user_not_found", "user not found", "email not found");
    }

    private static bool IsInvalidPasswordError(GotrueException exception)
    {
        if (exception is null)
        {
            return false;
        }

        var errorText = ExtractErrorText(exception);
        return ContainsAny(errorText, "invalid_grant", "invalid login credentials", "invalid password");
    }

    private static string? ExtractErrorText(GotrueException exception)
    {
        if (!string.IsNullOrWhiteSpace(exception.Message))
        {
            return exception.Message;
        }

        if (!string.IsNullOrWhiteSpace(exception.InnerException?.Message))
        {
            return exception.InnerException.Message;
        }

        return null;
    }

    private static bool ContainsAny(string? value, params string[] candidates)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        foreach (var candidate in candidates)
        {
            if (value.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private async Task<LoginFailureReason> DetermineCredentialFailureAsync(Supabase.Client client, string email)
    {
        try
        {
            var response = await client.From<ProfileLookupRecord>()
                .Select("id, username")
                .Filter(x => x.Username, PostgrestOperator.Equals, email)
                .Limit(1)
                .Get();

            if (response.Models?.Any() == true)
            {
                return LoginFailureReason.InvalidPassword;
            }

            return LoginFailureReason.UserNotFound;
        }
        catch (PostgrestException ex)
        {
            _logger.LogWarning(ex, "Failed to determine credential failure reason for {Email} due to Postgrest exception.", email);
            return LoginFailureReason.Unknown;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to determine credential failure reason for {Email}.", email);
            return LoginFailureReason.Unknown;
        }
    }

    private async Task<bool> IsUserApprovedAsync(Supabase.Client client, Session session)
    {
        if (session.User?.Id is null)
        {
            return false;
        }

        if (!Guid.TryParse(session.User.Id, out var userId))
        {
            _logger.LogWarning("Supabase session user id was not a valid GUID: {UserId}.", session.User.Id);
            return false;
        }

        var response = await client.From<OrganizationUserRecord>()
            .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
            .Limit(1)
            .Get();

        var membership = response.Models?.FirstOrDefault();
        if (membership is null)
        {
            _logger.LogInformation("User {UserId} attempted login without organization membership record.", userId);
            return false;
        }

        var isApproved = string.Equals(membership.Status, "approved", StringComparison.OrdinalIgnoreCase);
        if (!isApproved)
        {
            _logger.LogInformation("User {UserId} attempted login with status {Status}.", userId, membership.Status);
        }

        return isApproved;
    }

    public async ValueTask DisposeAsync()
    {
        if (_authEventHandler is null)
        {
            return;
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            client.Auth.RemoveStateChangedListener(_authEventHandler);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove Supabase auth event handler during disposal.");
        }
        finally
        {
            _authEventHandler = null;
        }
    }
}
