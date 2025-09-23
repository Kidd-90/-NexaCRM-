using Microsoft.Extensions.Configuration;
using NexaCRM.WebClient.Models.SystemInfo;
using NexaCRM.WebClient.Services.Interfaces;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services;

public class SystemInfoService : ISystemInfoService
{
    private readonly IConfiguration _configuration;

    public SystemInfoService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<SystemInfo> GetSystemInfoAsync()
    {
        var info = _configuration.GetSection("SystemInfo").Get<SystemInfo>() ?? new SystemInfo();
        return Task.FromResult(info);
    }
}

