using System.Threading.Tasks;
namespace NexaCRM.UI.Services.Interfaces
{
    public enum DevicePlatform
    {
        Desktop,
        Android,
        Ios
    }

    public interface IDeviceService
    {
        Task<DevicePlatform> GetPlatformAsync();
        Task<bool> IsMobileAsync();
        Task<bool> IsIosAsync();
        Task<bool> IsAndroidAsync();
    }
}
