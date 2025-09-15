namespace NexaCRM.WebClient.Models.Organization;

using System.ComponentModel.DataAnnotations;

public class OrganizationUnit
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public int? ParentId { get; set; }
}

public record OrganizationStats(string UnitName, int MemberCount);

public class OrganizationUser
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string Status { get; set; } = "";
}

