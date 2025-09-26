using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface IDealService
    {
        Task<IEnumerable<Deal>> GetDealsAsync();
        Task<IReadOnlyList<DealStage>> GetDealStagesAsync(CancellationToken cancellationToken = default);
        Task<Deal> CreateDealAsync(DealCreateRequest request, CancellationToken cancellationToken = default);
    }
}
