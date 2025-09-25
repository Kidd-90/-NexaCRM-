using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.WebClient.Models.Supabase;

[Table("agents")]
public sealed class AgentRecord : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [Column("email")]
    public string? Email { get; set; }

    [Column("role")]
    public string Role { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
