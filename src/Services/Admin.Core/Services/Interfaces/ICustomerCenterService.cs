using NexaCRM.WebClient.Models.CustomerCenter;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface ICustomerCenterService
{
    Task<IEnumerable<Notice>> GetNoticesAsync();
    Task SaveNoticeAsync(Notice notice);
    Task<IEnumerable<FaqItem>> GetFaqItemsAsync();
    Task SaveFaqItemAsync(FaqItem item);
}

