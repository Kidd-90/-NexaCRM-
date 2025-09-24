using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.CustomerCenter;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services;

public class FaqService : IFaqService
{
    private readonly List<FaqItem> faqs = new();
    private readonly object faqLock = new();

    public Task<List<FaqItem>> GetFaqsAsync()
    {
        lock (faqLock)
        {
            return Task.FromResult(
                faqs.OrderBy(f => f.Order)
                    .Select(f => new FaqItem
                    {
                        Id = f.Id,
                        Category = f.Category,
                        Question = f.Question,
                        Answer = f.Answer,
                        Order = f.Order
                    })
                    .ToList());
        }
    }

    public Task SaveFaqAsync(FaqItem item)
    {
        lock (faqLock)
        {
            var existing = faqs.FirstOrDefault(f => f.Id == item.Id);
            if (existing is null)
            {
                item.Id = faqs.Count == 0 ? 1 : faqs.Max(f => f.Id) + 1;
                item.Order = faqs.Count;
                faqs.Add(new FaqItem
                {
                    Id = item.Id,
                    Category = item.Category,
                    Question = item.Question,
                    Answer = item.Answer,
                    Order = item.Order
                });
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
        lock (faqLock)
        {
            int index = 0;
            foreach (var item in items)
            {
                var existing = faqs.FirstOrDefault(f => f.Id == item.Id);
                if (existing != null)
                {
                    existing.Order = index++;
                }
            }
            faqs.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        return Task.CompletedTask;
    }
}

