using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Settings;

namespace NexaCRM.Services.Admin;

public sealed class SettingsService : ISettingsService
{
    private CompanyInfo _companyInfo = new();
    private SmsSettings _smsSettings = new();
    private UserProfile _userProfile = new();

    public Task<CompanyInfo> GetCompanyInfoAsync() => Task.FromResult(_companyInfo);

    public Task SaveCompanyInfoAsync(CompanyInfo info)
    {
        _companyInfo = info;
        return Task.CompletedTask;
    }

    public Task<SecuritySettings> GetSecuritySettingsAsync() => Task.FromResult(new SecuritySettings());

    public Task SaveSecuritySettingsAsync(SecuritySettings settings) => Task.CompletedTask;

    public Task<SmsSettings> GetSmsSettingsAsync() => Task.FromResult(_smsSettings);

    public Task SaveSmsSettingsAsync(SmsSettings settings)
    {
        _smsSettings = settings;
        return Task.CompletedTask;
    }

    public Task<UserProfile> GetUserProfileAsync() => Task.FromResult(_userProfile);

    public Task SaveUserProfileAsync(UserProfile profile)
    {
        _userProfile = profile;
        return Task.CompletedTask;
    }
}
