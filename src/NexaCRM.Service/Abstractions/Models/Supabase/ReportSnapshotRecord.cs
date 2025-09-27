using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("report_snapshots")]
public sealed class ReportSnapshotRecord : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("definition_id")]
    public long? DefinitionId { get; set; }

    [Column("generated_at")]
    public DateTime GeneratedAt { get; set; }

    [Column("payload_json")]
    public string PayloadJson { get; set; } = "{}";

    [Column("format")]
    public string Format { get; set; } = "json";

    [Column("metrics_summary")]
    public string? MetricsSummaryJson { get; set; }

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }
}
