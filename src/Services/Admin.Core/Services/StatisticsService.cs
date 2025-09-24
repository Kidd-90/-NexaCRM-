using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Statistics;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services;

public class StatisticsService : IStatisticsService
{
    public Task<StatisticsResult> GetStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        var rand = new Random();
        var dates = Enumerable.Range(0, (endDate - startDate).Days + 1)
            .Select(offset => startDate.AddDays(offset))
            .ToList();

        var loginTrend = dates
            .Select(d => new TrendPoint(d, rand.Next(0, 100)))
            .ToList();

        var downloadTrend = dates
            .Select(d => new TrendPoint(d, rand.Next(0, 50)))
            .ToList();

        var summary = new StatisticsSummary(
            TotalMembers: rand.Next(1000, 10000),
            TotalLogins: loginTrend.Sum(p => p.Value),
            TotalDownloads: downloadTrend.Sum(p => p.Value)
        );

        return Task.FromResult(new StatisticsResult(summary, loginTrend, downloadTrend));
    }
}

