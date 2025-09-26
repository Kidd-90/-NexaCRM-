using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NexaCRM.UI.Models.Supabase;

namespace NexaCRM.WebClient.Services.Analytics;

public static class AnalyticsCalculations
{
    public sealed record AnalyticsContext(
        int ContactCount,
        int DealCount,
        int OpenDeals,
        int WonDeals,
        int LostDeals,
        int TotalTasks,
        int ActiveTasks,
        int CompletedTasks,
        int OverdueTasks,
        int OpenTickets,
        int ClosedTickets,
        double AverageResponseHours,
        double AverageResolutionHours);

    public sealed record FilteredAnalyticsData(
        IReadOnlyList<DealRecord> Deals,
        IReadOnlyList<SupportTicketRecord> SupportTickets,
        IReadOnlyList<TaskRecord> Tasks);

    public sealed record DailyActivityStats(int LoginCount, int DownloadCount, int ActiveUsers);

    private static readonly Dictionary<string, Func<AnalyticsContext, double>> MetricMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["totalcontacts"] = context => context.ContactCount,
            ["contacts"] = context => context.ContactCount,
            ["totaldeals"] = context => context.DealCount,
            ["deals"] = context => context.DealCount,
            ["opendeals"] = context => context.OpenDeals,
            ["wondeals"] = context => context.WonDeals,
            ["lostdeals"] = context => context.LostDeals,
            ["totaltasks"] = context => context.TotalTasks,
            ["activetasks"] = context => context.ActiveTasks,
            ["completedtasks"] = context => context.CompletedTasks,
            ["overduetasks"] = context => context.OverdueTasks,
            ["opentickets"] = context => context.OpenTickets,
            ["closedtickets"] = context => context.ClosedTickets,
            ["avgresponsehours"] = context => context.AverageResponseHours,
            ["avgresolutionhours"] = context => context.AverageResolutionHours
        };

    public static FilteredAnalyticsData ApplyFilters(
        IReadOnlyList<DealRecord> deals,
        IDictionary<int, string> stageLookup,
        IReadOnlyList<SupportTicketRecord> tickets,
        IReadOnlyList<TaskRecord> tasks,
        IReadOnlyDictionary<string, string> filters)
    {
        if (filters.Count == 0)
        {
            return new FilteredAnalyticsData(deals, tickets, tasks);
        }

        IEnumerable<DealRecord> filteredDeals = deals;
        IEnumerable<SupportTicketRecord> filteredTickets = tickets;
        IEnumerable<TaskRecord> filteredTasks = tasks;

        foreach (var (key, value) in filters)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var normalizedKey = NormalizeKey(key);
            switch (normalizedKey)
            {
                case "stage":
                    filteredDeals = filteredDeals.Where(deal =>
                    {
                        if (!stageLookup.TryGetValue(deal.StageId, out var stage))
                        {
                            return false;
                        }

                        return stage.Contains(value, StringComparison.OrdinalIgnoreCase);
                    });
                    break;
                case "ticketstatus":
                case "status":
                    filteredTickets = filteredTickets.Where(ticket =>
                        ticket.Status?.Contains(value, StringComparison.OrdinalIgnoreCase) == true);
                    break;
                case "ticketcategory":
                case "category":
                    filteredTickets = filteredTickets.Where(ticket =>
                        ticket.Category?.Contains(value, StringComparison.OrdinalIgnoreCase) == true);
                    break;
                case "taskpriority":
                case "priority":
                    filteredTasks = filteredTasks.Where(task =>
                        task.Priority?.Contains(value, StringComparison.OrdinalIgnoreCase) == true);
                    break;
                case "assignedto":
                    filteredTasks = filteredTasks.Where(task =>
                        task.AssignedToName?.Contains(value, StringComparison.OrdinalIgnoreCase) == true);
                    filteredTickets = filteredTickets.Where(ticket =>
                        ticket.AgentName?.Contains(value, StringComparison.OrdinalIgnoreCase) == true);
                    break;
            }
        }

        return new FilteredAnalyticsData(
            filteredDeals.ToList(),
            filteredTickets.ToList(),
            filteredTasks.ToList());
    }

    public static AnalyticsContext BuildContext(
        int contactCount,
        IReadOnlyList<DealRecord> deals,
        IDictionary<int, string> stageLookup,
        IReadOnlyList<SupportTicketRecord> tickets,
        IReadOnlyList<TaskRecord> tasks,
        DateTime today)
    {
        var normalizedStageLookup = stageLookup.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            EqualityComparer<int>.Default);

        static bool IsWonStage(string? stageName) =>
            !string.IsNullOrWhiteSpace(stageName) &&
            stageName.Contains("won", StringComparison.OrdinalIgnoreCase);

        static bool IsLostStage(string? stageName) =>
            !string.IsNullOrWhiteSpace(stageName) &&
            stageName.Contains("lost", StringComparison.OrdinalIgnoreCase);

        bool IsClosedStage(string? stageName) => IsWonStage(stageName) || IsLostStage(stageName);

        var dealCount = deals.Count;
        var openDeals = deals.Count(deal =>
        {
            if (!normalizedStageLookup.TryGetValue(deal.StageId, out var stageName))
            {
                return false;
            }

            return !IsClosedStage(stageName);
        });
        var wonDeals = deals.Count(deal =>
            normalizedStageLookup.TryGetValue(deal.StageId, out var stageName) &&
            IsWonStage(stageName));
        var lostDeals = deals.Count(deal =>
            normalizedStageLookup.TryGetValue(deal.StageId, out var stageName) &&
            IsLostStage(stageName));

        var totalTasks = tasks.Count;
        var completedTasks = tasks.Count(task => task.IsCompleted);
        var activeTasks = totalTasks - completedTasks;
        var overdueTasks = tasks.Count(task =>
            !task.IsCompleted && task.DueDate is not null && task.DueDate.Value.Date < today.Date);

        static bool IsClosedTicket(string? status) =>
            !string.IsNullOrWhiteSpace(status) &&
            (status.Contains("closed", StringComparison.OrdinalIgnoreCase) ||
             status.Contains("resolved", StringComparison.OrdinalIgnoreCase));

        var openTickets = tickets.Count(ticket => !IsClosedTicket(ticket.Status));
        var closedTickets = tickets.Count(ticket => IsClosedTicket(ticket.Status));

        double AverageHours(IEnumerable<SupportTicketRecord> source, Func<SupportTicketRecord, TimeSpan?> selector)
        {
            var durations = source
                .Select(selector)
                .Where(duration => duration.HasValue)
                .Select(duration => duration!.Value.TotalHours)
                .ToList();

            return durations.Count == 0 ? 0 : Math.Round(durations.Average(), 2);
        }

        var avgResponseHours = AverageHours(
            tickets,
            ticket => ticket.LastReplyAt.HasValue && ticket.CreatedAt.HasValue
                ? ticket.LastReplyAt.Value - ticket.CreatedAt.Value
                : null);

        var avgResolutionHours = AverageHours(
            tickets.Where(ticket => IsClosedTicket(ticket.Status)),
            ticket => ticket.UpdatedAt.HasValue && ticket.CreatedAt.HasValue
                ? ticket.UpdatedAt.Value - ticket.CreatedAt.Value
                : null);

        return new AnalyticsContext(
            contactCount,
            dealCount,
            openDeals,
            wonDeals,
            lostDeals,
            totalTasks,
            activeTasks,
            completedTasks,
            overdueTasks,
            openTickets,
            closedTickets,
            avgResponseHours,
            avgResolutionHours);
    }

    public static Dictionary<string, double> CalculateDefinitionMetrics(
        AnalyticsContext context,
        IEnumerable<string> fields)
    {
        var results = new Dictionary<string, double>();
        foreach (var field in fields)
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                continue;
            }

            var normalized = NormalizeKey(field);
            if (MetricMap.TryGetValue(normalized, out var extractor))
            {
                results[field] = extractor(context);
            }
            else
            {
                results[field] = 0;
            }
        }

        return results;
    }

    public static Dictionary<string, double> CalculateQuarterlyPerformance(
        IEnumerable<DealRecord> deals)
    {
        return deals
            .GroupBy(deal => GetQuarterKey(deal.CreatedAt))
            .OrderBy(group => group.Key)
            .ToDictionary(
                group => group.Key,
                group => Math.Round(group.Sum(deal => (double?)deal.Value ?? 0D), 2));
    }

    public static Dictionary<string, double> CalculateLeadSource(
        IEnumerable<ActivityRecord> activities)
    {
        return activities
            .GroupBy(activity => string.IsNullOrWhiteSpace(activity.Type)
                ? "Unknown"
                : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(activity.Type.ToLowerInvariant()))
            .OrderBy(group => group.Key)
            .ToDictionary(group => group.Key, group => (double)group.Count());
    }

    public static Dictionary<string, double> CalculateTicketVolume(
        IEnumerable<SupportTicketRecord> tickets)
    {
        return tickets
            .GroupBy(ticket => string.IsNullOrWhiteSpace(ticket.Status) ? "Unknown" : ticket.Status)
            .OrderBy(group => group.Key)
            .ToDictionary(group => group.Key, group => (double)group.Count());
    }

    public static Dictionary<string, double> CalculateResolutionRate(
        IEnumerable<SupportTicketRecord> tickets)
    {
        var ticketList = tickets.ToList();
        if (ticketList.Count == 0)
        {
            return new Dictionary<string, double> { ["Rate"] = 0 };
        }

        static bool IsClosed(string? status) =>
            !string.IsNullOrWhiteSpace(status) &&
            (status.Contains("closed", StringComparison.OrdinalIgnoreCase) ||
             status.Contains("resolved", StringComparison.OrdinalIgnoreCase));

        var closed = ticketList.Count(ticket => IsClosed(ticket.Status));
        var rate = ticketList.Count == 0 ? 0 : (double)closed / ticketList.Count;
        return new Dictionary<string, double> { ["Rate"] = Math.Round(rate, 3) };
    }

    public static Dictionary<string, double> CalculateTicketsByCategory(
        IEnumerable<SupportTicketRecord> tickets)
    {
        return tickets
            .GroupBy(ticket => string.IsNullOrWhiteSpace(ticket.Category) ? "Uncategorized" : ticket.Category)
            .OrderBy(group => group.Key)
            .ToDictionary(group => group.Key, group => (double)group.Count());
    }

    public static DailyActivityStats CalculateDailyActivityStats(IEnumerable<ActivityRecord> activities)
    {
        var loginCount = activities.Count(activity =>
            activity.Type?.Contains("login", StringComparison.OrdinalIgnoreCase) == true);
        var downloadCount = activities.Count(activity =>
            activity.Type?.Contains("download", StringComparison.OrdinalIgnoreCase) == true);
        var activeUsers = activities
            .Select(activity => activity.UserId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .Count();

        return new DailyActivityStats(loginCount, downloadCount, activeUsers);
    }

    public static string NormalizeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var characters = value
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray();
        return new string(characters);
    }

    private static string GetQuarterKey(DateTime timestamp)
    {
        var quarter = (timestamp.Month - 1) / 3 + 1;
        return $"{timestamp.Year}-Q{quarter}";
    }
}
