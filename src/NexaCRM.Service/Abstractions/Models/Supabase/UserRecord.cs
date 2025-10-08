using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.Services.Admin.Models.Supabase;

/// <summary>
/// Represents user_infos table record
/// </summary>
[Table("user_infos")]
public class UserInfoRecord : BaseModel
{
    [PrimaryKey("user_cuid", false)]
    [Column("user_cuid")]
    public string UserCuid { get; set; } = string.Empty;

    [Column("username")]
    public string? Username { get; set; }

    [Column("full_name")]
    public string? FullName { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Column("department")]
    public string? Department { get; set; }

    [Column("phone_number")]
    public string? PhoneNumber { get; set; }

    [Column("job_title")]
    public string? JobTitle { get; set; }

    [Column("role")]
    public string? Role { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("registered_at")]
    public DateTime? RegisteredAt { get; set; }

    [Column("approved_at")]
    public DateTime? ApprovedAt { get; set; }

    [Column("approved_by")]
    public string? ApprovedBy { get; set; }

    [Column("approval_memo")]
    public string? ApprovalMemo { get; set; }

    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Represents app_users table record
/// </summary>
[Table("app_users")]
public class AppUserRecord : BaseModel
{
    [PrimaryKey("cuid", false)]
    [Column("cuid")]
    public string Cuid { get; set; } = string.Empty;

    [Column("auth_user_id")]
    public Guid? AuthUserId { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Represents user_profiles table record
/// </summary>
[Table("user_profiles")]
public class ProfileRecord : BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_cuid")]
    public string UserCuid { get; set; } = string.Empty;

    [Column("username")]
    public string? Username { get; set; }

    [Column("full_name")]
    public string? FullName { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
}

/// <summary>
/// Represents organization_users table record
/// </summary>
[Table("organization_users")]
public class OrganizationUserRecord : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("user_cuid")]
    public string UserCuid { get; set; } = string.Empty;

    [Column("unit_id")]
    public long? UnitId { get; set; }

    [Column("role")]
    public string Role { get; set; } = string.Empty;

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("department")]
    public string? Department { get; set; }

    [Column("phone_number")]
    public string? PhoneNumber { get; set; }

    [Column("registered_at")]
    public DateTime? RegisteredAt { get; set; }

    [Column("approved_at")]
    public DateTime? ApprovedAt { get; set; }

    [Column("approval_memo")]
    public string? ApprovalMemo { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
