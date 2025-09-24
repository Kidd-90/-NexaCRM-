using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using NexaCRM.WebClient.Models.FileHub;
using NexaCRM.WebClient.Models.Supabase;
using NexaCRM.WebClient.Services;
using Xunit;

namespace NexaCRM.WebClient.UnitTests;

public sealed class SupabaseAdvancedServicesTests
{
    [Fact]
    public void MapToDomain_ParsesMetadataDictionary()
    {
        var record = new UserAccountRecord
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            DisplayName = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            MetadataJson = "{\"PreferredLanguage\":\"ko-KR\"}"
        };

        var method = typeof(SupabaseUserGovernanceService)
            .GetMethod("MapToDomain", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var result = method!.Invoke(null, new object[] { record, new List<string> { "admin" } });
        Assert.NotNull(result);

        var metadataProperty = result!.GetType().GetProperty("Metadata");
        Assert.NotNull(metadataProperty);

        var metadata = metadataProperty!.GetValue(result) as IReadOnlyDictionary<string, string>;
        Assert.NotNull(metadata);
        Assert.Equal("ko-KR", metadata!["PreferredLanguage"]);
    }

    [Fact]
    public void BuildObjectPath_ComposesDeterministicSegments()
    {
        var request = new FileUploadRequest
        {
            EntityType = "deal",
            EntityId = "123",
            FileName = "Quote.PDF"
        };

        var method = typeof(SupabaseFileHubService)
            .GetMethod("BuildObjectPath", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var path = (string)method!.Invoke(null, new object[] { request })!;

        Assert.Contains("deal/123", path, StringComparison.Ordinal);
        Assert.EndsWith("quote.pdf", path, StringComparison.Ordinal);

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        Assert.True(segments.Length >= 5);

        var dateSegment = string.Join('/', segments[2], segments[3], segments[4]);
        Assert.True(DateTime.TryParseExact(
            dateSegment,
            "yyyy/MM/dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _));
    }

    [Fact]
    public void DeserializeConfiguration_ReturnsDictionary()
    {
        const string json = "{\"widget\":\"pipeline\",\"size\":\"large\"}";

        var method = typeof(SupabaseSettingsCustomizationService)
            .GetMethod("DeserializeConfiguration", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var configuration = method!.Invoke(null, new object[] { json });
        Assert.NotNull(configuration);

        var dictionary = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(configuration);
        Assert.Equal("pipeline", dictionary["widget"]);
        Assert.Equal("large", dictionary["size"]);
    }
}
