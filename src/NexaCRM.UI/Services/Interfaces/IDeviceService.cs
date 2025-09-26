using System.Threading.Tasks;
namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface IDeviceService
    {
        Task<bool> IsMobileAsync();
    }
}
