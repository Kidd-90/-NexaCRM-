using NexaCRM.Services.Admin.Models.Settings;
using NexaCRM.Services.Admin.Interfaces;
using System.Threading.Tasks;

namespace NexaCRM.Services.Admin;

public class SecurityService : ISecurityService
{
    public Task<SecuritySettings> GetSecuritySettingsAsync() =>
        Task.FromResult(new SecuritySettings());

    public Task SaveSecuritySettingsAsync(SecuritySettings settings) =>
        Task.CompletedTask;
}
