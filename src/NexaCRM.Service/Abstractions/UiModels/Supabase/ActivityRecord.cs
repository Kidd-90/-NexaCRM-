using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("activities")]
public sealed class ActivityRecord : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("type")]
    public string? Type { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("activity_date")]
    public DateTime ActivityDate { get; set; }

    [Column("contact_id")]
    public int? ContactId { get; set; }

    [Column("deal_id")]
    public int? DealId { get; set; }

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("created_by_name")]
    public string? CreatedByName { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
