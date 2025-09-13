using System;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Models.Sms;

public record BulkSmsRequest(IList<string> Recipients, string Message)
{
    public BulkSmsRequest() : this(new List<string>(), string.Empty) { }
}

public record SmsHistoryItem(string Recipient, string Message, DateTime SentAt);

public record SmsScheduleItem(DateTime ScheduledAt, BulkSmsRequest Request)
{
    public SmsScheduleItem() : this(DateTime.UtcNow, new BulkSmsRequest()) { }
}

