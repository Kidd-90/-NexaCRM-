using NexaCRM.WebClient.Models.Settings;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface ISecurityService
{
    Task<SecuritySettings> GetSecuritySettingsAsync();
    Task SaveSecuritySettingsAsync(SecuritySettings settings);
}
