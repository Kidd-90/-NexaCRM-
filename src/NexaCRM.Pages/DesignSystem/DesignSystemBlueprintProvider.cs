using System;
using System.IO;
using System.Text.Json;

namespace NexaCRM.Pages.DesignSystem;

public static class DesignSystemBlueprintProvider
{
    private static readonly Lazy<DesignSystemBlueprint> CachedBlueprint = new(() =>
    {
        var assembly = typeof(DesignSystemBlueprintProvider).Assembly;
        using var stream = assembly.GetManifestResourceStream("NexaCRM.Pages.DesignSystem.PipedriveDesignSystem.json")
            ?? throw new InvalidOperationException("Unable to locate the Pipedrive design system blueprint resource.");

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        var blueprint = JsonSerializer.Deserialize<DesignSystemBlueprint>(json, options);
        return blueprint ?? throw new InvalidOperationException("Failed to deserialize the Pipedrive design system blueprint.");
    });

    public static DesignSystemBlueprint Blueprint => CachedBlueprint.Value;
}
