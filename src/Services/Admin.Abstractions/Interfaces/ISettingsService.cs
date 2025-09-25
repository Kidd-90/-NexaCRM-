using NexaCRM.Services.Admin.Models.Settings;
using System.Threading.Tasks;

namespace NexaCRM.Services.Admin.Interfaces;

public interface ISettingsService
{
    Task<CompanyInfo> GetCompanyInfoAsync();
    Task SaveCompanyInfoAsync(CompanyInfo info);
    Task<SecuritySettings> GetSecuritySettingsAsync();
    Task SaveSecuritySettingsAsync(SecuritySettings settings);
    Task<SmsSettings> GetSmsSettingsAsync();
    Task SaveSmsSettingsAsync(SmsSettings settings);
    Task<UserProfile> GetUserProfileAsync();
    Task SaveUserProfileAsync(UserProfile profile);
}

