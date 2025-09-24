using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Supabase;

namespace BuildingBlocks.Common.Supabase.Data.Contacts;

/// <summary>
/// Executes PostgREST queries against the Supabase <c>contacts</c> table.
/// </summary>
public sealed class SupabaseContactRepository : ISupabaseContactRepository
{
    private readonly Func<CancellationToken, Task<Client>> _clientResolver;
    private readonly Func<Client, CancellationToken, Task<IReadOnlyList<SupabaseContactRecord>>> _contactFetcher;
    private readonly ILogger<SupabaseContactRepository>? _logger;

    /// <summary>
    /// Initializes the repository using the provided <see cref="SupabaseAdminClientProvider"/>.
    /// </summary>
    public SupabaseContactRepository(
        SupabaseAdminClientProvider clientProvider,
        ILogger<SupabaseContactRepository>? logger = null)
        : this(
            clientProvider is null ? throw new ArgumentNullException(nameof(clientProvider)) : clientProvider.GetClientAsync,
            logger)
    {
    }

    /// <summary>
    /// Initializes the repository with explicit delegates. Intended for testing.
    /// </summary>
    public SupabaseContactRepository(
        Func<CancellationToken, Task<Client>> clientResolver,
        ILogger<SupabaseContactRepository>? logger = null,
        Func<Client, CancellationToken, Task<IReadOnlyList<SupabaseContactRecord>>>? contactFetcher = null)
    {
        _clientResolver = clientResolver ?? throw new ArgumentNullException(nameof(clientResolver));
        _logger = logger;
        _contactFetcher = contactFetcher ?? FetchContactsAsync;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SupabaseContactRecord>> GetContactsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await _clientResolver(cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Supabase admin client is not available.");

            var contacts = await _contactFetcher(client, cancellationToken).ConfigureAwait(false);
            return contacts ?? Array.Empty<SupabaseContactRecord>();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to retrieve contacts from Supabase.");
            throw new InvalidOperationException("Failed to retrieve contacts from Supabase.", ex);
        }
    }

    private static async Task<IReadOnlyList<SupabaseContactRecord>> FetchContactsAsync(
        Client client,
        CancellationToken cancellationToken)
    {
        var response = await client
            .From<SupabaseContactRecord>()
            .Get(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return response.Models ?? Array.Empty<SupabaseContactRecord>();
    }
}
