using System;
using System.Collections.Generic;

namespace NexaCRM.UI.Shared;

public sealed class AppContentLayoutOptions
{
    private readonly Action onOptionsChanged;

    internal AppContentLayoutOptions(Action onOptionsChanged)
    {
        this.onOptionsChanged = onOptionsChanged ?? throw new ArgumentNullException(nameof(onOptionsChanged));
    }

    public string LayoutMode { get; private set; } = AppContentLayoutSettings.DefaultLayoutMode;

    public string? AdditionalCssClass { get; private set; }

    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; private set; }

    internal void Apply(AppContentLayoutSettings settings)
    {
        if (settings is null)
        {
            Reset();
            return;
        }

        LayoutMode = settings.LayoutMode ?? AppContentLayoutSettings.DefaultLayoutMode;
        AdditionalCssClass = settings.AdditionalCssClass;
        AdditionalAttributes = settings.AdditionalAttributes is null
            ? null
            : new Dictionary<string, object?>(settings.AdditionalAttributes, StringComparer.Ordinal);

        NotifyChanged();
    }

    internal void Reset(bool suppressNotification = false)
    {
        LayoutMode = AppContentLayoutSettings.DefaultLayoutMode;
        AdditionalCssClass = null;
        AdditionalAttributes = null;

        if (!suppressNotification)
        {
            NotifyChanged();
        }
    }

    private void NotifyChanged()
    {
        onOptionsChanged();
    }
}
