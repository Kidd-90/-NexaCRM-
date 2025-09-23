using NexaCRM.WebClient.Models.Settings;
using NexaCRM.WebClient.Services.Interfaces;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services;

public class NotificationService : INotificationService
{
    private NotificationSettings _settings = new();

    public Task<NotificationSettings> GetSettingsAsync() =>
        Task.FromResult(_settings);

    public Task SaveSettingsAsync(NotificationSettings settings)
    {
        _settings = settings;
        return Task.CompletedTask;
    }
}
