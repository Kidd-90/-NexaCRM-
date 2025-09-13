namespace NexaCRM.WebClient.Models.Organization;

public record OrganizationUnit(int Id, string Name, int? ParentId);

public record OrganizationStats(string UnitName, int MemberCount);

