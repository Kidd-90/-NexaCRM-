namespace NexaCRM.Services.Admin.Models.Settings;

public class NotificationSettings
{
    public bool NewLeadCreated { get; set; }
    public bool LeadStatusUpdated { get; set; }
    public bool DealStageChanged { get; set; }
    public bool DealValueUpdated { get; set; }
    public bool NewTaskAssigned { get; set; }
    public bool TaskDueDateReminder { get; set; }
    public bool EmailNotifications { get; set; }
    public bool InAppNotifications { get; set; }
    public bool PushNotifications { get; set; }
}
