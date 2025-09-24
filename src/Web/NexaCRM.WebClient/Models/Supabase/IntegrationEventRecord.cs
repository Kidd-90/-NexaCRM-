using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.WebClient.Models.Supabase;

[Table("integration_events")]
public sealed class IntegrationEventRecord : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    [Column("payload_json")]
    public string PayloadJson { get; set; } = string.Empty;

    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("published_at")]
    public DateTime? PublishedAt { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
}
