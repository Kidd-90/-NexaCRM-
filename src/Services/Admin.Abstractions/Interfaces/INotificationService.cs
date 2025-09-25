using NexaCRM.Services.Admin.Models.Settings;
using System.Threading.Tasks;

namespace NexaCRM.Services.Admin.Interfaces;

public interface INotificationService
{
    Task<NotificationSettings> GetSettingsAsync();
    Task SaveSettingsAsync(NotificationSettings settings);
}
