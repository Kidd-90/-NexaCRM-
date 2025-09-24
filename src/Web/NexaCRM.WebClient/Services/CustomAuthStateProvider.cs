using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NexaCRM.WebClient.Services.Supabase;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;

namespace NexaCRM.WebClient.Services;

public sealed class CustomAuthStateProvider : AuthenticationStateProvider, IAsyncDisposable
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<CustomAuthStateProvider>? _logger;

    private Client? _client;
    private IGotrueClient<User, Session>.AuthEventHandler? _authStateListener;
    private AuthenticationState _cachedState = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public CustomAuthStateProvider(
        SupabaseClientProvider clientProvider,
        IJSRuntime jsRuntime,
        ILogger<CustomAuthStateProvider>? logger = null)
    {
        _clientProvider = clientProvider ?? throw new ArgumentNullException(nameof(clientProvider));
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var client = await EnsureClientAsync().ConfigureAwait(false);
        var principal = BuildPrincipal(client.Auth.CurrentUser, client.Auth.CurrentSession);
        _cachedState = new AuthenticationState(principal);
        return _cachedState;
    }

    public async Task<SupabaseSignInResult> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.", nameof(password));
        }

        var client = await EnsureClientAsync().ConfigureAwait(false);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var session = await client.Auth.SignInWithPassword(email, password).ConfigureAwait(false);
            if (session?.User is null)
            {
                return new SupabaseSignInResult(false, session, "사용자 인증에 실패했습니다.");
            }

            await PersistLegacyStateAsync(session.User).ConfigureAwait(false);
            var principal = BuildPrincipal(session.User, session);
            _cachedState = new AuthenticationState(principal);
            NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));

            return new SupabaseSignInResult(true, session, null);
        }
        catch (GotrueException ex)
        {
            _logger?.LogWarning(ex, "Supabase sign-in failed: {Message}", ex.Message);
            return new SupabaseSignInResult(false, null, ex.Message);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error during Supabase sign-in.");
            return new SupabaseSignInResult(false, null, "로그인 중 오류가 발생했습니다.");
        }
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        var client = await EnsureClientAsync().ConfigureAwait(false);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await client.Auth.SignOut(Supabase.Gotrue.Constants.SignOutScope.Global).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Supabase sign-out encountered an error.");
        }
        finally
        {
            await ClearLegacyStateAsync().ConfigureAwait(false);
            _cachedState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
        }
    }

    public bool IsAuthenticated => _cachedState.User.Identity?.IsAuthenticated ?? false;

    private async Task<Client> EnsureClientAsync()
    {
        if (_client is not null)
        {
            return _client;
        }

        var client = await _clientProvider.GetClientAsync().ConfigureAwait(false);
        _client = client;

        if (_authStateListener is null)
        {
            _authStateListener = (sender, state) => HandleAuthStateChanged(sender, state);
            client.Auth.AddStateChangedListener(_authStateListener);
        }

        return client;
    }

    private void HandleAuthStateChanged(IGotrueClient<User, Session> sender, Supabase.Gotrue.Constants.AuthState state)
    {
        _ = HandleAuthStateChangedAsync(sender, state);
    }

    private async Task HandleAuthStateChangedAsync(IGotrueClient<User, Session> sender, Supabase.Gotrue.Constants.AuthState state)
    {
        try
        {
            if (state == Supabase.Gotrue.Constants.AuthState.SignedIn && sender.CurrentUser is not null)
            {
                await PersistLegacyStateAsync(sender.CurrentUser).ConfigureAwait(false);
            }
            else if (state == Supabase.Gotrue.Constants.AuthState.SignedOut)
            {
                await ClearLegacyStateAsync().ConfigureAwait(false);
            }

            var principal = BuildPrincipal(sender.CurrentUser, sender.CurrentSession);
            _cachedState = new AuthenticationState(principal);
            NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to process Supabase auth state change.");
        }
    }

    private static ClaimsPrincipal BuildPrincipal(User? user, Session? session)
    {
        if (user is null)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var identity = new ClaimsIdentity("Supabase");

        if (!string.IsNullOrWhiteSpace(user.Id))
        {
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
        }

        var displayName = !string.IsNullOrWhiteSpace(user.Email)
            ? user.Email
            : !string.IsNullOrWhiteSpace(user.Phone) ? user.Phone : user.Id;

        if (!string.IsNullOrWhiteSpace(displayName))
        {
            identity.AddClaim(new Claim(ClaimTypes.Name, displayName));
        }

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
        }

        foreach (var role in ExtractRoles(user))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        if (session?.AccessToken is { Length: > 0 })
        {
            identity.AddClaim(new Claim("access_token", session.AccessToken));
        }

        if (session?.RefreshToken is { Length: > 0 })
        {
            identity.AddClaim(new Claim("refresh_token", session.RefreshToken));
        }

        return new ClaimsPrincipal(identity);
    }

    private static IEnumerable<string> ExtractRoles(User user)
    {
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(user.Role))
        {
            roles.Add(user.Role);
        }

        if (user.AppMetadata is IReadOnlyDictionary<string, object> metadata)
        {
            AddRolesFromMetadata(metadata, "roles", roles);
            AddRolesFromMetadata(metadata, "role", roles);
            AddRolesFromMetadata(metadata, "default_role", roles);
        }

        return roles;
    }

    private static void AddRolesFromMetadata(IReadOnlyDictionary<string, object> metadata, string key, HashSet<string> roles)
    {
        if (!metadata.TryGetValue(key, out var value) || value is null)
        {
            return;
        }

        foreach (var role in ParseRoleValues(value))
        {
            if (!string.IsNullOrWhiteSpace(role))
            {
                roles.Add(role);
            }
        }
    }

    private static IEnumerable<string> ParseRoleValues(object value)
    {
        switch (value)
        {
            case string str:
                {
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        yield break;
                    }

                    if (str.TrimStart().StartsWith("[") && str.TrimEnd().EndsWith("]"))
                    {
                        try
                        {
                            foreach (var token in JArray.Parse(str))
                            {
                                if (token.Type == JTokenType.String)
                                {
                                    var text = token.Value<string>();
                                    if (!string.IsNullOrWhiteSpace(text))
                                    {
                                        yield return text;
                                    }
                                }
                            }
                        }
                        catch (JsonException)
                        {
                        }

                        yield break;
                    }

                    yield return str;
                    yield break;
                }
            case IEnumerable enumerable:
                foreach (var item in enumerable)
                {
                    switch (item)
                    {
                        case string strItem when !string.IsNullOrWhiteSpace(strItem):
                            yield return strItem;
                            break;
                        case JValue jValue when jValue.Type == JTokenType.String:
                            var text = jValue.Value<string>();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                yield return text;
                            }
                            break;
                    }
                }

                yield break;
            default:
                yield break;
        }
    }

    private async Task PersistLegacyStateAsync(User user)
    {
        try
        {
            var username = !string.IsNullOrWhiteSpace(user.Email) ? user.Email : user.Id ?? "supabase-user";
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "username", username).ConfigureAwait(false);

            var roles = ExtractRoles(user).ToArray();
            var serializedRoles = JsonSerializer.Serialize(roles);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "roles", serializedRoles).ConfigureAwait(false);

            var isDeveloper = roles.Any(role => string.Equals(role, "Developer", StringComparison.OrdinalIgnoreCase));
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "isDeveloper", isDeveloper ? "true" : "false").ConfigureAwait(false);

            await _jsRuntime.InvokeVoidAsync(
                "eval",
                "if (window.authManager && window.authManager.resetSessionTimeout) { window.authManager.resetSessionTimeout(); }").ConfigureAwait(false);
        }
        catch (JSException ex)
        {
            _logger?.LogWarning(ex, "Failed to synchronize legacy authentication state.");
        }
    }

    private async Task ClearLegacyStateAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "username").ConfigureAwait(false);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "roles").ConfigureAwait(false);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "isDeveloper").ConfigureAwait(false);
            await _jsRuntime.InvokeVoidAsync(
                "eval",
                "if (window.authManager && window.authManager.sessionTimeoutId) { clearTimeout(window.authManager.sessionTimeoutId); window.authManager.sessionTimeoutId = null; }").ConfigureAwait(false);
        }
        catch (JSException ex)
        {
            _logger?.LogWarning(ex, "Failed to clear legacy authentication storage.");
        }
    }

    public ValueTask DisposeAsync()
    {
        if (_client is not null && _authStateListener is not null)
        {
            try
            {
                _client.Auth.RemoveStateChangedListener(_authStateListener);
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Failed to remove Supabase auth state listener.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
