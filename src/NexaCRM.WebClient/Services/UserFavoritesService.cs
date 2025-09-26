using System.Collections.Generic;
using System.Threading.Tasks;
using NexaCRM.UI.Models.Navigation;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.WebClient.Services;

/// <summary>
/// Provides a simple in-memory implementation for user favourite shortcuts.
/// In a production scenario this would be replaced with an API backed service.
/// </summary>
public sealed class UserFavoritesService : IUserFavoritesService
{
    private static readonly IReadOnlyList<UserFavoriteShortcut> DefaultFavorites =
        new List<UserFavoriteShortcut>
        {
            new(
                Id: "dashboard",
                Label: "Dashboard",
                IconCssClass: "bi bi-speedometer2",
                TargetUri: "/main-dashboard",
                BackgroundColor: "rgba(33, 83, 200, 0.14)",
                IconColor: "#2153C8"),
            new(
                Id: "sales",
                Label: "Sales",
                IconCssClass: "bi bi-kanban",
                TargetUri: "/sales-pipeline-page",
                BackgroundColor: "rgba(14, 165, 233, 0.14)",
                IconColor: "#0EA5E9"),
            new(
                Id: "contacts",
                Label: "Contacts",
                IconCssClass: "bi bi-people",
                TargetUri: "/contacts",
                BackgroundColor: "rgba(249, 115, 22, 0.14)",
                IconColor: "#F97316"),
            new(
                Id: "reports",
                Label: "Reports",
                IconCssClass: "bi bi-graph-up-arrow",
                TargetUri: "/reports-page",
                BackgroundColor: "rgba(34, 197, 94, 0.14)",
                IconColor: "#22C55E"),
        };

    public Task<IReadOnlyList<UserFavoriteShortcut>> GetFavoritesAsync()
        => Task.FromResult(DefaultFavorites);
}
