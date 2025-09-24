using System;
using System.Collections.Generic;
using System.Linq;
using NexaCRM.WebClient.Models.Supabase;
using NexaCRM.WebClient.Services.Analytics;
using Xunit;

namespace NexaCRM.WebClient.UnitTests;

public class AnalyticsCalculationsTests
{
    [Fact]
    public void BuildContext_ComputesAggregateValues()
    {
        var deals = new List<DealRecord>
        {
            new() { Id = 1, StageId = 1, CreatedAt = new DateTime(2024, 1, 5), Value = 5000m },
            new() { Id = 2, StageId = 2, CreatedAt = new DateTime(2024, 2, 3), Value = 12000m },
            new() { Id = 3, StageId = 3, CreatedAt = new DateTime(2024, 2, 10), Value = 3000m }
        };

        var stageLookup = new Dictionary<int, string>
        {
            [1] = "Prospecting",
            [2] = "Won",
            [3] = "Lost"
        };

        var tickets = new List<SupportTicketRecord>
        {
            new()
            {
                Id = 1,
                Status = "Open",
                CreatedAt = new DateTime(2024, 1, 7),
                LastReplyAt = new DateTime(2024, 1, 7, 4, 0, 0)
            },
            new()
            {
                Id = 2,
                Status = "Resolved",
                CreatedAt = new DateTime(2024, 1, 8),
                UpdatedAt = new DateTime(2024, 1, 10)
            }
        };

        var tasks = new List<TaskRecord>
        {
            new() { Id = 1, DueDate = new DateTime(2024, 1, 15), IsCompleted = false, Priority = "High" },
            new() { Id = 2, DueDate = new DateTime(2024, 1, 5), IsCompleted = false, Priority = "Medium" },
            new() { Id = 3, DueDate = new DateTime(2024, 1, 2), IsCompleted = true, Priority = "Low" }
        };

        var context = AnalyticsCalculations.BuildContext(
            contactCount: 12,
            deals,
            stageLookup,
            tickets,
            tasks,
            new DateTime(2024, 1, 15));

        Assert.Equal(12, context.ContactCount);
        Assert.Equal(3, context.DealCount);
        Assert.Equal(1, context.OpenDeals);
        Assert.Equal(1, context.WonDeals);
        Assert.Equal(1, context.LostDeals);
        Assert.Equal(3, context.TotalTasks);
        Assert.Equal(2, context.ActiveTasks);
        Assert.Equal(1, context.CompletedTasks);
        Assert.Equal(1, context.OverdueTasks);
        Assert.Equal(1, context.OpenTickets);
        Assert.Equal(1, context.ClosedTickets);
        Assert.True(context.AverageResponseHours > 0);
    }

    [Fact]
    public void CalculateDefinitionMetrics_ReturnsRecognizedFields()
    {
        var context = new AnalyticsCalculations.AnalyticsContext(
            ContactCount: 10,
            DealCount: 5,
            OpenDeals: 3,
            WonDeals: 1,
            LostDeals: 1,
            TotalTasks: 8,
            ActiveTasks: 5,
            CompletedTasks: 3,
            OverdueTasks: 2,
            OpenTickets: 4,
            ClosedTickets: 2,
            AverageResponseHours: 3.5,
            AverageResolutionHours: 5.1);

        var fields = new[] { "Total Contacts", "Open Deals", "Closed Tickets", "AvgResponseHours", "Unknown" };
        var metrics = AnalyticsCalculations.CalculateDefinitionMetrics(context, fields);

        Assert.Equal(10, metrics["Total Contacts"]);
        Assert.Equal(3, metrics["Open Deals"]);
        Assert.Equal(2, metrics["Closed Tickets"]);
        Assert.Equal(3.5, metrics["AvgResponseHours"]);
        Assert.Equal(0, metrics["Unknown"]);
    }

    [Fact]
    public void CalculateQuarterlyPerformance_GroupsDealsByQuarter()
    {
        var deals = new List<DealRecord>
        {
            new() { Id = 1, StageId = 1, CreatedAt = new DateTime(2024, 1, 1), Value = 2000m },
            new() { Id = 2, StageId = 1, CreatedAt = new DateTime(2024, 2, 15), Value = 1500m },
            new() { Id = 3, StageId = 1, CreatedAt = new DateTime(2024, 4, 10), Value = 3000m }
        };

        var metrics = AnalyticsCalculations.CalculateQuarterlyPerformance(deals);

        Assert.Equal(2, metrics.Count);
        Assert.Equal(3500, metrics["2024-Q1"]);
        Assert.Equal(3000, metrics["2024-Q2"]);
    }

    [Fact]
    public void CalculateLeadSource_GroupsByType()
    {
        var activities = new List<ActivityRecord>
        {
            new() { Id = 1, Type = "login" },
            new() { Id = 2, Type = "LOGIN" },
            new() { Id = 3, Type = "email" },
            new() { Id = 4, Type = null }
        };

        var metrics = AnalyticsCalculations.CalculateLeadSource(activities);

        Assert.Equal(3, metrics.Count);
        Assert.Equal(2, metrics["Login"]);
        Assert.Equal(1, metrics["Email"]);
        Assert.Equal(1, metrics["Unknown"]);
    }

    [Fact]
    public void CalculateDailyActivityStats_ComputesLoginAndDownloadCounts()
    {
        var activities = new List<ActivityRecord>
        {
            new() { Id = 1, Type = "login", UserId = Guid.NewGuid() },
            new() { Id = 2, Type = "download", UserId = Guid.NewGuid() },
            new() { Id = 3, Type = "Download", UserId = Guid.NewGuid() },
            new() { Id = 4, Type = "call", UserId = null }
        };

        var stats = AnalyticsCalculations.CalculateDailyActivityStats(activities);

        Assert.Equal(1, stats.LoginCount);
        Assert.Equal(2, stats.DownloadCount);
        Assert.Equal(3, stats.ActiveUsers);
    }
}
