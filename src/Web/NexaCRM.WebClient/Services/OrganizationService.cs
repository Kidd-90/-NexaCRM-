using NexaCRM.WebClient.Models.Organization;
using NexaCRM.WebClient.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services;

public class OrganizationService : IOrganizationService
{
    public Task<IEnumerable<OrganizationUnit>> GetStructureAsync() =>
        Task.FromResult<IEnumerable<OrganizationUnit>>(new List<OrganizationUnit>());

    public Task SaveOrganizationUnitAsync(OrganizationUnit unit) =>
        Task.CompletedTask;

    public Task DeleteOrganizationUnitAsync(int id) =>
        Task.CompletedTask;

    public Task<IEnumerable<OrganizationStats>> GetOrganizationStatsAsync() =>
        Task.FromResult<IEnumerable<OrganizationStats>>(new List<OrganizationStats>());

    public Task SetSystemAdministratorAsync(string userId) =>
        Task.CompletedTask;
}

