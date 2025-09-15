using System;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Models.Sms;

public record BulkSmsRequest(IList<string> Recipients, string Message)
{
    public BulkSmsRequest() : this(new List<string>(), string.Empty) { }
}

public record SmsHistoryItem(string Recipient, string Message, DateTime SentAt);

public record SmsScheduleItem(Guid Id, DateTime ScheduledAt, BulkSmsRequest Request, bool IsCancelled = false)
{
    public SmsScheduleItem() : this(Guid.NewGuid(), DateTime.UtcNow, new BulkSmsRequest()) { }
}

