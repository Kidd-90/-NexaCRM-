using System.Collections.Generic;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Navigation;

namespace NexaCRM.WebClient.Services.Interfaces;

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
