using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("consultation_notes")]
public sealed class ConsultationNoteRecord : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("contact_id")]
    public int ContactId { get; set; }

    [Column("contact_name")]
    public string? ContactName { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("tags")]
    public string? Tags { get; set; }

    [Column("priority")]
    public string Priority { get; set; } = string.Empty;

    [Column("is_follow_up_required")]
    public bool IsFollowUpRequired { get; set; }

    [Column("follow_up_date")]
    public DateTime? FollowUpDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
