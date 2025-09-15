using NexaCRM.WebClient.Models.Organization;
using NexaCRM.WebClient.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services;

public class OrganizationService : IOrganizationService
{
    private readonly List<OrganizationUser> _users = new()
    {
        new OrganizationUser { Id = 1, Name = "Ethan Carter", Email = "ethan.carter@example.com", Role = "Admin", Status = "Active" },
        new OrganizationUser { Id = 2, Name = "Olivia Bennett", Email = "olivia.bennett@example.com", Role = "Sales", Status = "Active" },
        new OrganizationUser { Id = 3, Name = "Noah Thompson", Email = "noah.thompson@example.com", Role = "Marketing", Status = "Inactive" }
    };

    public Task<IEnumerable<OrganizationUnit>> GetOrganizationStructureAsync() =>
        Task.FromResult<IEnumerable<OrganizationUnit>>(new List<OrganizationUnit>());

    public Task SaveOrganizationUnitAsync(OrganizationUnit unit) =>
        Task.CompletedTask;

    public Task<IEnumerable<OrganizationStats>> GetOrganizationStatsAsync() =>
        Task.FromResult<IEnumerable<OrganizationStats>>(new List<OrganizationStats>());

    public Task SetSystemAdministratorAsync(string userId) =>
        Task.CompletedTask;

    public Task<IEnumerable<OrganizationUser>> GetUsersAsync() =>
        Task.FromResult<IEnumerable<OrganizationUser>>(_users);

    public Task UpdateUserAsync(OrganizationUser user)
    {
        var index = _users.FindIndex(u => u.Id == user.Id);
        if (index >= 0)
        {
            _users[index] = user;
        }
        return Task.CompletedTask;
    }

    public Task DeleteUserAsync(int userId)
    {
        _users.RemoveAll(u => u.Id == userId);
        return Task.CompletedTask;
    }
}

