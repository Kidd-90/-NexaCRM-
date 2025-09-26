using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Settings;

namespace NexaCRM.WebClient.Services.Admin;

public sealed class SecurityService : ISecurityService
{
    public Task<SecuritySettings> GetSecuritySettingsAsync() =>
        Task.FromResult(new SecuritySettings());

    public Task SaveSecuritySettingsAsync(SecuritySettings settings) => Task.CompletedTask;
}
