using NexaCRM.WebClient.Models.Organization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface IOrganizationService
{
    Task<IEnumerable<OrganizationUnit>> GetOrganizationStructureAsync();
    Task SaveOrganizationUnitAsync(OrganizationUnit unit);
    Task<IEnumerable<OrganizationStats>> GetOrganizationStatsAsync();
    Task SetSystemAdministratorAsync(string userId);
    Task<IEnumerable<OrganizationUser>> GetUsersAsync();
    Task UpdateUserAsync(OrganizationUser user);
    Task DeleteUserAsync(int userId);
}

