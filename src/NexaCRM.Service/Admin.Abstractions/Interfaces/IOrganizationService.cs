using System.Collections.Generic;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Models.Organization;
using AgentModel = NexaCRM.Services.Admin.Models.Agent;
using NewUserModel = NexaCRM.Services.Admin.Models.NewUser;

namespace NexaCRM.Services.Admin.Interfaces;

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
    Task ApproveUserAsync(int userId);
    Task RejectUserAsync(int userId, string? reason);
    Task SetSystemAdminAsync(string userId);
    Task RegisterUserAsync(NewUserModel user);
}

