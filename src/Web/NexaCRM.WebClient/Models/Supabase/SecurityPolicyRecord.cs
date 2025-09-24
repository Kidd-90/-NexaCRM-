using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.WebClient.Models.Supabase;

[Table("security_policies")]
public sealed class SecurityPolicyRecord : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("organization_id")]
    public Guid OrganizationId { get; set; }

    [Column("require_mfa")]
    public bool RequireMfa { get; set; }

    [Column("session_timeout_minutes")]
    public int SessionTimeoutMinutes { get; set; }

    [Column("ip_allow_list")]
    public string? IpAllowList { get; set; }

    [Column("password_expiry_days")]
    public int PasswordExpiryDays { get; set; }
}
