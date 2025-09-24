using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Common.Supabase.Data.Contacts;
using Microsoft.Extensions.Logging;
using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Services.Interfaces;
using Supabase.Postgrest.Constants;

namespace NexaCRM.WebClient.Services.Supabase;

/// <summary>
/// Retrieves contact information from Supabase for the Blazor WebAssembly client.
/// </summary>
public sealed class SupabaseContactService : IContactService
{
    private readonly SupabaseClientProvider? _clientProvider;
    private readonly Func<CancellationToken, Task<IReadOnlyList<SupabaseContactRecord>>> _contactFetcher;
    private readonly ILogger<SupabaseContactService>? _logger;

    /// <summary>
    /// Creates a service that resolves records through the configured <see cref="SupabaseClientProvider"/>.
    /// </summary>
    public SupabaseContactService(
        SupabaseClientProvider clientProvider,
        ILogger<SupabaseContactService>? logger = null)
    {
        _clientProvider = clientProvider ?? throw new ArgumentNullException(nameof(clientProvider));
        _logger = logger;
        _contactFetcher = FetchContactsAsync;
    }

    /// <summary>
    /// Creates a service that uses a custom fetch delegate. Primarily used for testing.
    /// </summary>
    public SupabaseContactService(
        Func<CancellationToken, Task<IReadOnlyList<SupabaseContactRecord>>> contactFetcher,
        ILogger<SupabaseContactService>? logger = null)
    {
        _contactFetcher = contactFetcher ?? throw new ArgumentNullException(nameof(contactFetcher));
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Contact>> GetContactsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var records = await _contactFetcher(cancellationToken).ConfigureAwait(false);
            if (records is null || records.Count == 0)
            {
                return Array.Empty<Contact>();
            }

            var contacts = new List<Contact>(records.Count);
            for (var index = 0; index < records.Count; index++)
            {
                var record = records[index];
                contacts.Add(MapToContact(record, index + 1));
            }

            return contacts;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load contacts from Supabase.");
            throw new InvalidOperationException("Failed to load contacts from Supabase.", ex);
        }
    }

    private async Task<IReadOnlyList<SupabaseContactRecord>> FetchContactsAsync(CancellationToken cancellationToken)
    {
        if (_clientProvider is null)
        {
            throw new InvalidOperationException("Supabase client provider is not configured.");
        }

        var client = await _clientProvider.GetClientAsync(cancellationToken).ConfigureAwait(false);
        var response = await client
            .From<SupabaseContactRecord>()
            .Order("last_name", Ordering.Ascending)
            .Get(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return response.Models ?? Array.Empty<SupabaseContactRecord>();
    }

    private static Contact MapToContact(SupabaseContactRecord record, int index)
    {
        var contact = new Contact
        {
            Id = index,
            SupabaseId = record.Id,
            FirstName = record.FirstName,
            LastName = record.LastName,
            Email = record.Email,
            PhoneNumber = record.Phone,
            Company = null,
            Title = record.Title
        };

        return contact;
    }
}
