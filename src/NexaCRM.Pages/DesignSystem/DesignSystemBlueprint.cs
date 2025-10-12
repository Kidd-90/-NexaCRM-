using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NexaCRM.Pages.DesignSystem;

public sealed record class DesignSystemBlueprint
{
    [JsonPropertyName("designSystemName")]
    public required string DesignSystemName { get; init; }

    [JsonPropertyName("basedOn")]
    public required string BasedOn { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("designPhilosophy")]
    public required DesignPhilosophy DesignPhilosophy { get; init; }

    [JsonPropertyName("brandIdentity")]
    public required BrandIdentity BrandIdentity { get; init; }

    [JsonPropertyName("layout")]
    public required LayoutDefinition Layout { get; init; }

    [JsonPropertyName("components")]
    public required IReadOnlyList<ComponentDefinition> Components { get; init; }

    [JsonPropertyName("userFlowsAndInteraction")]
    public required UserFlowsAndInteraction UserFlowsAndInteraction { get; init; }

    [JsonPropertyName("aiImplementationNotes")]
    public required AiImplementationNotes AiImplementationNotes { get; init; }
}

public sealed record class DesignPhilosophy
{
    [JsonPropertyName("corePrinciples")]
    public required IReadOnlyList<string> CorePrinciples { get; init; }

    [JsonPropertyName("experienceGoals")]
    public required IReadOnlyList<string> ExperienceGoals { get; init; }
}

public sealed record class BrandIdentity
{
    [JsonPropertyName("logo")]
    public required LogoDetails Logo { get; init; }

    [JsonPropertyName("colorPalette")]
    public required ColorPalette ColorPalette { get; init; }

    [JsonPropertyName("typography")]
    public required Typography Typography { get; init; }

    [JsonPropertyName("toneOfVoice")]
    public required ToneOfVoice ToneOfVoice { get; init; }
}

public sealed record class LogoDetails
{
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("usageGuidelines")]
    public required IReadOnlyList<string> UsageGuidelines { get; init; }

    [JsonPropertyName("minimumSize")]
    public required MinimumSize MinimumSize { get; init; }
}

public sealed record class MinimumSize
{
    [JsonPropertyName("web")]
    public required string Web { get; init; }

    [JsonPropertyName("print")]
    public required string Print { get; init; }
}

public sealed record class ColorPalette
{
    [JsonPropertyName("styleGuide")]
    public required ColorPaletteStyleGuide StyleGuide { get; init; }

    [JsonPropertyName("lightMode")]
    public required ColorPaletteMode LightMode { get; init; }

    [JsonPropertyName("darkMode")]
    public required ColorPaletteMode DarkMode { get; init; }
}

public sealed record class ColorPaletteStyleGuide
{
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("summary")]
    public required string Summary { get; init; }

    [JsonPropertyName("keywords")]
    public required IReadOnlyList<string> Keywords { get; init; }

    [JsonPropertyName("referenceBrands")]
    public required IReadOnlyList<string> ReferenceBrands { get; init; }
}

public sealed record class ColorPaletteMode
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("roles")]
    public required IReadOnlyList<ColorRoleDefinition> Roles { get; init; }

    [JsonPropertyName("implementationNotes")]
    public IReadOnlyList<string>? ImplementationNotes { get; init; }
}

public sealed record class ColorRoleDefinition
{
    [JsonPropertyName("token")]
    public required string Token { get; init; }

    [JsonPropertyName("role")]
    public required string Role { get; init; }

    [JsonPropertyName("color")]
    public required ColorSwatchDefinition Color { get; init; }

    [JsonPropertyName("usage")]
    public required string Usage { get; init; }
}

public sealed record class ColorSwatchDefinition
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("hex")]
    public required string Hex { get; init; }

    [JsonPropertyName("usage")]
    public string? Usage { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}

public sealed record class Typography
{
    [JsonPropertyName("fontFamily")]
    public required string FontFamily { get; init; }

