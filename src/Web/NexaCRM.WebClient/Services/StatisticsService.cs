using NexaCRM.WebClient.Models.Statistics;
using NexaCRM.WebClient.Services.Interfaces;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services;

public class StatisticsService : IStatisticsService
{
    public Task<StatisticsSummary> GetStatisticsAsync(string? companyId = null, string? teamId = null, string? memberId = null)
    {
        var members = 100;
        var logins = 200;
        var downloads = 50;

        if (!string.IsNullOrEmpty(companyId)) members += 10;
        if (!string.IsNullOrEmpty(teamId)) logins += 20;
        if (!string.IsNullOrEmpty(memberId)) downloads += 5;

        return Task.FromResult(new StatisticsSummary(members, logins, downloads));
    }
}

