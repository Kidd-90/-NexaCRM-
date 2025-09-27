using System.Threading.Tasks;
namespace NexaCRM.UI.Services.Interfaces
{
    public interface IDeviceService
    {
        Task<bool> IsMobileAsync();
    }
}