    [JsonPropertyName("principles")]
    public required IReadOnlyList<string> Principles { get; init; }

    [JsonPropertyName("typeScale")]
    public required IReadOnlyList<TypeScaleToken> TypeScale { get; init; }
}

public sealed record class TypeScaleToken
{
    [JsonPropertyName("token")]
    public required string Token { get; init; }

    [JsonPropertyName("size")]
    public required string Size { get; init; }

    [JsonPropertyName("lineHeight")]
    public required string LineHeight { get; init; }

    [JsonPropertyName("usage")]
    public required string Usage { get; init; }
}

public sealed record class ToneOfVoice
{
    [JsonPropertyName("principles")]
    public required IReadOnlyList<string> Principles { get; init; }

    [JsonPropertyName("microcopyPatterns")]
    public required IReadOnlyList<string> MicrocopyPatterns { get; init; }
}

public sealed record class LayoutDefinition
{
    [JsonPropertyName("mainStructure")]
    public required MainStructure MainStructure { get; init; }

    [JsonPropertyName("navigation")]
    public required NavigationDefinition Navigation { get; init; }

    [JsonPropertyName("coreViews")]
    public required IReadOnlyList<CoreView> CoreViews { get; init; }
}

public sealed record class MainStructure
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("grid")]
    public required LayoutGrid Grid { get; init; }
}

public sealed record class LayoutGrid
{
    [JsonPropertyName("contentWidth")]
    public required string ContentWidth { get; init; }

    [JsonPropertyName("sidebarWidth")]
    public required string SidebarWidth { get; init; }

    [JsonPropertyName("breakpoints")]
    public required Breakpoints Breakpoints { get; init; }
}

public sealed record class Breakpoints
{
    [JsonPropertyName("sm")]
    public required string Small { get; init; }

    [JsonPropertyName("md")]
    public required string Medium { get; init; }

    [JsonPropertyName("lg")]
    public required string Large { get; init; }

    [JsonPropertyName("xl")]
    public required string ExtraLarge { get; init; }
}

public sealed record class NavigationDefinition
{
    [JsonPropertyName("primary")]
    public required NavigationLayer Primary { get; init; }

    [JsonPropertyName("secondary")]
    public required NavigationLayer Secondary { get; init; }
}

public sealed record class NavigationLayer
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("features")]
    public required IReadOnlyList<string> Features { get; init; }
}

public sealed record class CoreView
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("keyElements")]
    public required IReadOnlyList<string> KeyElements { get; init; }
}

public sealed record class ComponentDefinition
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("attributes")]
    public required IReadOnlyList<string> Attributes { get; init; }
}

public sealed record class UserFlowsAndInteraction
{
    [JsonPropertyName("coreFlows")]
    public required IReadOnlyList<CoreFlow> CoreFlows { get; init; }

    [JsonPropertyName("interactionPatterns")]
    public required InteractionPatterns InteractionPatterns { get; init; }
}

public sealed record class CoreFlow
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("persona")]
    public required string Persona { get; init; }

    [JsonPropertyName("goal")]
    public required string Goal { get; init; }

    [JsonPropertyName("steps")]
    public required IReadOnlyList<string> Steps { get; init; }
}

public sealed record class InteractionPatterns
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Items { get; init; } = new();
}

public sealed record class AiImplementationNotes
{
    [JsonPropertyName("trainingReferences")]
    public required IReadOnlyList<string> TrainingReferences { get; init; }

    [JsonPropertyName("dataConsiderations")]
    public required IReadOnlyList<string> DataConsiderations { get; init; }

    [JsonPropertyName("prototyping")]
    public required PrototypingNotes Prototyping { get; init; }
}

public sealed record class PrototypingNotes
{
    [JsonPropertyName("preferredTools")]
    public required IReadOnlyList<string> PreferredTools { get; init; }

    [JsonPropertyName("handoff")]
    public required string Handoff { get; init; }
}
