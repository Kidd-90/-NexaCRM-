using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.WebClient.Models.Supabase;

[Table("audit_logs")]
public sealed class AuditLogRecord : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("actor_id")]
    public Guid? ActorId { get; set; }

    [Column("action")]
    public string Action { get; set; } = string.Empty;

    [Column("entity_type")]
    public string EntityType { get; set; } = string.Empty;

    [Column("entity_id")]
    public string? EntityId { get; set; }

    [Column("payload_json")]
    public string? PayloadJson { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
}
