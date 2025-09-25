using System;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Models.Statistics;

namespace NexaCRM.Services.Admin.Interfaces;

public interface IStatisticsService
{
    Task<StatisticsResult> GetStatisticsAsync(DateTime startDate, DateTime endDate);
}

