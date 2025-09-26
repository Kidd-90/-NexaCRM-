namespace NexaCRM.WebClient.Models.Navigation;

/// <summary>
/// Represents a shortcut that a user has marked as a favourite for quick access on mobile devices.
/// </summary>
public sealed record UserFavoriteShortcut(
    string Id,
    string Label,
    string IconCssClass,
    string TargetUri,
    string BackgroundColor,
    string IconColor)
{
    /// <summary>
    /// Provides a defensive empty instance when favourite data cannot be loaded.
    /// </summary>
    public static readonly UserFavoriteShortcut Empty = new(
        Id: string.Empty,
        Label: string.Empty,
        IconCssClass: string.Empty,
        TargetUri: string.Empty,
        BackgroundColor: "transparent",
        IconColor: "var(--primary-color)");

    /// <summary>
    /// Checks whether the shortcut contains a valid navigation target.
    /// </summary>
    public bool HasTarget => !string.IsNullOrWhiteSpace(TargetUri);
}
