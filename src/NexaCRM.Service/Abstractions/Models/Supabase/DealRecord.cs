using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("deals")]
public sealed class DealRecord : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("value")]
    public decimal? Value { get; set; }

    [Column("company_id")]
    public int? CompanyId { get; set; }

    [Column("company_name")]
    public string? CompanyName { get; set; }

    [Column("contact_id")]
    public int? ContactId { get; set; }

    [Column("contact_name")]
    public string? ContactName { get; set; }

    [Column("stage_id")]
    public int StageId { get; set; }

    [Column("assigned_to")]
    public Guid? AssignedTo { get; set; }

    [Column("assigned_to_name")]
    public string? AssignedToName { get; set; }

    [Column("expected_close_date")]
    public DateTime? ExpectedCloseDate { get; set; }

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
