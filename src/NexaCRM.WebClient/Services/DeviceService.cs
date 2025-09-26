using System.Threading.Tasks;
using Microsoft.JSInterop;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.WebClient.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IJSRuntime _jsRuntime;

        public DeviceService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> IsMobileAsync()
        {
            return await _jsRuntime.InvokeAsync<bool>("deviceInfo.isMobile");
        }
    }
}
