using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.CustomerCenter;

namespace NexaCRM.WebClient.Services.Admin;

/// <summary>
/// Provides an in-memory customer center implementation suitable for demo and offline scenarios.
/// </summary>
public sealed class CustomerCenterService : ICustomerCenterService
{
    private readonly List<Notice> _notices;
    private readonly List<FaqItem> _faqItems;
    private int _nextNoticeId;
    private int _nextFaqId;

    public CustomerCenterService()
    {
        _notices = new List<Notice>
        {
            new(1, "정기 점검 안내", "서비스 점검이 10월 5일 예정되어 있습니다."),
            new(2, "신규 기능 출시", "고객 설문 자동화 기능이 추가되었습니다."),
        };

        _faqItems = new List<FaqItem>
        {
            new() { Id = 1, Category = "계정", Question = "로그인은 어떻게 하나요?", Answer = "이메일과 비밀번호로 로그인할 수 있습니다.", Order = 0 },
            new() { Id = 2, Category = "결제", Question = "결제 수단은 무엇이 있나요?", Answer = "신용카드와 계좌이체를 지원합니다.", Order = 1 },
        };

        _nextNoticeId = _notices.Count == 0 ? 1 : _notices.Max(n => n.Id) + 1;
        _nextFaqId = _faqItems.Count == 0 ? 1 : _faqItems.Max(f => f.Id) + 1;
    }

    public Task<IEnumerable<Notice>> GetNoticesAsync()
    {
        var ordered = _notices
            .OrderByDescending(n => n.Id)
            .Select(CloneNotice)
            .ToList();

        return Task.FromResult<IEnumerable<Notice>>(ordered);
    }

    public Task SaveNoticeAsync(Notice notice)
    {
        ArgumentNullException.ThrowIfNull(notice);

        if (notice.Id == 0)
        {
            var newNotice = notice with { Id = _nextNoticeId++ };
            _notices.Add(newNotice);
        }
        else
        {
            var index = _notices.FindIndex(n => n.Id == notice.Id);
            if (index >= 0)
            {
                _notices[index] = notice;
            }
            else
            {
                _notices.Add(notice);
                _nextNoticeId = Math.Max(_nextNoticeId, notice.Id + 1);
            }
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<FaqItem>> GetFaqItemsAsync()
    {
        var ordered = _faqItems
            .OrderBy(f => f.Order)
            .ThenBy(f => f.Id)
            .Select(CloneFaqItem)
            .ToList();

        return Task.FromResult<IEnumerable<FaqItem>>(ordered);
    }

    public Task SaveFaqItemAsync(FaqItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.Id == 0)
        {
            var newItem = CloneFaqItem(item);
            newItem.Id = _nextFaqId++;
            newItem.Order = _faqItems.Count;
            _faqItems.Add(newItem);
        }
        else
        {
            var existing = _faqItems.FirstOrDefault(f => f.Id == item.Id);
            if (existing is null)
            {
                var newItem = CloneFaqItem(item);
                _faqItems.Add(newItem);
                _nextFaqId = Math.Max(_nextFaqId, newItem.Id + 1);
            }
            else
            {
                existing.Category = item.Category;
                existing.Question = item.Question;
                existing.Answer = item.Answer;
                existing.Order = item.Order;
            }
        }

        _faqItems.Sort((left, right) => left.Order.CompareTo(right.Order));
        return Task.CompletedTask;
    }

    private static Notice CloneNotice(Notice source) => source with { };

    private static FaqItem CloneFaqItem(FaqItem source) => new()
    {
        Id = source.Id,
        Category = source.Category,
        Question = source.Question,
        Answer = source.Answer,
        Order = source.Order
    };
}
