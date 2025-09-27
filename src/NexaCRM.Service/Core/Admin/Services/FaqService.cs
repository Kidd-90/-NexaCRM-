using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.CustomerCenter;

namespace NexaCRM.Services.Admin;

/// <summary>
/// In-memory FAQ store that satisfies the shared administrative service contract.
/// </summary>
public sealed class FaqService : IFaqService
{
    private readonly List<FaqItem> _faqItems = new()
    {
        new() { Id = 1, Category = "계정", Question = "로그인은 어떻게 하나요?", Answer = "이메일과 비밀번호로 로그인할 수 있습니다.", Order = 0 },
        new() { Id = 2, Category = "결제", Question = "결제 수단은 무엇이 있나요?", Answer = "신용카드와 계좌이체를 지원합니다.", Order = 1 },
    };

    private int _nextId = 3;

    public Task<List<FaqItem>> GetFaqsAsync()
    {
        var ordered = _faqItems
            .OrderBy(f => f.Order)
            .ThenBy(f => f.Id)
            .Select(Clone)
            .ToList();

        return Task.FromResult(ordered);
    }

    public Task SaveFaqAsync(FaqItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.Id == 0)
        {
            var newItem = Clone(item);
            newItem.Id = _nextId++;
            newItem.Order = _faqItems.Count;
            _faqItems.Add(newItem);
        }
        else
        {
            var existing = _faqItems.FirstOrDefault(f => f.Id == item.Id);
            if (existing is null)
            {
                var newItem = Clone(item);
                _faqItems.Add(newItem);
                _nextId = Math.Max(_nextId, newItem.Id + 1);
            }
            else
            {
                existing.Category = item.Category;
                existing.Question = item.Question;
                existing.Answer = item.Answer;
            }
        }

        return Task.CompletedTask;
    }

    public Task ReorderFaqsAsync(IEnumerable<FaqItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var newOrder = items
            .Select((item, index) => (item.Id, index))
            .ToDictionary(tuple => tuple.Id, tuple => tuple.index);

        foreach (var faq in _faqItems)
        {
            if (newOrder.TryGetValue(faq.Id, out var order))
            {
                faq.Order = order;
            }
        }

        _faqItems.Sort((left, right) => left.Order.CompareTo(right.Order));
        return Task.CompletedTask;
    }

    private static FaqItem Clone(FaqItem source) => new()
    {
        Id = source.Id,
        Category = source.Category,
        Question = source.Question,
        Answer = source.Answer,
        Order = source.Order
    };
}
