using System.Collections.Generic;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Organization;
using AgentModel = NexaCRM.WebClient.Models.Agent;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface IOrganizationService
{
    Task<IEnumerable<OrganizationUnit>> GetOrganizationStructureAsync();
    Task SaveOrganizationUnitAsync(OrganizationUnit unit);
    Task<IEnumerable<OrganizationStats>> GetOrganizationStatsAsync();
    Task SetSystemAdministratorAsync(string userId);
    Task<IEnumerable<AgentModel>> GetAdminsAsync();
    Task AddAdminAsync(string userId);
    Task RemoveAdminAsync(string userId);
}

