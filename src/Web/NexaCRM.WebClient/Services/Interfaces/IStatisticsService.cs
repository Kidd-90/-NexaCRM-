using NexaCRM.WebClient.Models.Statistics;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface IStatisticsService
{
    Task<StatisticsSummary> GetStatisticsAsync();
}

