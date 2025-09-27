
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexaCRM.UI.Models.Supabase;
using NexaCRM.Service.Supabase.Configuration;
using NexaCRM.UI.Services.Interfaces;
using Supabase.Postgrest.Models;
using LoginResult = NexaCRM.UI.Models.LoginResult;
using LoginFailureReason = NexaCRM.UI.Models.LoginFailureReason;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Exceptions;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using SupabaseAuthState = Supabase.Gotrue.Constants.AuthState;

namespace NexaCRM.Service.Supabase;

// NOTE: YOU MUST PROVIDE THE CORRECT TABLE AND COLUMN NAMES
// I am assuming:
// Table: "profiles"
// Columns: "id" (uuid), "username" (text), "password_hash" (text), "email" (text)
[Table("app_users")]
public class UserAuthRecord : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("username")]
    public string? Username { get; set; }

    [Column("password_hash")]
    public string? PasswordHash { get; set; }

    [Column("email")]
    public string? Email { get; set; }
}


public class SupabaseAuthenticationStateProvider : AuthenticationStateProvider, IAuthenticationService, IAsyncDisposable
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseAuthenticationStateProvider> _logger;
    private readonly IOptions<SupabaseClientOptions> _supabaseOptions;
    private readonly SemaphoreSlim _stateLock = new(1, 1);

    private AuthenticationState _currentState =
        new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

    private bool _initialized;
    private IGotrueClient<User, Session>.AuthEventHandler? _authEventHandler;
    private global::Supabase.Client? _subscribedClient;
    private bool _isDisposed;

    public SupabaseAuthenticationStateProvider(
        SupabaseClientProvider clientProvider,
        ILogger<SupabaseAuthenticationStateProvider> logger,
        IOptions<SupabaseClientOptions> supabaseOptions)
    {
        _clientProvider = clientProvider ?? throw new ArgumentNullException(nameof(clientProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _supabaseOptions = supabaseOptions ?? throw new ArgumentNullException(nameof(supabaseOptions));
    }

    public bool IsAuthenticated => _currentState.User.Identity?.IsAuthenticated ?? false;

    protected ILogger Logger => _logger;

    protected SupabaseClientProvider ClientProvider => _clientProvider;

    protected bool IsDisposed => _isDisposed;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await EnsureInitializedAsync().ConfigureAwait(false);
        return _currentState;
    }

    public virtual async Task<LoginResult> SignInAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return LoginResult.Failed(LoginFailureReason.MissingUsername, GetFailureMessage(LoginFailureReason.MissingUsername));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return LoginResult.Failed(LoginFailureReason.MissingPassword, GetFailureMessage(LoginFailureReason.MissingPassword));
        }

        try
        {
            var serviceClient = await GetServiceClientAsync();

            // NOTE: Assumes the login form uses a 'username' field. If it's email, change the column name.
            var response = await serviceClient.From<UserAuthRecord>()
                .Filter(x => x.Username, PostgrestOperator.Equals, username)
                .Limit(1)
                .Get();

            var userRecord = response.Models.FirstOrDefault();

            if (userRecord?.PasswordHash is null)
            {
                _logger.LogWarning("User not found or no password hash for {Username}.", username);
                return LoginResult.Failed(LoginFailureReason.UserNotFound, GetFailureMessage(LoginFailureReason.UserNotFound));
            }

            if (!BCrypt.Net.BCrypt.Verify(password, userRecord.PasswordHash))
            {
                _logger.LogWarning("Invalid password attempt for {Username}.", username);
                return LoginResult.Failed(LoginFailureReason.InvalidPassword, GetFailureMessage(LoginFailureReason.InvalidPassword));
            }

            // We have a valid user. Now check if they are approved in the system.
            // This uses the public client, but could also use the service client.
            var publicClient = await _clientProvider.GetClientAsync().ConfigureAwait(false);
            var isApproved = await IsUserApprovedAsync(publicClient, userRecord.Id).ConfigureAwait(false);
            if (!isApproved)
            {
                return LoginResult.Failed(LoginFailureReason.RequiresApproval, GetFailureMessage(LoginFailureReason.RequiresApproval));
            }

            // Manually create a session-like object to build the principal
            var session = new Session
            {
                User = new User { Id = userRecord.Id.ToString(), Email = userRecord.Email }
                // Note: AccessToken is not set as we are not using Supabase RLS for this user.
            };

            await UpdateAuthenticationStateAsync(session).ConfigureAwait(false);
            return LoginResult.Success();
        }
        catch (PostgrestException ex)
        {
            _logger.LogError(ex, "Database error during login for {Username}.", username);
            return LoginResult.Failed(LoginFailureReason.Unknown, GetFailureMessage(LoginFailureReason.Unknown));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected login failure for {Username}.", username);
            return LoginResult.Failed(LoginFailureReason.Unknown, GetFailureMessage(LoginFailureReason.Unknown));
        }
    }

    public virtual async Task LogoutAsync()
    {
        // Even with custom auth, we might want to clear the Supabase session if one was ever created.
        var client = await _clientProvider.GetClientAsync().ConfigureAwait(false);
        try
        {
            await client.Auth.SignOut().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during Supabase sign out on custom logout. This might be expected.");
        }
        finally
        {
            await UpdateAuthenticationStateAsync(null).ConfigureAwait(false);
        }
    }

    protected virtual async Task<global::Supabase.Client> GetServiceClientAsync()
    {
        var url = _supabaseOptions.Value.Url;
        var serviceKey = _supabaseOptions.Value.ServiceKey;

        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(serviceKey))
        {
            _logger.LogError("Supabase URL or ServiceKey is not configured.");
            throw new InvalidOperationException("Supabase client is not configured for service operations.");
        }

        var options = new global::Supabase.SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true,
        };

        var client = new global::Supabase.Client(url, serviceKey, options);
        await client.InitializeAsync();
        return client;
    }


    protected async Task EnsureInitializedAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _stateLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_initialized)
            {
                return;
            }

            var client = await _clientProvider.GetClientAsync().ConfigureAwait(false);
            _subscribedClient = client;
            // We keep this handler to allow for potential future integration with Supabase-driven auth events
            _authEventHandler = async (sender, state) => await HandleAuthStateChangedAsync(sender, state).ConfigureAwait(false);
            client.Auth.AddStateChangedListener(_authEventHandler);

            // With custom auth, the initial state is always unauthenticated from Supabase's perspective
            await UpdateAuthenticationStateAsync(null).ConfigureAwait(false);
            _initialized = true;
        }
        finally
        {
            _stateLock.Release();
        }
    }

    protected virtual async Task HandleAuthStateChangedAsync(IGotrueClient<User, Session> sender, SupabaseAuthState state)
    {
        if (_isDisposed)
        {
            return;
        }

        // This handler might be invoked if other parts of the app use Supabase auth functions.
        // We need to decide if a Supabase-driven auth change should override our custom app login.
        // For now, we log it and do not automatically change the state if the user is already authenticated.
        if (state == SupabaseAuthState.SignedOut && IsAuthenticated)
        {
            _logger.LogInformation("Supabase session was signed out, but user remains logged in via custom authentication.");
            // To force a full logout, you could call: await UpdateAuthenticationStateAsync(null);
        }
    }

    protected async Task UpdateAuthenticationStateAsync(Session? session)
    {
        await _stateLock.WaitAsync().ConfigureAwait(false);
        try
        {
            var principal = await BuildPrincipalAsync(session).ConfigureAwait(false);
            _currentState = new AuthenticationState(principal);
        }
        finally
        {
            _stateLock.Release();
        }

        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    protected virtual async Task<ClaimsPrincipal> BuildPrincipalAsync(Session? session)
    {
        if (session?.User?.Id is null)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        try
        {
            // Use the public client for loading roles, as this should be subject to RLS.
            var client = await _clientProvider.GetClientAsync().ConfigureAwait(false);
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

            foreach (string role in await LoadRolesAsync(client, session.User.Id).ConfigureAwait(false))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "CustomAppAuth"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to construct authentication principal from custom session.");
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
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

    protected virtual async Task<IEnumerable<string>> LoadRolesAsync(global::Supabase.Client client, string userId)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            return Enumerable.Empty<string>();
        }

        var response = await client.From<UserRoleRecord>()
            .Filter(x => x.UserId, PostgrestOperator.Equals, userGuid)
            .Get()
            .ConfigureAwait(false);

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

    protected virtual async Task<bool> IsUserApprovedAsync(global::Supabase.Client client, Guid userId)
    {
        var response = await client.From<OrganizationUserRecord>()
            .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
            .Limit(1)
            .Get()
            .ConfigureAwait(false);

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

    public virtual async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;

        var handler = _authEventHandler;
        var client = Interlocked.Exchange(ref _subscribedClient, null);
        _authEventHandler = null;

        if (handler is null || client is null)
        {
            return;
        }

        try
        {
            client.Auth.RemoveStateChangedListener(handler);
        }
        catch (ObjectDisposedException)
        {
            _logger.LogDebug("Supabase client was already disposed during auth provider disposal.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove Supabase auth event handler during disposal.");
        }
    }
}
