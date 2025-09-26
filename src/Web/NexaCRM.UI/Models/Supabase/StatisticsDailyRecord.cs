using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.WebClient.Models.Supabase;

[Table("statistics_daily")]
public sealed class StatisticsDailyRecord : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("metric_date")]
    public DateTime MetricDate { get; set; }

    [Column("tenant_unit_id")]
    public long? TenantUnitId { get; set; }

    [Column("total_members")]
    public int TotalMembers { get; set; }

    [Column("total_logins")]
    public int TotalLogins { get; set; }

    [Column("total_downloads")]
    public int TotalDownloads { get; set; }

    [Column("active_users")]
    public int ActiveUsers { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
