
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexaCRM.Service.Abstractions.Models.Supabase;
using NexaCRM.Service.Supabase.Configuration;
using NexaCRM.UI.Services.Interfaces;
using LoginResult = NexaCRM.UI.Models.LoginResult;
using LoginFailureReason = NexaCRM.UI.Models.LoginFailureReason;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using Supabase.Postgrest.Exceptions;
using Supabase.Gotrue.Exceptions;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using SupabaseAuthState = Supabase.Gotrue.Constants.AuthState;
using BCrypt.Net;

namespace NexaCRM.Service.Supabase;

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

    private static readonly IReadOnlyDictionary<string, string> DemoAccountAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["manager"] = "manager@nexa.test",
        ["sales"] = "sales@nexa.test",
        ["develop"] = "develop@nexa.test"
    };

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
            await EnsureInitializedAsync().ConfigureAwait(false);

            var publicClient = await _clientProvider.GetClientAsync().ConfigureAwait(false);
            var accountRecord = await ResolveAccountAsync(publicClient, username).ConfigureAwait(false);

            if (accountRecord is null || string.IsNullOrWhiteSpace(accountRecord.Email))
            {
                _logger.LogWarning("Unable to locate account for credential {Username}.", username);
                return LoginResult.Failed(LoginFailureReason.UserNotFound, GetFailureMessage(LoginFailureReason.UserNotFound));
            }

            if (string.IsNullOrWhiteSpace(accountRecord.PasswordHash) || !BCrypt.Net.BCrypt.Verify(password, accountRecord.PasswordHash))
            {
                _logger.LogWarning("Invalid password supplied for {Username}.", username);
                return LoginResult.Failed(LoginFailureReason.InvalidPassword, GetFailureMessage(LoginFailureReason.InvalidPassword));
            }
            
            if (!IsAccountActive(accountRecord))
            {
                _logger.LogWarning(
                    "Login attempt for {Username} blocked due to account status {Status}.",
                    username,
                    accountRecord.Status);

                return LoginResult.Failed(
                    LoginFailureReason.RequiresApproval,
                    GetFailureMessage(LoginFailureReason.RequiresApproval));
            }

            // Create a mock session or use existing session if available
            var session = publicClient.Auth.CurrentSession ?? new Session
            {
                User = new User
                {
                    Id = accountRecord.AuthUserId.ToString(),
                    Email = accountRecord.Email,
                    UserMetadata = new Dictionary<string, object>()
                },
                AccessToken = "mock_token", // You might need to generate a proper token or handle differently
                RefreshToken = "mock_refresh_token"
            };

            await UpdateAuthenticationStateAsync(session).ConfigureAwait(false);
            return LoginResult.Success();
        }
        catch (PostgrestException ex)
        {
            _logger.LogError(ex, "Database error during login for {Username}.", username);
            return LoginResult.Failed(LoginFailureReason.Unknown, GetFailureMessage(LoginFailureReason.Unknown));
        }
        catch (GotrueException ex) when (IsInvalidCredentialError(ex))
        {
            _logger.LogWarning(ex, "Invalid Supabase credentials supplied for {Username}.", username);
            return LoginResult.Failed(LoginFailureReason.InvalidPassword, GetFailureMessage(LoginFailureReason.InvalidPassword));
        }
        catch (GotrueException ex)
        {
            _logger.LogError(ex, "Supabase authentication failed unexpectedly for {Username}.", username);
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
        var serviceKey = _supabaseOptions.Value.AnonKey;

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

        Session? existingSession = null;
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

            // Ensure the Supabase client reflects any sessions persisted by the
            // SupabaseServerSessionPersistence (especially in Development where
            // /test/signin writes the session). RetrieveSessionAsync will consult
            // the configured SessionHandler and populate CurrentSession.
            existingSession = client.Auth.CurrentSession;
            if (existingSession is null)
            {
                try
                {
                    await client.Auth.RetrieveSessionAsync().ConfigureAwait(false);
                    existingSession = client.Auth.CurrentSession;
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to retrieve session from Supabase client during initialization.");
                }
            }
            _initialized = true;
        }
        finally
        {
            _stateLock.Release();
        }

        await UpdateAuthenticationStateAsync(existingSession).ConfigureAwait(false);
    }

    protected virtual async Task HandleAuthStateChangedAsync(IGotrueClient<User, Session> sender, SupabaseAuthState state)
    {
        if (_isDisposed)
        {
            return;
        }

        switch (state)
        {
            case SupabaseAuthState.SignedIn:
                await UpdateAuthenticationStateAsync(sender.CurrentSession).ConfigureAwait(false);
                break;
            case SupabaseAuthState.SignedOut:
                await UpdateAuthenticationStateAsync(null).ConfigureAwait(false);
                break;
            default:
                _logger.LogDebug("Received Supabase auth state notification: {State}", state);
                break;
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
            var client = await _clientProvider.GetClientAsync().ConfigureAwait(false);
            var claims = new List<Claim>();

            if (!Guid.TryParse(session.User.Id, out var authUserId))
            {
                _logger.LogWarning("Supabase session user id {UserId} is not a valid GUID.", session.User.Id);
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            var account = await LoadAccountOverviewAsync(client, authUserId).ConfigureAwait(false);

            if (account is not null)
            {
                var primaryIdentifier = account.AuthUserId != Guid.Empty
                    ? account.AuthUserId.ToString()
                    : session.User.Id;

                claims.Add(new Claim(ClaimTypes.NameIdentifier, primaryIdentifier));

                if (!string.IsNullOrWhiteSpace(account.Cuid))
                {
                    claims.Add(new Claim("cuid", account.Cuid));
                }

                if (!string.IsNullOrWhiteSpace(account.Email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, account.Email));
                }

                if (!string.IsNullOrWhiteSpace(account.FullName))
                {
                    claims.Add(new Claim(ClaimTypes.Name, account.FullName));
                }
                else if (!string.IsNullOrWhiteSpace(account.Username))
                {
                    claims.Add(new Claim(ClaimTypes.Name, account.Username));
                }
                else if (!string.IsNullOrEmpty(session.User.Email))
                {
                    claims.Add(new Claim(ClaimTypes.Name, session.User.Email));
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Name, session.User.Id));
                }

                if (!string.IsNullOrWhiteSpace(account.Username))
                {
                    claims.Add(new Claim("preferred_username", account.Username));
                }

                foreach (var role in account.RoleCodes ?? Array.Empty<string>())
                {
                    if (!string.IsNullOrWhiteSpace(role))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }
            }
            else
            {
                // No account record found for this session. In Development and for
                // test-sessions we still want to produce a usable principal so E2E
                // tests can exercise protected pages. Add basic identity claims
                // derived from the session payload. Role claims may be absent in
                // the session object; tests that need roles can still seed
                // additional localStorage flags or the /test/signin endpoint can
                // be enhanced to include them.
                claims.Add(new Claim(ClaimTypes.NameIdentifier, session.User.Id));
                if (!string.IsNullOrEmpty(session.User.Email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, session.User.Email));
                    claims.Add(new Claim(ClaimTypes.Name, session.User.Email));
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Name, session.User.Id));
                }

                // In Development, add a default 'user' role to allow authorization
                // checks to pass if no explicit roles are present. This keeps test
                // behavior deterministic while not affecting production.
                try
                {
                    // Only attach the default role when running in Development.
                    // Detect environment via optional Environment variable set by the host.
                    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "user"));
                    }
                }
                catch
                {
                    // Swallow any environment read errors; roles are optional.
                }
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "SupabaseAuth"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to construct authentication principal from Supabase session.");
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

    protected virtual async Task<UserAccountOverviewRecord?> ResolveAccountAsync(global::Supabase.Client client, string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return null;
        }

        var normalized = identifier.Trim();

        if (DemoAccountAliases.TryGetValue(normalized, out var aliasEmail))
        {
            normalized = aliasEmail;
        }

        var query = client.From<UserAccountOverviewRecord>();

        var filteredQuery = normalized.Contains('@', StringComparison.Ordinal)
            ? query.Filter(x => x.Email, PostgrestOperator.ILike, normalized)
            : normalized.StartsWith("cuid_", StringComparison.OrdinalIgnoreCase)
                ? query.Filter(x => x.Cuid, PostgrestOperator.Equals, normalized)
                : query.Filter(x => x.Username, PostgrestOperator.ILike, normalized);

        var response = await filteredQuery
            .Limit(1)
            .Get()
            .ConfigureAwait(false);

        return response.Models.FirstOrDefault();
    }

    protected virtual bool IsAccountActive(UserAccountOverviewRecord account)
    {
        return string.Equals(account.Status, "active", StringComparison.OrdinalIgnoreCase);
    }

    protected virtual async Task<UserAccountOverviewRecord?> LoadAccountOverviewAsync(global::Supabase.Client client, Guid authUserId)
    {
        var response = await client.From<UserAccountOverviewRecord>()
            .Filter("auth_user_id", PostgrestOperator.Equals, authUserId.ToString())
            .Limit(1)
            .Get()
            .ConfigureAwait(false);

        var account = response.Models.FirstOrDefault();
        _logger.LogInformation("Loaded account for {AuthUserId}: {@Account}", authUserId, account);
        return account;
    }

    private static bool IsInvalidCredentialError(GotrueException exception)
    {
        if (exception is null)
        {
            return false;
        }

        var message = exception.Message ?? string.Empty;
        return message.Contains("invalid login credentials", StringComparison.OrdinalIgnoreCase)
            || message.Contains("invalid email or password", StringComparison.OrdinalIgnoreCase);
    }
}
