using NexaCRM.WebClient.Models.Statistics;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface IStatisticsService
{
    Task<StatisticsSummary> GetStatisticsAsync(string? companyId = null, string? teamId = null, string? memberId = null);
}

