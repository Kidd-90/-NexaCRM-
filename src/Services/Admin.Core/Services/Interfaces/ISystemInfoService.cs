using NexaCRM.WebClient.Models.SystemInfo;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface ISystemInfoService
{
    Task<SystemInfo> GetSystemInfoAsync();
}

