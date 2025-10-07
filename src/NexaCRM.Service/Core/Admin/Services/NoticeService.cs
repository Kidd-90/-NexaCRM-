using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.CustomerCenter;

namespace NexaCRM.Services.Admin;

public sealed class NoticeService : INoticeService
{
    private readonly List<Notice> _notices = new()
    {
        new Notice
        {
            Id = 1,
            Title = "Welcome to NexaCRM",
            Summary = "온보딩 체크리스트와 함께 NexaCRM을 빠르게 시작해 보세요.",
            Content = "새롭게 개편된 온보딩 튜토리얼을 통해 연락처, 파이프라인, 자동화를 손쉽게 설정할 수 있습니다.",
            Category = NoticeCategory.General,
            Importance = NoticeImportance.Highlight,
            PublishedAt = DateTimeOffset.UtcNow.AddDays(-10),
            IsPinned = true
        },
        new Notice
        {
            Id = 2,
            Title = "시스템 점검 안내",
            Summary = "7월 마지막 주 금요일 02:00~04:00 사이 접속이 순차적으로 제한됩니다.",
            Content = "서비스 안정화를 위한 DB 이중화 구성 작업이 예정되어 있습니다. 작업 중간 단계에서 짧은 재시작이 발생할 수 있습니다.",
            Category = NoticeCategory.Maintenance,
            Importance = NoticeImportance.Normal,
            PublishedAt = DateTimeOffset.UtcNow.AddDays(-4)
        }
    };

    private int _nextId = 3;

    public Task<IEnumerable<Notice>> GetNoticesAsync() =>
        Task.FromResult<IEnumerable<Notice>>(_notices);

    public Task<Notice?> GetNoticeAsync(long id) =>
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

    public Task DeleteNoticeAsync(long id)
    {
        var notice = _notices.FirstOrDefault(n => n.Id == id);
        if (notice is not null)
        {
            _notices.Remove(notice);
        }
        return Task.CompletedTask;
    }
}
