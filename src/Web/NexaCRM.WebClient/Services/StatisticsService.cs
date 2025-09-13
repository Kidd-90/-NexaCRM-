using NexaCRM.WebClient.Models.Statistics;
using NexaCRM.WebClient.Services.Interfaces;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services;

public class StatisticsService : IStatisticsService
{
    public Task<StatisticsSummary> GetStatisticsAsync() =>
        Task.FromResult(new StatisticsSummary(0, 0, 0));
}

