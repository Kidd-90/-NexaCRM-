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

