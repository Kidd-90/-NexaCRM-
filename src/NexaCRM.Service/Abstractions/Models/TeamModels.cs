using System;

namespace NexaCRM.UI.Models.Teams;

public class TeamInfo
{
    public long Id { get; set; }
    public string TeamCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime RegisteredAt { get; set; }
}

public class TeamMemberInfo
{
    public long Id { get; set; }
    public long TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool AllowExcelUpload { get; set; }
    public bool IsActive { get; set; }
    public DateTime RegisteredAt { get; set; }
}
