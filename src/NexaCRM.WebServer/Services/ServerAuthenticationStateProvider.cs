using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using NexaCRM.UI.Services.Interfaces;
using LoginFailureReason = NexaCRM.UI.Models.LoginFailureReason;
using LoginResult = NexaCRM.UI.Models.LoginResult;

namespace NexaCRM.WebServer.Services;

public sealed class ServerAuthenticationStateProvider : AuthenticationStateProvider, IAuthenticationService
{
    private readonly ILogger<ServerAuthenticationStateProvider> _logger;
    private readonly ConcurrentDictionary<string, UserAccount> _accounts;
    private AuthenticationState _currentState = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public ServerAuthenticationStateProvider(ILogger<ServerAuthenticationStateProvider> logger)
    {
        _logger = logger;
        _accounts = new ConcurrentDictionary<string, UserAccount>(StringComparer.OrdinalIgnoreCase)
        {
            ["manager@nexacrm.com"] = new UserAccount("manager@nexacrm.com", "Password123!", new[] { "Manager", "Admin" }),
            ["sales@nexacrm.com"] = new UserAccount("sales@nexacrm.com", "Password123!", new[] { "Sales" }),
            ["developer@nexacrm.com"] = new UserAccount("developer@nexacrm.com", "Password123!", new[] { "Developer", "Admin" })
        };
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(_currentState);
    }

    public Task<LoginResult> SignInAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Task.FromResult(LoginResult.Failed(LoginFailureReason.MissingUsername, "아이디를 입력해주세요."));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult(LoginResult.Failed(LoginFailureReason.MissingPassword, "비밀번호를 입력해주세요."));
        }

        if (!_accounts.TryGetValue(username, out var account))
        {
            _logger.LogInformation("Unknown username {Username} attempted to sign in.", username);
            return Task.FromResult(LoginResult.Failed(LoginFailureReason.UserNotFound, "등록되지 않은 계정입니다."));
        }

        if (!string.Equals(password, account.Password, StringComparison.Ordinal))
        {
            _logger.LogInformation("Invalid password supplied for {Username}.", username);
            return Task.FromResult(LoginResult.Failed(LoginFailureReason.InvalidPassword, "비밀번호가 올바르지 않습니다."));
        }

        var identity = new ClaimsIdentity("ServerAuth");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, account.Username));
        identity.AddClaim(new Claim(ClaimTypes.Name, account.Username));

        foreach (var role in account.Roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        var principal = new ClaimsPrincipal(identity);
        _currentState = new AuthenticationState(principal);
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));

        return Task.FromResult(LoginResult.Success());
    }

    public Task LogoutAsync()
    {
        _currentState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
        return Task.CompletedTask;
    }

    private sealed record UserAccount(string Username, string Password, IReadOnlyList<string> Roles);
}
