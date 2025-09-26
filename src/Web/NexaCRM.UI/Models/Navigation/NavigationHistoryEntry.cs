using System;
using System.Text.Json.Serialization;

namespace NexaCRM.WebClient.Models.Navigation;

public sealed record NavigationHistoryEntry(
    [property: JsonPropertyName("href")] string Href,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("icon")] string IconCssClass,
    [property: JsonPropertyName("timestampUtc")] DateTime TimestampUtc)
{
    public static NavigationHistoryEntry Create(string href, string title, string iconCssClass)
    {
        var normalizedHref = string.IsNullOrWhiteSpace(href) ? string.Empty : href.Trim('/');
        return new NavigationHistoryEntry(normalizedHref, title, iconCssClass, DateTime.UtcNow);
    }
}
