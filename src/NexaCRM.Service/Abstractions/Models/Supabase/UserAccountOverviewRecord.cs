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
    public Guid? AuthUserId { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("username")]
    public string? Username { get; set; }

    [Column("full_name")]
    public string? FullName { get; set; }

        [Column("job_title")]
    public string? JobTitle { get; set; }

    [Column("phone_number")]
    public string? PhoneNumber { get; set; }

    [Column("role_codes")]
    public string[]? RoleCodes { get; set; }

    [Column("password_hash")]
    public string? PasswordHash { get; set; }

    [Column("department")]
    public string? Department { get; set; }

    [Column("account_created_at")]
    public DateTime? AccountCreatedAt { get; set; }

    [Column("account_updated_at")]
    public DateTime? AccountUpdatedAt { get; set; }

    [Column("profile_created_at")]
    public DateTime? ProfileCreatedAt { get; set; }

    [Column("profile_updated_at")]
    public DateTime? ProfileUpdatedAt { get; set; }
}
