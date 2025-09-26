using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("user_roles")]
public sealed class UserRoleRecord : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("role_code")]
    public string RoleCode { get; set; } = string.Empty;

    [Column("assigned_by")]
    public Guid? AssignedBy { get; set; }

    [Column("assigned_at")]
    public DateTime AssignedAt { get; set; }
}
