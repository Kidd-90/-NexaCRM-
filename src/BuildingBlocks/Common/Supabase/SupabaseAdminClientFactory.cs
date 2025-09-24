using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Common.Options;
using Microsoft.Extensions.Logging;
using Supabase;
using Supabase.Gotrue;

namespace BuildingBlocks.Common.Supabase;

/// <summary>
/// Default implementation that constructs <see cref="Client"/> instances for privileged server use.
/// </summary>
public sealed class SupabaseAdminClientFactory : ISupabaseAdminClientFactory
{
    private readonly ILogger<SupabaseAdminClientFactory>? _logger;

    public SupabaseAdminClientFactory(ILogger<SupabaseAdminClientFactory>? logger = null)
    {
        _logger = logger;
    }

    public async Task<Client> CreateClientAsync(
        SupabaseServerOptions options,
        SupabaseOptions supabaseOptions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(supabaseOptions);

        options.Validate();

        var client = new Client(options.Url!, options.ServiceRoleKey!, supabaseOptions);

        try
        {
            await client.InitializeAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize Supabase admin client.");
            throw;
        }

        return client;
    }
}
