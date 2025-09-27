using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexaCRM.Service.Supabase.Configuration;
using LoginResult = NexaCRM.UI.Models.LoginResult;
using LoginFailureReason = NexaCRM.UI.Models.LoginFailureReason;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;
using SupabaseAuthState = Supabase.Gotrue.Constants.AuthState;

namespace NexaCRM.Service.Supabase;

public sealed class ClientSupabaseAuthenticationStateProvider : SupabaseAuthenticationStateProvider
{
    private readonly ILogger<ClientSupabaseAuthenticationStateProvider> _logger;

    public ClientSupabaseAuthenticationStateProvider(
        SupabaseClientProvider clientProvider,
        ILogger<SupabaseAuthenticationStateProvider> baseLogger,
        IOptions<SupabaseClientOptions> supabaseOptions,
        ILogger<ClientSupabaseAuthenticationStateProvider> logger)
        : base(clientProvider, baseLogger, supabaseOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<LoginResult> SignInAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return LoginResult.Failed(LoginFailureReason.MissingUsername, "Username is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return LoginResult.Failed(LoginFailureReason.MissingPassword, "Password is required.");
        }

        await EnsureInitializedAsync().ConfigureAwait(false);

        var client = await ClientProvider.GetClientAsync().ConfigureAwait(false);

        try
        {
            var session = await client.Auth.SignIn(username, password).ConfigureAwait(false);

            if (session?.User is null)
            {
                return LoginResult.Failed(LoginFailureReason.InvalidPassword, "Invalid login credentials.");
            }

            _logger.LogInformation("User {UserId} successfully signed in via client auth.", session.User.Id);
            await UpdateAuthenticationStateAsync(session).ConfigureAwait(false);
            return LoginResult.Success();
        }
        catch (GotrueException ex)
        {
            _logger.LogError(ex, "Login failed for user {Username}.", username);
            return LoginResult.Failed(LoginFailureReason.Unknown, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign-in for {Username}.", username);
            return LoginResult.Failed(LoginFailureReason.Unknown, "An unexpected error occurred.");
        }
    }

    protected override async Task HandleAuthStateChangedAsync(IGotrueClient<User, Session> sender, SupabaseAuthState state)
    {
        if (IsDisposed)
        {
            return;
        }

        await UpdateAuthenticationStateAsync(sender.CurrentSession).ConfigureAwait(false);
        await base.HandleAuthStateChangedAsync(sender, state).ConfigureAwait(false);
    }
}
