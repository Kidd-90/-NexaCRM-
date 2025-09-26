using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.WebClient.Services;

public sealed class MobileInteractionService : IMobileInteractionService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ActionInterop _actionInterop;
    private bool _searchOpen;
    private bool _notificationsOpen;

    public MobileInteractionService(IJSRuntime jsRuntime, ActionInterop actionInterop)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _actionInterop = actionInterop ?? throw new ArgumentNullException(nameof(actionInterop));
    }

    public bool IsSearchOpen => _searchOpen;
    public bool AreNotificationsOpen => _notificationsOpen;

    public event Action? StateChanged;

    public async Task ToggleMenuAsync()
    {
        await ClosePanelsInternalAsync(closeSearch: true, closeNotifications: true).ConfigureAwait(false);
        try
        {
            await _jsRuntime.InvokeVoidAsync("layoutInterop.toggleMenu", false).ConfigureAwait(false);
        }
        catch
        {
            // Swallow invocation exceptions to keep UX responsive in environments without the helper script.
        }
    }

    public async Task ToggleSearchAsync()
    {
        _searchOpen = !_searchOpen;
        if (_searchOpen)
        {
            _notificationsOpen = false;
        }

        OnStateChanged();

        if (_searchOpen)
        {
            await Task.Delay(120).ConfigureAwait(false);
            await _actionInterop.FocusSelectorAsync(".mobile-search-input input").ConfigureAwait(false);
        }
    }

    public Task ToggleNotificationsAsync()
    {
        _notificationsOpen = !_notificationsOpen;
        if (_notificationsOpen)
        {
            _searchOpen = false;
        }

        OnStateChanged();
        return Task.CompletedTask;
    }

    public async Task CloseAllAsync()
    {
        await ClosePanelsInternalAsync(closeSearch: true, closeNotifications: true).ConfigureAwait(false);
    }

    public Task ScrollToAsync(string elementId)
    {
        return _actionInterop.SmoothScrollToAsync(elementId);
    }

    private async Task ClosePanelsInternalAsync(bool closeSearch, bool closeNotifications)
    {
        var changed = false;

        if (closeSearch && _searchOpen)
        {
            _searchOpen = false;
            changed = true;
        }

        if (closeNotifications && _notificationsOpen)
        {
            _notificationsOpen = false;
            changed = true;
        }

        if (changed)
        {
            OnStateChanged();
            await Task.Yield();
        }
    }

    private void OnStateChanged() => StateChanged?.Invoke();
}
