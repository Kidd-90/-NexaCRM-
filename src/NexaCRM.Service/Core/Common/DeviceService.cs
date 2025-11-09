using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.UI.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IJSRuntime _jsRuntime;

        public DeviceService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<DevicePlatform> GetPlatformAsync()
        {
            try
            {
                var platform = await _jsRuntime.InvokeAsync<string>("deviceInterop.getPlatform");
                return ConvertPlatform(platform);
            }
            catch (JSException)
            {
                return DevicePlatform.Desktop;
            }
            catch (InvalidOperationException)
            {
                return DevicePlatform.Desktop;
            }
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
}
