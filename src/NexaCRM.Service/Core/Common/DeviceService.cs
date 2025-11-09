using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.UI.Services
{
    public sealed class DeviceService : IDeviceService
    {
        private readonly IJSRuntime _jsRuntime;

        public DeviceService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<DevicePlatform> GetPlatformAsync()
        {
            const int maxAttempts = 5;
            const int retryDelayMilliseconds = 200;
            Exception? lastException = null;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var platform = await _jsRuntime.InvokeAsync<string>("deviceInterop.getPlatform");
                    return ConvertPlatform(platform);
                }
                catch (JSException ex) when (attempt < maxAttempts)
                {
                    lastException = ex;
                    await Task.Delay(retryDelayMilliseconds);
                }
                catch (InvalidOperationException ex) when (attempt < maxAttempts)
                {
                    lastException = ex;
                    await Task.Delay(retryDelayMilliseconds);
                }
                catch (JSException ex)
                {
                    lastException = ex;
                    break;
                }
                catch (InvalidOperationException ex)
                {
                    lastException = ex;
                    break;
                }
            }

            if (lastException is not null)
            {
                throw new DevicePlatformDetectionException("Failed to resolve the device platform from the JavaScript runtime.", lastException);
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

    public sealed class DevicePlatformDetectionException : Exception
    {
        public DevicePlatformDetectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
