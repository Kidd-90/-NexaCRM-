using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace NexaCRM.UI.Models.Supabase;

[Table("user_directory_entries")]
public sealed class UserDirectoryEntryRecord : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("user_cuid")]
    public string UserCuid { get; set; } = string.Empty;

    [Column("company_id")]
    public long? CompanyId { get; set; }

    [Column("branch_id")]
    public long? BranchId { get; set; }

    [Column("team_id")]
    public long? TeamId { get; set; }

    [Column("tenant_unit_id")]
    public long? TenantUnitId { get; set; }

    [Column("job_title")]
    public string? JobTitle { get; set; }

    [Column("employee_number")]
    public string? EmployeeNumber { get; set; }

    [Column("employment_type")]
    public string? EmploymentType { get; set; }

    [Column("status")]
    public string Status { get; set; } = "active";

    [Column("hired_on")]
    public DateTime? HiredOn { get; set; }

    [Column("ended_on")]
    public DateTime? EndedOn { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
