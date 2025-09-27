using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("team_members")]
public sealed class TeamMemberRecord : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("team_id")]
    public int TeamId { get; set; }

    [Column("user_cuid")]
    public string UserCuid { get; set; } = string.Empty;

    [Column("team_name")]
    public string? TeamName { get; set; }

    [Column("role")]
    public string Role { get; set; } = string.Empty;

    [Column("employee_code")]
    public string EmployeeCode { get; set; } = string.Empty;

    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Column("allow_excel_upload")]
    public bool AllowExcelUpload { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("registered_at")]
    public DateTime RegisteredAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
