using System.Threading.Tasks;

namespace NexaCRM.Services.Admin.Interfaces
{
    public interface IDuplicateMonitorService
    {
        Task StartAsync();
        Task RunOnceAsync();
    }
}
