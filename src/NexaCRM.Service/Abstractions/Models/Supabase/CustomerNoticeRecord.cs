using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("customer_notices")]
public sealed class CustomerNoticeRecord : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("tenant_id")]
    public Guid? TenantId { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("summary")]
    public string? Summary { get; set; }

    [Column("content")]
    public string? Content { get; set; }

    [Column("category")]
    public string? Category { get; set; }

    [Column("importance")]
    public string? Importance { get; set; }

    [Column("published_at")]
    public DateTime PublishedAt { get; set; }

    [Column("is_pinned")]
    public bool IsPinned { get; set; }

    [Column("reference_url")]
    public string? ReferenceUrl { get; set; }

    [Column("status")]
    public string? Status { get; set; }
}
