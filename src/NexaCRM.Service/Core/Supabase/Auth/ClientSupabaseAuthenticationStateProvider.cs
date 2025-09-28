using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexaCRM.Service.Supabase.Configuration;
using LoginResult = NexaCRM.UI.Models.LoginResult;
using Supabase.Gotrue;
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
        var result = await base.SignInAsync(username, password).ConfigureAwait(false);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Username} successfully signed in via shared auth pipeline.", username);
        }

        return result;
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
