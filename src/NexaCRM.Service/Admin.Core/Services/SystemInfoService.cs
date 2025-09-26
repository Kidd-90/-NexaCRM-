using Microsoft.Extensions.Configuration;
using NexaCRM.Services.Admin.Models.SystemInfo;
using NexaCRM.Services.Admin.Interfaces;
using System.Threading.Tasks;

namespace NexaCRM.Services.Admin;

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

