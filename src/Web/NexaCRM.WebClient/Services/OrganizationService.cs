using System.Collections.Generic;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Organization;
using NexaCRM.WebClient.Services.Interfaces;
using AgentModel = NexaCRM.WebClient.Models.Agent;

namespace NexaCRM.WebClient.Services;

public class OrganizationService : IOrganizationService
{
    private readonly List<AgentModel> _admins = new()
    {
        new AgentModel { Id = 1, Name = "Alice", Email = "alice@example.com", Role = "Admin" },
        new AgentModel { Id = 2, Name = "Bob", Email = "bob@example.com", Role = "Admin" }
    };

    public Task<IEnumerable<OrganizationUnit>> GetOrganizationStructureAsync() =>
        Task.FromResult<IEnumerable<OrganizationUnit>>(new List<OrganizationUnit>());

    public Task SaveOrganizationUnitAsync(OrganizationUnit unit) =>
        Task.CompletedTask;

    public Task<IEnumerable<OrganizationStats>> GetOrganizationStatsAsync() =>
        Task.FromResult<IEnumerable<OrganizationStats>>(new List<OrganizationStats>());

    public Task SetSystemAdministratorAsync(string userId) =>
        Task.CompletedTask;

    public Task<IEnumerable<AgentModel>> GetAdminsAsync() =>
        Task.FromResult<IEnumerable<AgentModel>>(_admins);

    public Task AddAdminAsync(string userId)
    {
        if (int.TryParse(userId, out var id))
        {
            _admins.Add(new AgentModel
            {
                Id = id,
                Name = $"User{id}",
                Email = $"user{id}@example.com",
                Role = "Admin"
            });
        }
        return Task.CompletedTask;
    }

    public Task RemoveAdminAsync(string userId)
    {
        if (int.TryParse(userId, out var id))
        {
            _admins.RemoveAll(a => a.Id == id);
        }
        return Task.CompletedTask;
    }
}

