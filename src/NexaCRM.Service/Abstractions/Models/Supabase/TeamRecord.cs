using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("teams")]
public sealed class TeamRecord : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("tenant_unit_id")]
    public long TenantUnitId { get; set; }

    [Column("company_id")]
    public long? CompanyId { get; set; }

    [Column("branch_id")]
    public long? BranchId { get; set; }

    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("manager_id")]
    public Guid? ManagerId { get; set; }

    [Column("manager_cuid")]
    public string? ManagerCuid { get; set; }

    [Column("manager_name")]
    public string? ManagerName { get; set; }

    [Column("member_count")]
    public int MemberCount { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("registered_at")]
    public DateTime RegisteredAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
