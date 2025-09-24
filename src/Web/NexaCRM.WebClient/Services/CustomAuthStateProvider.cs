using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using NexaCRM.WebClient.Models.Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using SupabaseAuthState = Supabase.Gotrue.Constants.AuthState;

namespace NexaCRM.WebClient.Services;

public sealed class CustomAuthStateProvider : AuthenticationStateProvider, IAsyncDisposable
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

    public async Task<bool> SignInAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.", nameof(password));
        }

        var client = await _clientProvider.GetClientAsync();

        var session = await client.Auth.SignInWithPassword(email, password);
        if (session is null)
        {
            return false;
        }

        await UpdateAuthenticationStateAsync(session);
        return true;
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
