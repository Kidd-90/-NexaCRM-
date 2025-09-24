using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.WebClient.Models.Supabase;

[Table("user_preferences")]
public sealed class UserPreferenceRecord : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("theme")]
    public string Theme { get; set; } = "system";

    [Column("date_format")]
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    [Column("enable_notifications")]
    public bool EnableNotifications { get; set; }

    [Column("widget_preferences_json")]
    public string? WidgetPreferencesJson { get; set; }
}
