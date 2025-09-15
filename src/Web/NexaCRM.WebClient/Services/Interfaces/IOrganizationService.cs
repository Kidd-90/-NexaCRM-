using System.Collections.Generic;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Organization;
using AgentModel = NexaCRM.WebClient.Models.Agent;
using NewUserModel = NexaCRM.WebClient.Models.NewUser;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface IOrganizationService
{
    Task<IEnumerable<OrganizationUnit>> GetOrganizationStructureAsync();
    Task SaveOrganizationUnitAsync(OrganizationUnit unit);
    Task DeleteOrganizationUnitAsync(int id);
    Task<IEnumerable<OrganizationStats>> GetOrganizationStatsAsync();
    Task<IEnumerable<AgentModel>> GetAdminsAsync();
    Task AddAdminAsync(string userId);
    Task RemoveAdminAsync(string userId);
    Task<IEnumerable<OrganizationUser>> GetUsersAsync();
    Task UpdateUserAsync(OrganizationUser user);
    Task DeleteUserAsync(int userId);
    Task SetSystemAdministratorAsync(string userId);
    Task RegisterUserAsync(NewUserModel user);
}

