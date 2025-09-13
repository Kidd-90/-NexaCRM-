using NexaCRM.WebClient.Models.SystemInfo;
using NexaCRM.WebClient.Services.Interfaces;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services;

public class SystemInfoService : ISystemInfoService
{
    public Task<SystemInfo> GetSystemInfoAsync() =>
        Task.FromResult(new SystemInfo());
}

