using NexaCRM.Services.Admin.Models.Settings;
using NexaCRM.Services.Admin.Interfaces;
using System.Threading.Tasks;

namespace NexaCRM.Services.Admin;

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
