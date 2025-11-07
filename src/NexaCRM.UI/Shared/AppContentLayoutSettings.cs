using System;
using System.Collections.Generic;

namespace NexaCRM.UI.Shared;

public sealed class AppContentLayoutSettings
{
    public const string DefaultLayoutMode = "constrained";

    public AppContentLayoutSettings(
        string? layoutMode = DefaultLayoutMode,
        string? additionalCssClass = null,
        IReadOnlyDictionary<string, object?>? additionalAttributes = null)
    {
        LayoutMode = string.IsNullOrWhiteSpace(layoutMode) ? DefaultLayoutMode : layoutMode;
        AdditionalCssClass = additionalCssClass;
        AdditionalAttributes = additionalAttributes;
    }

    public string LayoutMode { get; }

    public string? AdditionalCssClass { get; }

    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; }
}
