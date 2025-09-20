using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface IDuplicateMonitorService
    {
        Task StartAsync();
        Task RunOnceAsync();
    }
}
