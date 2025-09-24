using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.WebClient.Models.Supabase;

[Table("sync_conflicts")]
public sealed class SyncConflictRecord : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("entity_type")]
    public string EntityType { get; set; } = string.Empty;

    [Column("entity_id")]
    public string EntityId { get; set; } = string.Empty;

    [Column("resolution_strategy")]
    public string ResolutionStrategy { get; set; } = "ServerWins";

    [Column("payload_json")]
    public string PayloadJson { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
