using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.Service.Abstractions.Models.Supabase;

[Table("user_account_overview")]
public sealed class UserAccountOverviewRecord : BaseModel
{
    [PrimaryKey("cuid", false)]
    public string Cuid { get; set; } = string.Empty;

    [Column("auth_user_id")]
    public Guid AuthUserId { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("status")]
    public string Status { get; set; } = "active";

    [Column("username")]
    public string? Username { get; set; }

    [Column("full_name")]
    public string? FullName { get; set; }

        [Column("job_title")]
    public string? JobTitle { get; set; }

    [Column("role_codes")]
    public string[] RoleCodes { get; set; } = Array.Empty<string>();

    [Column("password_hash")]
    public string? PasswordHash { get; set; }

    [Column("department")]
    public string? Department { get; set; }
}
