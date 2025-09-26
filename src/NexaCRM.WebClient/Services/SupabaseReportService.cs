using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NexaCRM.UI.Models.Supabase;
using NexaCRM.WebClient.Services.Analytics;
using NexaCRM.UI.Services.Interfaces;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;
using ReportData = NexaCRM.UI.Models.ReportData;
using ReportDefinition = NexaCRM.UI.Models.ReportDefinition;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseReportService : IReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan SnapshotTtl = TimeSpan.FromHours(1);
    private static readonly TimeSpan SnapshotRetention = TimeSpan.FromDays(90);

    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseReportService> _logger;

    public SupabaseReportService(SupabaseClientProvider clientProvider, ILogger<SupabaseReportService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task SaveReportDefinitionAsync(ReportDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var userId = EnsureUserId(client);
            await EnsureDefinitionRecordAsync(client, userId, definition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save report definition {ReportName}.", definition.Name);
            throw;
        }
    }

    public async System.Threading.Tasks.Task<IEnumerable<ReportDefinition>> GetReportDefinitionsAsync()
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var userId = EnsureUserId(client);

            var response = await client.From<ReportDefinitionRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
                .Order(x => x.UpdatedAt, PostgrestOrdering.Descending)
                .Get();

            var models = response.Models ?? new List<ReportDefinitionRecord>();
            return models.Select(MapToDefinition).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load report definitions from Supabase.");
            throw;
        }
    }

    public async System.Threading.Tasks.Task<ReportData> GenerateReportAsync(ReportDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var userId = EnsureUserId(client);
            var record = await EnsureDefinitionRecordAsync(client, userId, definition);

            var data = await LoadAnalyticsSourceDataAsync(client);
            var stageLookup = data.DealStages.ToDictionary(stage => stage.Id, stage => stage.Name);
            var filters = definition.Filters ?? new Dictionary<string, string>();
            var filtered = AnalyticsCalculations.ApplyFilters(
                data.Deals,
                stageLookup,
                data.SupportTickets,
                data.Tasks,
                filters);

            var context = AnalyticsCalculations.BuildContext(
                data.Contacts.Count,
                filtered.Deals,
                stageLookup,
                filtered.SupportTickets,
                filtered.Tasks,
                DateTime.UtcNow);

            var metrics = AnalyticsCalculations.CalculateDefinitionMetrics(
                context,
                definition.SelectedFields ?? Enumerable.Empty<string>());

            var snapshot = new ReportSnapshotRecord
            {
                DefinitionId = record.Id,
                GeneratedAt = DateTime.UtcNow,
                PayloadJson = SerializeDictionary(metrics),
                Format = "json",
                CreatedBy = userId
            };

            await client.From<ReportSnapshotRecord>().Insert(snapshot);
            await PruneSnapshotsAsync(client, userId);

            return new ReportData
            {
                Title = string.IsNullOrWhiteSpace(definition.Name) ? "Custom Report" : definition.Name,
                Data = metrics
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate report {ReportName}.", definition.Name);
            throw;
        }
    }

    public System.Threading.Tasks.Task<ReportData> GetQuarterlyPerformanceAsync() =>
        GetCachedReportAsync(
            "quarterly_performance",
            "Quarterly Performance",
            async (client, data) =>
            {
                var metrics = AnalyticsCalculations.CalculateQuarterlyPerformance(data.Deals);
                return new ReportData
                {
                    Title = "Revenue By Quarter",
                    Data = metrics
                };
            });

    public System.Threading.Tasks.Task<ReportData> GetLeadSourceAnalyticsAsync() =>
        GetCachedReportAsync(
            "lead_source",
            "Lead Source Analytics",
            async (client, data) =>
            {
                var metrics = AnalyticsCalculations.CalculateLeadSource(data.Activities);
                return new ReportData
                {
                    Title = "Leads By Source",
                    Data = metrics
                };
            });

    public System.Threading.Tasks.Task<ReportData> GetTicketVolumeAsync() =>
        GetCachedReportAsync(
            "ticket_volume",
            "Ticket Volume",
            async (client, data) => new ReportData
            {
                Title = "Ticket Volume",
                Data = AnalyticsCalculations.CalculateTicketVolume(data.SupportTickets)
            });

    public System.Threading.Tasks.Task<ReportData> GetResolutionRateAsync() =>
        GetCachedReportAsync(
            "ticket_resolution",
            "Resolution Rate",
            async (client, data) => new ReportData
            {
                Title = "Resolution Rate",
                Data = AnalyticsCalculations.CalculateResolutionRate(data.SupportTickets)
            });

    public System.Threading.Tasks.Task<ReportData> GetTicketsByCategoryAsync() =>
        GetCachedReportAsync(
            "ticket_category",
            "Tickets By Category",
            async (client, data) => new ReportData
            {
                Title = "Tickets By Category",
                Data = AnalyticsCalculations.CalculateTicketsByCategory(data.SupportTickets)
            });

    private async System.Threading.Tasks.Task<ReportDefinitionRecord> EnsureDefinitionRecordAsync(
        Supabase.Client client,
        Guid userId,
        ReportDefinition definition)
    {
        var normalizedName = string.IsNullOrWhiteSpace(definition.Name)
            ? "Custom Report"
            : definition.Name.Trim();

        var selectedFields = definition.SelectedFields?
            .Where(field => !string.IsNullOrWhiteSpace(field))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? Array.Empty<string>();

        var filtersJson = SerializeFilters(definition.Filters);

        var response = await client.From<ReportDefinitionRecord>()
            .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
            .Filter(x => x.Name, PostgrestOperator.Equals, normalizedName)
            .Get();

        var record = response.Models.FirstOrDefault();
        if (record is null)
        {
            var newRecord = new ReportDefinitionRecord
            {
                UserId = userId,
                Name = normalizedName,
                SelectedFields = selectedFields,
                FiltersJson = filtersJson
            };

            var insertResponse = await client.From<ReportDefinitionRecord>().Insert(newRecord);
            return insertResponse.Models.FirstOrDefault() ?? newRecord;
        }

        record.SelectedFields = selectedFields;
        record.FiltersJson = filtersJson;

        var updateResponse = await client.From<ReportDefinitionRecord>()
            .Filter(x => x.Id, PostgrestOperator.Equals, record.Id)
            .Update(record);

        return updateResponse.Models.FirstOrDefault() ?? record;
    }

    private async System.Threading.Tasks.Task<ReportData> GetCachedReportAsync(
        string format,
        string retentionKey,
        Func<Supabase.Client, AnalyticsSourceData, System.Threading.Tasks.Task<ReportData>> generator)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var userId = EnsureUserId(client);

            var response = await client.From<ReportSnapshotRecord>()
                .Filter(x => x.CreatedBy, PostgrestOperator.Equals, userId)
                .Filter(x => x.Format, PostgrestOperator.Equals, format)
                .Order(x => x.GeneratedAt, PostgrestOrdering.Descending)
                .Limit(1)
                .Get();

            var existing = response.Models.FirstOrDefault();
            if (existing is not null && DateTime.UtcNow - existing.GeneratedAt <= SnapshotTtl)
            {
                var payload = DeserializePayload(existing.PayloadJson);
                return new ReportData { Title = ResolveTitle(format), Data = payload };
            }

            var data = await LoadAnalyticsSourceDataAsync(client);
            var report = await generator(client, data);

            var snapshot = new ReportSnapshotRecord
            {
                DefinitionId = null,
                GeneratedAt = DateTime.UtcNow,
                PayloadJson = SerializeDictionary(report.Data ?? new Dictionary<string, double>()),
                Format = format,
                CreatedBy = userId,
                MetricsSummaryJson = retentionKey
            };

            await client.From<ReportSnapshotRecord>().Insert(snapshot);
            await PruneSnapshotsAsync(client, userId);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load cached report for {Format}.", format);
            throw;
        }
    }

    private async System.Threading.Tasks.Task<AnalyticsSourceData> LoadAnalyticsSourceDataAsync(Supabase.Client client)
    {
        var dealsTask = client.From<DealRecord>().Get();
        var stagesTask = client.From<DealStageRecord>().Get();
        var contactsTask = client.From<ContactRecord>().Get();
        var ticketsTask = client.From<SupportTicketRecord>().Get();
        var tasksTask = client.From<TaskRecord>().Get();
        var activitiesTask = client.From<ActivityRecord>().Get();

        await System.Threading.Tasks.Task.WhenAll(
            dealsTask,
            stagesTask,
            contactsTask,
            ticketsTask,
            tasksTask,
            activitiesTask);

        return new AnalyticsSourceData(
            dealsTask.Result.Models ?? new List<DealRecord>(),
            stagesTask.Result.Models ?? new List<DealStageRecord>(),
            contactsTask.Result.Models ?? new List<ContactRecord>(),
            ticketsTask.Result.Models ?? new List<SupportTicketRecord>(),
            tasksTask.Result.Models ?? new List<TaskRecord>(),
            activitiesTask.Result.Models ?? new List<ActivityRecord>());
    }

    private async System.Threading.Tasks.Task PruneSnapshotsAsync(Supabase.Client client, Guid userId)
    {
        var cutoff = DateTime.UtcNow - SnapshotRetention;
        try
        {
            await client.From<ReportSnapshotRecord>()
                .Filter(x => x.CreatedBy, PostgrestOperator.Equals, userId)
                .Filter(x => x.GeneratedAt, PostgrestOperator.LessThan, cutoff.ToString("O"))
                .Delete();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to prune old report snapshots for user {UserId}.", userId);
        }
    }

    private static ReportDefinition MapToDefinition(ReportDefinitionRecord record)
    {
        var filters = string.IsNullOrWhiteSpace(record.FiltersJson)
            ? new Dictionary<string, string>()
            : JsonSerializer.Deserialize<Dictionary<string, string>>(record.FiltersJson, JsonOptions)
              ?? new Dictionary<string, string>();

        return new ReportDefinition
        {
            Name = record.Name,
            SelectedFields = record.SelectedFields?.ToList() ?? new List<string>(),
            Filters = filters
        };
    }

    private static string? SerializeFilters(IDictionary<string, string>? filters)
    {
        if (filters is null || filters.Count == 0)
        {
            return null;
        }

        return JsonSerializer.Serialize(filters, JsonOptions);
    }

    private static string SerializeDictionary(IReadOnlyDictionary<string, double> data)
        => JsonSerializer.Serialize(data, JsonOptions);

    private static Dictionary<string, double> DeserializePayload(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return new Dictionary<string, double>();
        }

        return JsonSerializer.Deserialize<Dictionary<string, double>>(payload, JsonOptions)
               ?? new Dictionary<string, double>();
    }

    private static Guid EnsureUserId(Supabase.Client client)
    {
        var userId = client.Auth.CurrentUser?.Id;
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsed))
        {
            throw new InvalidOperationException("Supabase user id is required to perform analytics operations.");
        }

        return parsed;
    }

    private static string ResolveTitle(string format) => format switch
    {
        "quarterly_performance" => "Quarterly Performance",
        "lead_source" => "Lead Source Analytics",
        "ticket_volume" => "Ticket Volume",
        "ticket_resolution" => "Resolution Rate",
        "ticket_category" => "Tickets By Category",
        _ => "Analytics"
    };

    private sealed record AnalyticsSourceData(
        IReadOnlyList<DealRecord> Deals,
        IReadOnlyList<DealStageRecord> DealStages,
        IReadOnlyList<ContactRecord> Contacts,
        IReadOnlyList<SupportTicketRecord> SupportTickets,
        IReadOnlyList<TaskRecord> Tasks,
        IReadOnlyList<ActivityRecord> Activities);
}
