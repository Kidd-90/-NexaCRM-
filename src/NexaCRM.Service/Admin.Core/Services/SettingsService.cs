using NexaCRM.Services.Admin.Models.Settings;
using NexaCRM.Services.Admin.Interfaces;
using System.Threading.Tasks;

namespace NexaCRM.Services.Admin;

public class SettingsService : ISettingsService
{
    public Task<CompanyInfo> GetCompanyInfoAsync() =>
        Task.FromResult(new CompanyInfo());

    public Task SaveCompanyInfoAsync(CompanyInfo info) =>
        Task.CompletedTask;

    public Task<SecuritySettings> GetSecuritySettingsAsync() =>
        Task.FromResult(new SecuritySettings());

    public Task SaveSecuritySettingsAsync(SecuritySettings settings) =>
        Task.CompletedTask;

    public Task<SmsSettings> GetSmsSettingsAsync() =>
        Task.FromResult(new SmsSettings());

    public Task SaveSmsSettingsAsync(SmsSettings settings) =>
        Task.CompletedTask;

    public Task<UserProfile> GetUserProfileAsync() =>
        Task.FromResult(new UserProfile());

    public Task SaveUserProfileAsync(UserProfile profile) =>
        Task.CompletedTask;
}

