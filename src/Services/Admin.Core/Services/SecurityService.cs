using NexaCRM.WebClient.Models.Settings;
using NexaCRM.WebClient.Services.Interfaces;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services;

public class SecurityService : ISecurityService
{
    public Task<SecuritySettings> GetSecuritySettingsAsync() =>
        Task.FromResult(new SecuritySettings());

    public Task SaveSecuritySettingsAsync(SecuritySettings settings) =>
        Task.CompletedTask;
}
