using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Organization;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services;

public class OrganizationService : IOrganizationService
{
    // Temporary in-memory user list for demonstration; replace with API call if needed
    private readonly List<OrganizationUser> _users = new List<OrganizationUser>();

    private readonly HttpClient _httpClient;
    public OrganizationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<OrganizationUnit>> GetOrganizationStructureAsync()
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<OrganizationUnit>>("api/organization/structure")
            ?? new List<OrganizationUnit>();
    }

    public async Task SaveOrganizationUnitAsync(OrganizationUnit unit)
    {
        await _httpClient.PostAsJsonAsync("api/organization/structure", unit);
    }

    public async Task<IEnumerable<OrganizationStats>> GetOrganizationStatsAsync()
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<OrganizationStats>>("api/organization/stats")
            ?? new List<OrganizationStats>();
    }

    public async Task SetSystemAdministratorAsync(string userId)
    {
        await _httpClient.PostAsJsonAsync("api/organization/system-admin", new { userId });
    }

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

