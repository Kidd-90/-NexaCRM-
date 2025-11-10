using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.UI.Services
{
    public class DeviceService : IDeviceService
    {
        private const int MaximumRetryAttempts = 5;
        private const int RetryDelayMilliseconds = 150;

        private readonly IJSRuntime _jsRuntime;

        public DeviceService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<DevicePlatform> GetPlatformAsync()
        {
            for (var attempt = 0; attempt < MaximumRetryAttempts; attempt++)
            {
                try
                {
                    var platformToken = await _jsRuntime.InvokeAsync<string>("deviceInterop.getPlatform");

                    if (string.IsNullOrWhiteSpace(platformToken))
                    {
                        if (attempt < MaximumRetryAttempts - 1)
                        {
                            await Task.Delay(RetryDelayMilliseconds).ConfigureAwait(false);
                            continue;
                        }

                        return DevicePlatform.Desktop;
                    }

                    return ConvertPlatform(platformToken);
                }
                catch (JSException ex) when (ShouldRetryInterop(ex) && attempt < MaximumRetryAttempts - 1)
                {
                    await Task.Delay(RetryDelayMilliseconds).ConfigureAwait(false);
                }
                catch (InvalidOperationException) when (attempt < MaximumRetryAttempts - 1)
                {
                    await Task.Delay(RetryDelayMilliseconds).ConfigureAwait(false);
                }
                catch (JSException)
                {
                    break;
                }
            }

            return DevicePlatform.Desktop;
        }

        public async Task<bool> IsMobileAsync()
        {
            var platform = await GetPlatformAsync();
            return platform == DevicePlatform.Android || platform == DevicePlatform.Ios;
        }

        public async Task<bool> IsIosAsync()
        {
            var platform = await GetPlatformAsync();
            return platform == DevicePlatform.Ios;
        }

        public async Task<bool> IsAndroidAsync()
        {
            var platform = await GetPlatformAsync();
            return platform == DevicePlatform.Android;
        }

        private static bool ShouldRetryInterop(JSException exception)
        {
            if (exception is null)
            {
                return false;
            }

            var message = exception.Message;
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            return message.Contains("deviceInterop", StringComparison.OrdinalIgnoreCase)
                || message.Contains("is not defined", StringComparison.OrdinalIgnoreCase)
                || message.Contains("does not exist", StringComparison.OrdinalIgnoreCase);
        }

        private static DevicePlatform ConvertPlatform(string? platformToken)
        {
            return platformToken?.Trim().ToLowerInvariant() switch
            {
                "android" => DevicePlatform.Android,
                "ios" => DevicePlatform.Ios,
                _ => DevicePlatform.Desktop
            };
        }
    }
}
