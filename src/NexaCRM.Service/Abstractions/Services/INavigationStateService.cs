using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NexaCRM.UI.Models.Navigation;

namespace NexaCRM.UI.Services.Interfaces;

public interface INavigationStateService
{
    event EventHandler? RecentLinksChanged;

    IReadOnlyList<NavigationHistoryEntry> RecentLinks { get; }

    Task InitializeAsync();

    Task TrackAsync(NavigationHistoryEntry entry);

    Task ClearRecentAsync();
}
