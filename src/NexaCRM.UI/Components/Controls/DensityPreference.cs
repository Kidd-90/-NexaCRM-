using System;

namespace NexaCRM.UI.Components.Controls;

public static class DensityPreference
{
    public const string StorageKey = "nexacrm:ui:density-mode";

    public static string ToDataAttribute(this DensityMode mode)
    {
        return mode switch
        {
            DensityMode.Compact => "compact",
            _ => "comfortable"
        };
    }

    public static bool TryParse(string? value, out DensityMode mode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            mode = DensityMode.Comfortable;
            return false;
        }

        return Enum.TryParse(value, ignoreCase: true, out mode);
    }
}
