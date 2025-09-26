using NexaCRM.Services.Admin.Models.Settings;
using System.Threading.Tasks;

namespace NexaCRM.Services.Admin.Interfaces;

public interface ISecurityService
{
    Task<SecuritySettings> GetSecuritySettingsAsync();
    Task SaveSecuritySettingsAsync(SecuritySettings settings);
}
