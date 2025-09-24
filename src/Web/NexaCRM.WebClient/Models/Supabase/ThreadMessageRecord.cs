using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.WebClient.Models.Supabase;

[Table("thread_messages")]
public sealed class ThreadMessageRecord : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("thread_id")]
    public Guid ThreadId { get; set; }

    [Column("sender_id")]
    public Guid SenderId { get; set; }

    [Column("body")]
    public string Body { get; set; } = string.Empty;

    [Column("sent_at")]
    public DateTime SentAt { get; set; }

    [Column("channels")]
    public string? Channels { get; set; }
}
