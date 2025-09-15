using NexaCRM.WebClient.Models.CustomerCenter;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface IFaqService
{
    Task<List<FaqItem>> GetFaqsAsync();
    Task SaveFaqAsync(FaqItem item);
    Task ReorderFaqsAsync(IEnumerable<FaqItem> items);
}

