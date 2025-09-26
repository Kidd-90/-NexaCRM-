using System;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Models.CustomerCenter;

/// <summary>
/// Snapshot style metrics for the customer center dashboard.
/// </summary>
public sealed class CustomerCenterSummary
{
    public int TotalTickets { get; set; }

    public int OpenTickets { get; set; }

    public int AvgResponseMinutes { get; set; }

    public decimal SatisfactionScore { get; set; }

    public IReadOnlyCollection<string> TopCategories { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Captures individual pieces of qualitative feedback left by customers.
/// </summary>
public sealed class CustomerFeedback
{
    public Guid FeedbackId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string Channel { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime ReceivedAtUtc { get; set; }
}

/// <summary>
/// Extends the service layer FAQ model so pages can reference it from the web client namespace.
/// </summary>
public class FaqItem : NexaCRM.Services.Admin.Models.CustomerCenter.FaqItem
{
    public FaqItem()
    {
    }

    public FaqItem(int id, string category, string question, string answer, int order = 0)
    {
        Id = id;
        Category = category;
        Question = question;
        Answer = answer;
        Order = order;
    }
}
