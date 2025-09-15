using System;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Statistics;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface IStatisticsService
{
    Task<StatisticsResult> GetStatisticsAsync(DateTime startDate, DateTime endDate);
}

