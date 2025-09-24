using System;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface IMobileInteractionService
{
    bool IsSearchOpen { get; }
    bool AreNotificationsOpen { get; }

    event Action? StateChanged;

    Task ToggleMenuAsync();
    Task ToggleSearchAsync();
    Task ToggleNotificationsAsync();
    Task CloseAllAsync();
    Task ScrollToAsync(string elementId);
}
