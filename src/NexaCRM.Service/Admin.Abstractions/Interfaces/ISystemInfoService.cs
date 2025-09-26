using NexaCRM.Services.Admin.Models.SystemInfo;
using System.Threading.Tasks;

namespace NexaCRM.Services.Admin.Interfaces;

public interface ISystemInfoService
{
    Task<SystemInfo> GetSystemInfoAsync();
}

