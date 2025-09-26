using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace NexaCRM.WebClient.Services;

public sealed class ActionInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    public ActionInterop(IJSRuntime jsRuntime)
    {
        if (jsRuntime is null)
        {
            throw new ArgumentNullException(nameof(jsRuntime));
        }

        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/actions.js").AsTask());
    }

    public async Task VibrateAsync(int duration)
    {
        if (duration <= 0)
        {
            return;
        }

        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync("vibrate", duration).ConfigureAwait(false);
    }

    public async Task OpenTelephoneAsync(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return;
        }

        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync("openTel", phoneNumber).ConfigureAwait(false);
    }

    public async Task OpenMailtoAsync(string? mailtoUrl)
    {
        if (string.IsNullOrWhiteSpace(mailtoUrl))
        {
            return;
        }

        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync("openMailto", mailtoUrl).ConfigureAwait(false);
    }

    public async Task<bool> CopyTextAsync(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        var module = await _moduleTask.Value.ConfigureAwait(false);
        return await module.InvokeAsync<bool>("copyText", value).ConfigureAwait(false);
    }

    public async Task TriggerDownloadAsync(string base64Data, string fileName, string? contentType = null)
    {
        if (string.IsNullOrEmpty(base64Data) || string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync("triggerDownload", base64Data, fileName, contentType).ConfigureAwait(false);
    }

    public async Task SmoothScrollToAsync(string elementId)
    {
        if (string.IsNullOrWhiteSpace(elementId))
        {
            return;
        }

        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync("smoothScrollToId", elementId).ConfigureAwait(false);
    }

    public async Task FocusSelectorAsync(string selector)
    {
        if (string.IsNullOrWhiteSpace(selector))
        {
            return;
        }

        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync("focusSelector", selector).ConfigureAwait(false);
    }

    public async Task<bool> IsMobileViewportAsync()
    {
        var module = await _moduleTask.Value.ConfigureAwait(false);
        return await module.InvokeAsync<bool>("isMobileViewport").ConfigureAwait(false);
    }

    public async Task RegisterFabOutsideHandlerAsync()
    {
        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync("registerFabOutsideHandler").ConfigureAwait(false);
    }

    public async Task SetupMobileDashboardAsync()
    {
        var module = await _moduleTask.Value.ConfigureAwait(false);
        await module.InvokeVoidAsync("setupMobileDashboard").ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            try
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.DisposeAsync().ConfigureAwait(false);
            }
            catch
            {
                // ignore disposal failures
            }
        }
    }
}
