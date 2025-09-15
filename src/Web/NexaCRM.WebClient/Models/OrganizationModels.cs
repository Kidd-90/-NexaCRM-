using System.Collections.Generic;

namespace NexaCRM.WebClient.Models.Organization;

public record OrganizationUnit(int Id, string Name, int? ParentId);

public record OrganizationStats(string UnitName, int MemberCount);

public class OrganizationNode
{
    public OrganizationUnit Unit { get; set; } = default!;
    public List<OrganizationNode> Children { get; set; } = new();
    public bool IsExpanded { get; set; }
}

public class OrganizationUser
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public string? Status { get; set; }
}

