using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using NexaCRM.WebClient.Models.Navigation;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services;

public sealed class NavigationStateService : INavigationStateService, IAsyncDisposable
{
    private const int MaxRecentItems = 6;
    private readonly IJSRuntime _jsRuntime;
    private readonly List<NavigationHistoryEntry> _recent = new();
    private readonly ReadOnlyCollection<NavigationHistoryEntry> _readonlyRecent;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _initialized;
    private IJSObjectReference? _module;

    public NavigationStateService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _readonlyRecent = _recent.AsReadOnly();
    }

    public event EventHandler? RecentLinksChanged;

    public IReadOnlyList<NavigationHistoryEntry> RecentLinks => _readonlyRecent;

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_initialized)
            {
                return;
            }

            try
            {
                var json = await JsInvokeAsync<string?>("getRecentNavigation").ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var entries = JsonSerializer.Deserialize<List<NavigationHistoryEntry>>(json, SerializerOptions);
                    if (entries is not null && entries.Count > 0)
                    {
                        _recent.Clear();
                        _recent.AddRange(entries
                            .Where(entry => entry is not null)
                            .OrderByDescending(entry => entry.TimestampUtc)
                            .Take(MaxRecentItems));
                    }
                }
            }
            catch
            {
                // Local storage might be unavailable (e.g., private browsing). Ignore errors.
                _recent.Clear();
            }

            _initialized = true;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task TrackAsync(NavigationHistoryEntry entry)
    {
        if (entry is null)
        {
            return;
        }

        await InitializeAsync().ConfigureAwait(false);
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var normalizedHref = string.IsNullOrWhiteSpace(entry.Href) ? string.Empty : entry.Href.Trim('/');
            _recent.RemoveAll(item => string.Equals(item.Href, normalizedHref, StringComparison.OrdinalIgnoreCase));
            _recent.Insert(0, entry with { Href = normalizedHref });

            if (_recent.Count > MaxRecentItems)
            {
                _recent.RemoveRange(MaxRecentItems, _recent.Count - MaxRecentItems);
            }

            await PersistAsync().ConfigureAwait(false);
            RecentLinksChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task ClearRecentAsync()
    {
        await InitializeAsync().ConfigureAwait(false);
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            _recent.Clear();
            await JsInvokeVoidAsync("clearRecentNavigation").ConfigureAwait(false);
            RecentLinksChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task PersistAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_recent, SerializerOptions);
            await JsInvokeVoidAsync("saveRecentNavigation", json).ConfigureAwait(false);
        }
        catch
        {
            // Ignore persistence failures – e.g., storage quota exceeded.
        }
    }

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        if (_module is not null)
        {
            return _module;
        }

        _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/layout.js");
        return _module;
    }

    private async Task<T?> JsInvokeAsync<T>(string method, params object[] args)
    {
        try
        {
            var module = await GetModuleAsync().ConfigureAwait(false);
            return await module.InvokeAsync<T?>(method, args).ConfigureAwait(false);
        }
        catch
        {
            return default;
        }
    }

    private async Task JsInvokeVoidAsync(string method, params object[] args)
    {
        try
        {
            var module = await GetModuleAsync().ConfigureAwait(false);
            await module.InvokeVoidAsync(method, args).ConfigureAwait(false);
        }
        catch
        {
            // ignore
        }
    }

    private static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync().ConfigureAwait(false);
            }
            catch
            {
                // ignore
            }
        }

        _gate.Dispose();
    }
}
