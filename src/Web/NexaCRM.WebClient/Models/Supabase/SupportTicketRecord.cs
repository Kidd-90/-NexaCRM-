using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.WebClient.Models.Supabase;

[Table("support_tickets")]
public sealed class SupportTicketRecord : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("subject")]
    public string? Subject { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Open";

    [Column("priority")]
    public string Priority { get; set; } = "Medium";

    [Column("customer_id")]
    public int? CustomerId { get; set; }

    [Column("customer_name")]
    public string? CustomerName { get; set; }

    [Column("agent_id")]
    public Guid? AgentId { get; set; }

    [Column("agent_name")]
    public string? AgentName { get; set; }

    [Column("category")]
    public string? Category { get; set; }

    [Column("tenant_unit_id")]
    public long? TenantUnitId { get; set; }

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("last_reply_at")]
    public DateTime? LastReplyAt { get; set; }
}
