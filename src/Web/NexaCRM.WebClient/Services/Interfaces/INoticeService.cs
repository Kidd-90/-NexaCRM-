using NexaCRM.WebClient.Models.CustomerCenter;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface INoticeService
{
    Task<IEnumerable<Notice>> GetNoticesAsync();
    Task<Notice?> GetNoticeAsync(int id);
    Task CreateNoticeAsync(Notice notice);
    Task UpdateNoticeAsync(Notice notice);
    Task DeleteNoticeAsync(int id);
}

