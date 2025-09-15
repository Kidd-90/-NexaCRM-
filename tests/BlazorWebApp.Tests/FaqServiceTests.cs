using System.Linq;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.CustomerCenter;
using NexaCRM.WebClient.Services;

namespace BlazorWebApp.Tests;

public class FaqServiceTests
{
    [Fact]
    public async Task ReorderFaqsAsync_ChangesOrder()
    {
        var service = new FaqService();
        await service.SaveFaqAsync(new FaqItem { Category = "General", Question = "Q1", Answer = "A1" });
        await service.SaveFaqAsync(new FaqItem { Category = "General", Question = "Q2", Answer = "A2" });

        var items = await service.GetFaqsAsync();
        items.Reverse();
        await service.ReorderFaqsAsync(items);
        var reordered = await service.GetFaqsAsync();
        Assert.Equal("Q2", reordered.First().Question);
    }

    [Fact]
    public async Task SaveFaqAsync_UpdatesExistingItem()
    {
        var service = new FaqService();
        await service.SaveFaqAsync(new FaqItem { Category = "General", Question = "Q1", Answer = "A1" });
        var existing = (await service.GetFaqsAsync()).First();

        await service.SaveFaqAsync(new FaqItem
        {
            Id = existing.Id,
            Category = existing.Category,
            Question = existing.Question,
            Answer = "Updated"
        });

        var updated = (await service.GetFaqsAsync()).First();
        Assert.Equal("Updated", updated.Answer);
    }
}

