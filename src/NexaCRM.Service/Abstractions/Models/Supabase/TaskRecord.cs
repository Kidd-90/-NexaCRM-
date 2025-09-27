using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("tasks")]
public sealed class TaskRecord : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("due_date")]
    public DateTime? DueDate { get; set; }

    [Column("is_completed")]
    public bool IsCompleted { get; set; }

    [Column("priority")]
    public string Priority { get; set; } = "Medium";

    [Column("assigned_to")]
    public Guid? AssignedTo { get; set; }

    [Column("assigned_to_name")]
    public string? AssignedToName { get; set; }

    [Column("contact_id")]
    public int? ContactId { get; set; }

    [Column("deal_id")]
    public int? DealId { get; set; }

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    [Column("tenant_unit_id")]
    public int? TenantUnitId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
