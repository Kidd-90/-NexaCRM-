using System.Collections.Generic;
using System.Threading.Tasks;
using NexaCRM.UI.Models.Navigation;

namespace NexaCRM.UI.Services.Interfaces;

/// <summary>
/// Provides access to the shortcuts a user has marked as favourites.
/// </summary>
public interface IUserFavoritesService
{
    /// <summary>
    /// Retrieves the favourites configured for the current user.
    /// </summary>
    Task<IReadOnlyList<UserFavoriteShortcut>> GetFavoritesAsync();
}
