using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Settings;

namespace NexaCRM.Services.Admin;

public sealed class NotificationService : INotificationService
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
