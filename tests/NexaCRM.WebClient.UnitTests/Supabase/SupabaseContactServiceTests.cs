using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Common.Supabase.Data.Contacts;
using Microsoft.Extensions.Logging.Abstractions;
using NexaCRM.WebClient.Services.Supabase;
using Xunit;

namespace NexaCRM.WebClient.UnitTests.Supabase;

public class SupabaseContactServiceTests
{
    [Fact]
    public async Task GetContactsAsync_MapsRecordsToContacts()
    {
        var records = new List<SupabaseContactRecord>
        {
            new() { Id = Guid.NewGuid(), FirstName = "Ada", LastName = "Lovelace", Email = "ada@example.com", Phone = "123" },
            new() { Id = Guid.NewGuid(), FirstName = "Grace", LastName = "Hopper", Email = "grace@example.com", Phone = "456" }
        };

        var service = new SupabaseContactService(
            _ => Task.FromResult<IReadOnlyList<SupabaseContactRecord>>(records),
            NullLogger<SupabaseContactService>.Instance);

        var contacts = (await service.GetContactsAsync()).ToList();

        Assert.Equal(2, contacts.Count);
        Assert.Equal(1, contacts[0].Id);
        Assert.Equal(records[0].Id, contacts[0].SupabaseId);
        Assert.Equal("Ada", contacts[0].FirstName);
        Assert.Equal("Lovelace", contacts[0].LastName);
        Assert.Equal("ada@example.com", contacts[0].Email);
        Assert.Equal("123", contacts[0].PhoneNumber);
        Assert.Equal(2, contacts[1].Id);
        Assert.Equal(records[1].Id, contacts[1].SupabaseId);
    }

    [Fact]
    public async Task GetContactsAsync_ReturnsEmptyWhenNoRecords()
    {
        var service = new SupabaseContactService(
            _ => Task.FromResult<IReadOnlyList<SupabaseContactRecord>>(Array.Empty<SupabaseContactRecord>()),
            NullLogger<SupabaseContactService>.Instance);

        var contacts = await service.GetContactsAsync();

        Assert.Empty(contacts);
    }

    [Fact]
    public async Task GetContactsAsync_WrapsFetcherExceptions()
    {
        var service = new SupabaseContactService(
            _ => throw new InvalidOperationException("boom"),
            NullLogger<SupabaseContactService>.Instance);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetContactsAsync());
        Assert.Equal("Failed to load contacts from Supabase.", exception.Message);
        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Equal("boom", exception.InnerException?.Message);
    }
}
