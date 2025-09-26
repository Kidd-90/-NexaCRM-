using System;
using System.Collections.Generic;

namespace NexaCRM.Services.Admin.Models.Statistics;

public record StatisticsSummary(int TotalMembers, int TotalLogins, int TotalDownloads);

public record TrendPoint(DateTime Date, int Value);

public record StatisticsResult(
    StatisticsSummary Summary,
    List<TrendPoint> LoginTrend,
    List<TrendPoint> DownloadTrend);

