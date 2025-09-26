using NexaCRM.Services.Admin.Models.CustomerCenter;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.Services.Admin.Interfaces;

public interface ICustomerCenterService
{
    Task<IEnumerable<Notice>> GetNoticesAsync();
    Task SaveNoticeAsync(Notice notice);
    Task<IEnumerable<FaqItem>> GetFaqItemsAsync();
    Task SaveFaqItemAsync(FaqItem item);
}

