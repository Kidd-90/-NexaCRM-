using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("notification_settings")]
public sealed class NotificationSettingsRecord : BaseModel
{
    [PrimaryKey("user_id")]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("new_lead_created")]
    public bool NewLeadCreated { get; set; } = true;

    [Column("lead_status_updated")]
    public bool LeadStatusUpdated { get; set; } = true;

    [Column("deal_stage_changed")]
    public bool DealStageChanged { get; set; } = true;

    [Column("deal_value_updated")]
    public bool DealValueUpdated { get; set; } = true;

    [Column("new_task_assigned")]
    public bool NewTaskAssigned { get; set; } = true;

    [Column("task_due_date_reminder")]
    public bool TaskDueDateReminder { get; set; } = true;

    [Column("email_notifications")]
    public bool EmailNotifications { get; set; } = true;

    [Column("in_app_notifications")]
    public bool InAppNotifications { get; set; } = true;

    [Column("push_notifications")]
    public bool PushNotifications { get; set; } = false;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
