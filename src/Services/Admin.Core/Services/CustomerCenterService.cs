using NexaCRM.WebClient.Models.CustomerCenter;
using NexaCRM.WebClient.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services;

public class CustomerCenterService : ICustomerCenterService
{
    public Task<IEnumerable<Notice>> GetNoticesAsync() =>
        Task.FromResult<IEnumerable<Notice>>(new List<Notice>());

    public Task SaveNoticeAsync(Notice notice) =>
        Task.CompletedTask;

    public Task<IEnumerable<FaqItem>> GetFaqItemsAsync() =>
        Task.FromResult<IEnumerable<FaqItem>>(new List<FaqItem>());

    public Task SaveFaqItemAsync(FaqItem item) =>
        Task.CompletedTask;
}

