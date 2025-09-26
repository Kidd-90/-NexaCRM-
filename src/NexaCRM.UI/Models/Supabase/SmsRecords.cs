using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("sms_settings")]
public sealed class SmsSettingsRecord : BaseModel
{
    [PrimaryKey("user_id")]
    public Guid UserId { get; set; }

    [Column("provider_api_key")]
    public string? ProviderApiKey { get; set; }

    [Column("provider_api_secret")]
    public string? ProviderApiSecret { get; set; }

    [Column("sender_id")]
    public string? SenderId { get; set; }

    [Column("default_template")]
    public string? DefaultTemplate { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

[Table("sms_sender_numbers")]
public sealed class SmsSenderRecord : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("number")]
    public string Number { get; set; } = string.Empty;

    [Column("label")]
    public string? Label { get; set; }

    [Column("is_default")]
    public bool IsDefault { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
}

[Table("sms_templates")]
public sealed class SmsTemplateRecord : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("template_code")]
    public string? TemplateCode { get; set; }

    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("is_default")]
    public bool IsDefault { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
}

[Table("sms_history")]
public sealed class SmsHistoryRecord : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("recipient")]
    public string Recipient { get; set; } = string.Empty;

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("sent_at")]
    public DateTime? SentAt { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Sent";

    [Column("sender_number")]
    public string? SenderNumber { get; set; }

    [Column("recipient_name")]
    public string? RecipientName { get; set; }

    [Column("attachments")]
    public string? AttachmentsJson { get; set; }

    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    [Column("metadata")]
    public string? MetadataJson { get; set; }
}

[Table("sms_schedules")]
public sealed class SmsScheduleRecord : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("scheduled_at")]
    public DateTime ScheduledAt { get; set; }

    [Column("payload_json")]
    public string PayloadJson { get; set; } = string.Empty;

    [Column("is_cancelled")]
    public bool IsCancelled { get; set; }

    [Column("status")]
    public string Status { get; set; } = "scheduled";

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("dispatched_at")]
    public DateTime? DispatchedAt { get; set; }
}
