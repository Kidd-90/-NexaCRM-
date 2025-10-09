using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("biz_branches")]
public sealed class BizBranchRecord : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("company_id")]
    public long CompanyId { get; set; }

    [Column("tenant_unit_id")]
    public long TenantUnitId { get; set; }

    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("manager_id")]
    public Guid? ManagerId { get; set; }

    [Column("manager_cuid")]
    public string? ManagerCuid { get; set; }

    [Column("memo")]
    public string? Memo { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
