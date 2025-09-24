using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildingBlocks.Common.Supabase.Data.Contacts;
using Microsoft.Extensions.Logging.Abstractions;
using Supabase;
using Xunit;

namespace BuildingBlocks.Common.UnitTests;

public class SupabaseContactRepositoryTests
{
    [Fact]
    public async Task GetContactsAsync_ReturnsContactsFromFetcher()
    {
        var contactId = Guid.NewGuid();
        var repository = new SupabaseContactRepository(
            _ => Task.FromResult(CreateClient()),
            NullLogger<SupabaseContactRepository>.Instance,
            (_, _) => Task.FromResult<IReadOnlyList<SupabaseContactRecord>>(new[]
            {
                new SupabaseContactRecord { Id = contactId, FirstName = "Jane", LastName = "Doe" }
            }));

        var results = await repository.GetContactsAsync();

        Assert.Single(results);
        Assert.Equal(contactId, results[0].Id);
        Assert.Equal("Jane", results[0].FirstName);
    }

    [Fact]
    public async Task GetContactsAsync_ReturnsEmptyWhenFetcherReturnsNull()
    {
        var repository = new SupabaseContactRepository(
            _ => Task.FromResult(CreateClient()),
            NullLogger<SupabaseContactRepository>.Instance,
            (_, _) => Task.FromResult<IReadOnlyList<SupabaseContactRecord>>(null!));

        var results = await repository.GetContactsAsync();

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetContactsAsync_ThrowsWhenClientUnavailable()
    {
        var repository = new SupabaseContactRepository(
            _ => Task.FromResult<Client>(null!),
            NullLogger<SupabaseContactRepository>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.GetContactsAsync());
    }

    [Fact]
    public async Task GetContactsAsync_WrapsFetcherExceptions()
    {
        var expected = new InvalidOperationException("boom");
        var repository = new SupabaseContactRepository(
            _ => Task.FromResult(CreateClient()),
            NullLogger<SupabaseContactRepository>.Instance,
            (_, _) => throw expected);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.GetContactsAsync());
        Assert.Equal("Failed to retrieve contacts from Supabase.", exception.Message);
        Assert.Same(expected, exception.InnerException);
    }

    private static Client CreateClient() => new("https://example.supabase.co", "service-role", new SupabaseOptions());
}
