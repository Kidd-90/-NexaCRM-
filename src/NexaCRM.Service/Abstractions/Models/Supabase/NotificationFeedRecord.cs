using System;
using System.Text.Json.Serialization;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("notification_feed")]
public sealed class NotificationFeedRecord : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("message")]
    public string? Message { get; set; }

    [Column("type")]
    public string Type { get; set; } = "info";

    [Column("is_read")]
    public bool IsRead { get; set; }

    // metadata 필드 제거 - JSONB 타입으로 인한 역직렬화 오류 방지
    // 필요한 경우 별도 쿼리로 처리

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
