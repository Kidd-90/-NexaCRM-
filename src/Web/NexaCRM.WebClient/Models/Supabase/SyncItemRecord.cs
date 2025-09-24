using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.WebClient.Models.Supabase;

[Table("sync_items")]
public sealed class SyncItemRecord : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("envelope_id")]
    public Guid EnvelopeId { get; set; }

    [Column("entity_type")]
    public string EntityType { get; set; } = string.Empty;

    [Column("entity_id")]
    public string EntityId { get; set; } = string.Empty;

    [Column("last_modified_at")]
    public DateTime LastModifiedAt { get; set; }

    [Column("payload_json")]
    public string PayloadJson { get; set; } = string.Empty;
}
