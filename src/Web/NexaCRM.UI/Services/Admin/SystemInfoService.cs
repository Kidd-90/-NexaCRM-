using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.SystemInfo;

namespace NexaCRM.WebClient.Services.Admin;

public sealed class SystemInfoService : ISystemInfoService
{
    private SystemInfo _info = new()
    {
        Terms = "기본 이용 약관",
        CompanyAddress = "서울특별시 강남구",
        SupportContacts = new[] { "support@nexacrm.com" }
    };

    public Task<SystemInfo> GetSystemInfoAsync() => Task.FromResult(_info);

    public Task SaveSystemInfoAsync(SystemInfo info)
    {
        _info = info;
        return Task.CompletedTask;
    }
}
