using NexaCRM.WebClient.Models.CustomerCenter;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services;

public class NoticeService : INoticeService
{
    private readonly List<Notice> _notices = new()
    {
        new Notice(1, "Welcome", "Welcome to NexaCRM!"),
        new Notice(2, "Maintenance", "System maintenance on Friday."),
    };

    private int _nextId = 3;

    public Task<IEnumerable<Notice>> GetNoticesAsync() =>
        Task.FromResult<IEnumerable<Notice>>(_notices);

    public Task<Notice?> GetNoticeAsync(int id) =>
        Task.FromResult(_notices.FirstOrDefault(n => n.Id == id));

    public Task CreateNoticeAsync(Notice notice)
    {
        var newNotice = notice with { Id = _nextId++ };
        _notices.Add(newNotice);
        return Task.CompletedTask;
    }

    public Task UpdateNoticeAsync(Notice notice)
    {
        var index = _notices.FindIndex(n => n.Id == notice.Id);
        if (index >= 0)
        {
            _notices[index] = notice;
        }
        return Task.CompletedTask;
    }

    public Task DeleteNoticeAsync(int id)
    {
        var notice = _notices.FirstOrDefault(n => n.Id == id);
        if (notice is not null)
        {
            _notices.Remove(notice);
        }
        return Task.CompletedTask;
    }
}

