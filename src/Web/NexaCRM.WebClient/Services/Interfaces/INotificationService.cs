using NexaCRM.WebClient.Models.Settings;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface INotificationService
{
    Task<NotificationSettings> GetSettingsAsync();
    Task SaveSettingsAsync(NotificationSettings settings);
}
