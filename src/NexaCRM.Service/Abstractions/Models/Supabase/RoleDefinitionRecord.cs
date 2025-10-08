using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.Services.Admin.Models.Supabase;

/// <summary>
/// Represents role_definitions table record
/// </summary>
[Table("role_definitions")]
public class RoleDefinitionRecord : BaseModel
{
    [PrimaryKey("code", false)]
    [Column("code")]
    public string RoleCode { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
