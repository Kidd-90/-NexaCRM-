using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Organization;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services;

public class OrganizationService : IOrganizationService
{
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
}

