using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Models.Supabase;
using NexaCRM.WebClient.Services.Interfaces;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseContactService : IContactService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseContactService> _logger;

    public SupabaseContactService(SupabaseClientProvider clientProvider, ILogger<SupabaseContactService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<IEnumerable<Contact>> GetContactsAsync()
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<ContactRecord>()
                .Order(x => x.LastName, PostgrestOrdering.Ascending)
                .Get();

            var models = response.Models ?? new List<ContactRecord>();
            if (models.Count == 0)
            {
                return new List<Contact>();
            }

            return models.Select(MapToContact).ToList()!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load contacts from Supabase.");
            throw;
        }
    }

    public async Task<Contact> CreateContactAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        if (contact is null)
        {
            throw new ArgumentNullException(nameof(contact));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var record = new ContactRecord
            {
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Email = contact.Email,
                Phone = contact.PhoneNumber,
                CompanyName = contact.Company,
                Title = contact.Title
            };

            var response = await client.From<ContactRecord>()
                .Insert(record, cancellationToken: cancellationToken);

            var created = response.Models.FirstOrDefault() ?? record;
            return MapToContact(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create contact in Supabase.");
            throw;
        }
    }

    private static Contact MapToContact(ContactRecord record)
    {
        return new Contact
        {
            Id = record.Id,
            FirstName = record.FirstName,
            LastName = record.LastName,
            Email = record.Email,
            PhoneNumber = record.Phone,
            Company = record.CompanyName,
            Title = record.Title
        };
    }
}
