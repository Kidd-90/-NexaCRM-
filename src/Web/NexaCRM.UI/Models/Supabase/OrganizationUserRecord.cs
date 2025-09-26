using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.WebClient.Models.Supabase;

[Table("organization_users")]
public sealed class OrganizationUserRecord : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("unit_id")]
    public long? UnitId { get; set; }

    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
