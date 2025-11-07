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

        var layoutMode = settings.LayoutMode ?? AppContentLayoutSettings.DefaultLayoutMode;
        var cssClass = settings.AdditionalCssClass;
        var attributes = settings.AdditionalAttributes is null
            ? null
            : new Dictionary<string, object?>(settings.AdditionalAttributes, StringComparer.Ordinal);

        if (string.Equals(LayoutMode, layoutMode, StringComparison.Ordinal)
            && string.Equals(AdditionalCssClass, cssClass, StringComparison.Ordinal)
            && DictionaryEquals(AdditionalAttributes, attributes))
        {
            return;
        }

        LayoutMode = layoutMode;
        AdditionalCssClass = cssClass;
        AdditionalAttributes = attributes;

        NotifyChanged();
    }

    internal void Reset(bool suppressNotification = false)
    {
        var changed = !string.Equals(LayoutMode, AppContentLayoutSettings.DefaultLayoutMode, StringComparison.Ordinal)
            || !string.IsNullOrEmpty(AdditionalCssClass)
            || (AdditionalAttributes?.Count ?? 0) > 0;

        LayoutMode = AppContentLayoutSettings.DefaultLayoutMode;
        AdditionalCssClass = null;
        AdditionalAttributes = null;

        if (!suppressNotification && changed)
        {
            NotifyChanged();
        }
    }

    private void NotifyChanged()
    {
        onOptionsChanged();
    }

    private static bool DictionaryEquals(
        IReadOnlyDictionary<string, object?>? left,
        IReadOnlyDictionary<string, object?>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return left is null && right is null;
        }

        if (left.Count != right.Count)
        {
            return false;
        }

        foreach (var (key, value) in left)
        {
            if (!right.TryGetValue(key, out var otherValue))
            {
                return false;
            }

            if (!Equals(value, otherValue))
            {
                return false;
            }
        }

        return true;
    }
}
