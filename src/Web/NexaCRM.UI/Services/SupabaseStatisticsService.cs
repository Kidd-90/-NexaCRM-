using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.Services.Admin.Models.Statistics;
using NexaCRM.WebClient.Models.Supabase;
using NexaCRM.WebClient.Services.Analytics;
using NexaCRM.Services.Admin.Interfaces;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseStatisticsService : IStatisticsService
{
    private static readonly TimeSpan StatisticsTtl = TimeSpan.FromHours(12);
    private static readonly TimeSpan StatisticsRetention = TimeSpan.FromDays(365);

    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseStatisticsService> _logger;

    public SupabaseStatisticsService(SupabaseClientProvider clientProvider, ILogger<SupabaseStatisticsService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<StatisticsResult> GetStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            (startDate, endDate) = (endDate, startDate);
        }

        startDate = startDate.Date;
        endDate = endDate.Date;

        try
        {
            var client = await _clientProvider.GetClientAsync();
            _ = EnsureUserId(client);

            var existingRecords = await LoadStatisticsRecordsAsync(client, startDate, endDate);
            var recordLookup = existingRecords.ToDictionary(record => record.MetricDate.Date);

            var totalMembers = await LoadTotalMembersAsync(client);

            for (var current = startDate; current <= endDate; current = current.AddDays(1))
            {
                if (!recordLookup.TryGetValue(current, out var record) || IsStale(record))
                {
                    var refreshed = await BuildDailyRecordAsync(client, current, totalMembers);
                    recordLookup[current] = refreshed;
                }
            }

            await PruneStatisticsAsync(client, endDate);

            var orderedRecords = recordLookup.Values
                .OrderBy(record => record.MetricDate)
                .ToList();

            return BuildResult(orderedRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load statistics for range {Start} to {End}.", startDate, endDate);
            throw;
        }
    }

    private async Task<List<StatisticsDailyRecord>> LoadStatisticsRecordsAsync(
        Supabase.Client client,
        DateTime startDate,
        DateTime endDate)
    {
        var response = await client.From<StatisticsDailyRecord>()
            .Filter(x => x.MetricDate, PostgrestOperator.GreaterThanOrEqual, startDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .Filter(x => x.MetricDate, PostgrestOperator.LessThanOrEqual, endDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .Order(x => x.MetricDate, PostgrestOrdering.Ascending)
            .Get();

        return response.Models ?? new List<StatisticsDailyRecord>();
    }

    private async Task<StatisticsDailyRecord> BuildDailyRecordAsync(
        Supabase.Client client,
        DateTime date,
        int totalMembers)
    {
        var nextDate = date.AddDays(1);
        var response = await client.From<ActivityRecord>()
            .Filter(x => x.ActivityDate, PostgrestOperator.GreaterThanOrEqual, date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .Filter(x => x.ActivityDate, PostgrestOperator.LessThan, nextDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .Get();

        var activities = response.Models ?? new List<ActivityRecord>();
        var stats = AnalyticsCalculations.CalculateDailyActivityStats(activities);

        var record = new StatisticsDailyRecord
        {
            MetricDate = date,
            TotalMembers = totalMembers,
            TotalLogins = stats.LoginCount,
            TotalDownloads = stats.DownloadCount,
            ActiveUsers = stats.ActiveUsers,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await UpsertStatisticsRecordAsync(client, record);
    }

    private static async Task<StatisticsDailyRecord> UpsertStatisticsRecordAsync(
        Supabase.Client client,
        StatisticsDailyRecord record)
    {
        var existingResponse = await client.From<StatisticsDailyRecord>()
            .Filter(x => x.MetricDate, PostgrestOperator.Equals, record.MetricDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .Limit(1)
            .Get();

        var existing = existingResponse.Models.FirstOrDefault();
        if (existing is null)
        {
            var insertResponse = await client.From<StatisticsDailyRecord>().Insert(record);
            return insertResponse.Models.FirstOrDefault() ?? record;
        }

        record.Id = existing.Id;
        var updateResponse = await client.From<StatisticsDailyRecord>()
            .Filter(x => x.Id, PostgrestOperator.Equals, existing.Id)
            .Update(record);

        return updateResponse.Models.FirstOrDefault() ?? record;
    }

    private async Task<int> LoadTotalMembersAsync(Supabase.Client client)
    {
        var response = await client.From<OrganizationUserRecord>()
            .Filter(x => x.Status, PostgrestOperator.Equals, "approved")
            .Get();

        var models = response.Models ?? new List<OrganizationUserRecord>();
        return models.Count;
    }

    private async Task PruneStatisticsAsync(Supabase.Client client, DateTime referenceDate)
    {
        var cutoff = referenceDate.Date - StatisticsRetention;
        if (cutoff <= DateTime.MinValue.AddDays(1))
        {
            return;
        }

        try
        {
            await client.From<StatisticsDailyRecord>()
                .Filter(x => x.MetricDate, PostgrestOperator.LessThan, cutoff.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                .Delete();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to prune statistics before {Cutoff}.", cutoff);
        }
    }

    private static StatisticsResult BuildResult(IReadOnlyList<StatisticsDailyRecord> records)
    {
        if (records.Count == 0)
        {
            return new StatisticsResult(
                new StatisticsSummary(0, 0, 0),
                new List<TrendPoint>(),
                new List<TrendPoint>());
        }

        var ordered = records.OrderBy(record => record.MetricDate).ToList();

        var summary = new StatisticsSummary(
            ordered.Last().TotalMembers,
            ordered.Sum(record => record.TotalLogins),
            ordered.Sum(record => record.TotalDownloads));

        var loginTrend = ordered
            .Select(record => new TrendPoint(record.MetricDate, record.TotalLogins))
            .ToList();

        var downloadTrend = ordered
            .Select(record => new TrendPoint(record.MetricDate, record.TotalDownloads))
            .ToList();

        return new StatisticsResult(summary, loginTrend, downloadTrend);
    }

    private static bool IsStale(StatisticsDailyRecord record)
    {
        if (record.UpdatedAt == default)
        {
            return true;
        }

        return DateTime.UtcNow - record.UpdatedAt > StatisticsTtl;
    }

    private static Guid EnsureUserId(Supabase.Client client)
    {
        var userId = client.Auth.CurrentUser?.Id;
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsed))
        {
            throw new InvalidOperationException("Supabase user id is required to load statistics.");
        }

        return parsed;
    }
}
