using System;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Models.Sms;

public record BulkSmsRequest(IList<string> Recipients, string Message)
{
    public BulkSmsRequest() : this(new List<string>(), string.Empty) { }
}

public record SmsAttachment(string FileName, long FileSizeBytes, string ContentType);

public record SmsHistoryItem(
    string Recipient,
    string Message,
    DateTime SentAt,
    string Status,
    string SenderNumber = "",
    string RecipientName = "",
    IReadOnlyList<SmsAttachment>? Attachments = null)
{
    public bool HasAttachments => Attachments is { Count: > 0 };
    public int AttachmentCount => Attachments?.Count ?? 0;
}

public record SmsScheduleItem(Guid Id, DateTime ScheduledAt, BulkSmsRequest Request, bool IsCancelled = false)
{
    public SmsScheduleItem() : this(Guid.NewGuid(), DateTime.UtcNow, new BulkSmsRequest()) { }
}

public record SmsTemplate(string Id, string Content);

