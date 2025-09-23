using NexaCRM.WebClient.Models.Settings;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces;

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

