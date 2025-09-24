using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.WebClient.Options;
using Supabase;
using Supabase.Gotrue;

namespace NexaCRM.WebClient.Services.Supabase;

public sealed class SupabaseClientFactory : ISupabaseClientFactory
{
    private readonly ILogger<SupabaseClientFactory>? _logger;

    public SupabaseClientFactory(ILogger<SupabaseClientFactory>? logger = null)
    {
        _logger = logger;
    }

    public async Task<Client> CreateClientAsync(SupabaseClientOptions configuration, SupabaseOptions supabaseOptions, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(supabaseOptions);

        var client = new Client(configuration.Url!, configuration.AnonKey!, supabaseOptions);

        try
        {
            await client.InitializeAsync().ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize Supabase client for the web application.");
            throw;
        }

        return client;
    }
}
