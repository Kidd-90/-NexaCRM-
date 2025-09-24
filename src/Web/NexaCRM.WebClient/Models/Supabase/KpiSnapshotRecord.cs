using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.WebClient.Models.Supabase;

[Table("kpi_snapshots")]
public sealed class KpiSnapshotRecord : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("metric")]
    public string Metric { get; set; } = string.Empty;

    [Column("value")]
    public decimal Value { get; set; }

    [Column("captured_at")]
    public DateTime CapturedAt { get; set; }
}
