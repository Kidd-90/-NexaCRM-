using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.CustomerCenter;

namespace NexaCRM.WebClient.Services.Admin;

public sealed class FaqService : IFaqService
{
    private readonly List<FaqItem> _faqItems = new()
    {
        new FaqItem(1, "계정", "로그인을 어떻게 하나요?", "이메일과 비밀번호로 로그인할 수 있습니다."),
        new FaqItem(2, "결제", "결제 수단은 무엇이 있나요?", "신용카드와 계좌이체를 지원합니다."),
    };

    private int _nextId = 3;

    public Task<IEnumerable<FaqItem>> GetFaqItemsAsync() =>
        Task.FromResult<IEnumerable<FaqItem>>(_faqItems);

    public Task<FaqItem?> GetFaqItemAsync(int id) =>
        Task.FromResult(_faqItems.FirstOrDefault(f => f.Id == id));

    public Task CreateFaqItemAsync(FaqItem item)
    {
        var newItem = item with { Id = _nextId++ };
        _faqItems.Add(newItem);
        return Task.CompletedTask;
    }

    public Task UpdateFaqItemAsync(FaqItem item)
    {
        var index = _faqItems.FindIndex(f => f.Id == item.Id);
        if (index >= 0)
        {
            _faqItems[index] = item;
        }
        return Task.CompletedTask;
    }

    public Task DeleteFaqItemAsync(int id)
    {
        var existing = _faqItems.FirstOrDefault(f => f.Id == id);
        if (existing is not null)
        {
            _faqItems.Remove(existing);
        }
        return Task.CompletedTask;
    }
}
