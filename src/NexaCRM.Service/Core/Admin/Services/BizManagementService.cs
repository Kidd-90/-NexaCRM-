using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.UI.Models.Supabase;
using Supabase;

namespace NexaCRM.Services.Admin;

public sealed class BizManagementService : IBizManagementService
{
    private readonly Client _supabaseClient;
    private readonly ILogger<BizManagementService> _logger;

    public BizManagementService(Client supabaseClient, ILogger<BizManagementService> logger)
    {
        _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Organization Units
    public async Task<List<OrganizationUnitRecord>> GetOrganizationUnitsAsync()
    {
        try
        {
            var response = await _supabaseClient
                .From<OrganizationUnitRecord>()
                .Get();

            return response.Models ?? new List<OrganizationUnitRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch organization units");
            throw;
        }
    }

    public async Task<OrganizationUnitRecord?> GetOrganizationUnitByIdAsync(long id)
    {
        try
        {
            var response = await _supabaseClient
                .From<OrganizationUnitRecord>()
                .Where(u => u.Id == id)
                .Single();

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch organization unit {UnitId}", id);
            return null;
        }
    }

    public async Task<long> GetOrCreateDefaultOrganizationUnitAsync()
    {
        try
        {
            // Try to get the default organization unit (id=1)
            var existingUnit = await GetOrganizationUnitByIdAsync(1);
            if (existingUnit != null)
            {
                return existingUnit.Id;
            }

            // If it doesn't exist, create it
            _logger.LogWarning("Default organization unit (id=1) not found. Creating it.");
            
            var defaultUnit = new OrganizationUnitRecord
            {
                Name = "기본 조직",
                TenantCode = "DEFAULT"
            };

            var response = await _supabaseClient
                .From<OrganizationUnitRecord>()
                .Insert(defaultUnit);

            var created = response.Models.FirstOrDefault();
            if (created != null)
            {
                _logger.LogInformation("Created default organization unit with ID {UnitId}", created.Id);
                return created.Id;
            }

            throw new InvalidOperationException("Failed to create default organization unit");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get or create default organization unit");
            throw;
        }
    }

    // Users
    public async Task<List<UserDirectoryEntryRecord>> GetUsersAsync(long tenantUnitId)
    {
        try
        {
            var response = await _supabaseClient
                .From<UserDirectoryEntryRecord>()
                .Where(u => u.TenantUnitId == tenantUnitId && u.Status == "active")
                .Order(u => u.JobTitle, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            return response.Models ?? new List<UserDirectoryEntryRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch users for tenant {TenantUnitId}", tenantUnitId);
            throw;
        }
    }

    // Companies
    public async Task<List<BizCompanyRecord>> GetCompaniesAsync(long tenantUnitId)
    {
        try
        {
            var response = await _supabaseClient
                .From<BizCompanyRecord>()
                .Where(c => c.TenantUnitId == tenantUnitId)
                .Order(c => c.Name, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            return response.Models ?? new List<BizCompanyRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch companies for tenant {TenantUnitId}", tenantUnitId);
            throw;
        }
    }

    public async Task<BizCompanyRecord?> GetCompanyByIdAsync(long id)
    {
        try
        {
            var response = await _supabaseClient
                .From<BizCompanyRecord>()
                .Where(c => c.Id == id)
                .Single();

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch company {CompanyId}", id);
            return null;
        }
    }

    public async Task<BizCompanyRecord> CreateCompanyAsync(BizCompanyRecord company)
    {
        try
        {
            var response = await _supabaseClient
                .From<BizCompanyRecord>()
                .Insert(company);

            var created = response.Models.FirstOrDefault();
            if (created == null)
            {
                throw new InvalidOperationException("Failed to create company");
            }

            _logger.LogInformation("Created company {CompanyName} with ID {CompanyId}", company.Name, created.Id);
            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create company {CompanyName}", company.Name);
            throw;
        }
    }

    public async Task<bool> UpdateCompanyAsync(BizCompanyRecord company)
    {
        try
        {
            await _supabaseClient
                .From<BizCompanyRecord>()
                .Where(c => c.Id == company.Id)
                .Update(company);

            _logger.LogInformation("Updated company {CompanyId}", company.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update company {CompanyId}", company.Id);
            return false;
        }
    }

    public async Task<bool> DeleteCompanyAsync(long id)
    {
        try
        {
            await _supabaseClient
                .From<BizCompanyRecord>()
                .Where(c => c.Id == id)
                .Delete();

            _logger.LogInformation("Deleted company {CompanyId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete company {CompanyId}", id);
            return false;
        }
    }

    // Branches
    public async Task<List<BizBranchRecord>> GetBranchesAsync(long tenantUnitId)
    {
        try
        {
            var response = await _supabaseClient
                .From<BizBranchRecord>()
                .Where(b => b.TenantUnitId == tenantUnitId)
                .Order(b => b.Name, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            return response.Models ?? new List<BizBranchRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch branches for tenant {TenantUnitId}", tenantUnitId);
            throw;
        }
    }

    public async Task<List<BizBranchRecord>> GetBranchesByCompanyAsync(long companyId)
    {
        try
        {
            var response = await _supabaseClient
                .From<BizBranchRecord>()
                .Where(b => b.CompanyId == companyId)
                .Order(b => b.Name, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            return response.Models ?? new List<BizBranchRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch branches for company {CompanyId}", companyId);
            throw;
        }
    }

    public async Task<BizBranchRecord?> GetBranchByIdAsync(long id)
    {
        try
        {
            var response = await _supabaseClient
                .From<BizBranchRecord>()
                .Where(b => b.Id == id)
                .Single();

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch branch {BranchId}", id);
            return null;
        }
    }

    public async Task<BizBranchRecord> CreateBranchAsync(BizBranchRecord branch)
    {
        try
        {
            var response = await _supabaseClient
                .From<BizBranchRecord>()
                .Insert(branch);

            var created = response.Models.FirstOrDefault();
            if (created == null)
            {
                throw new InvalidOperationException("Failed to create branch");
            }

            _logger.LogInformation("Created branch {BranchName} with ID {BranchId}", branch.Name, created.Id);
            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create branch {BranchName}", branch.Name);
            throw;
        }
    }

    public async Task<bool> UpdateBranchAsync(BizBranchRecord branch)
    {
        try
        {
            await _supabaseClient
                .From<BizBranchRecord>()
                .Where(b => b.Id == branch.Id)
                .Update(branch);

            _logger.LogInformation("Updated branch {BranchId}", branch.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update branch {BranchId}", branch.Id);
            return false;
        }
    }

    public async Task<bool> DeleteBranchAsync(long id)
    {
        try
        {
            await _supabaseClient
                .From<BizBranchRecord>()
                .Where(b => b.Id == id)
                .Delete();

            _logger.LogInformation("Deleted branch {BranchId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete branch {BranchId}", id);
            return false;
        }
    }

    // Teams
    public async Task<List<TeamRecord>> GetTeamsAsync(long tenantUnitId)
    {
        try
        {
            var response = await _supabaseClient
                .From<TeamRecord>()
                .Where(t => t.TenantUnitId == tenantUnitId)
                .Order(t => t.Name, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            return response.Models ?? new List<TeamRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch teams for tenant {TenantUnitId}", tenantUnitId);
            throw;
        }
    }

    public async Task<List<TeamRecord>> GetTeamsByBranchAsync(long branchId)
    {
        try
        {
            var response = await _supabaseClient
                .From<TeamRecord>()
                .Where(t => t.BranchId == branchId)
                .Order(t => t.Name, Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            return response.Models ?? new List<TeamRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch teams for branch {BranchId}", branchId);
            throw;
        }
    }

    public async Task<TeamRecord?> GetTeamByIdAsync(long id)
    {
        try
        {
            var response = await _supabaseClient
                .From<TeamRecord>()
                .Where(t => t.Id == id)
                .Single();

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch team {TeamId}", id);
            return null;
        }
    }

    public async Task<TeamRecord> CreateTeamAsync(TeamRecord team)
    {
        try
        {
            var response = await _supabaseClient
                .From<TeamRecord>()
                .Insert(team);

            var created = response.Models.FirstOrDefault();
            if (created == null)
            {
                throw new InvalidOperationException("Failed to create team");
            }

            _logger.LogInformation("Created team {TeamName} with ID {TeamId}", team.Name, created.Id);
            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create team {TeamName}", team.Name);
            throw;
        }
    }

    public async Task<bool> UpdateTeamAsync(TeamRecord team)
    {
        try
        {
            await _supabaseClient
                .From<TeamRecord>()
                .Where(t => t.Id == team.Id)
                .Update(team);

            _logger.LogInformation("Updated team {TeamId}", team.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update team {TeamId}", team.Id);
            return false;
        }
    }

    public async Task<bool> DeleteTeamAsync(long id)
    {
        try
        {
            await _supabaseClient
                .From<TeamRecord>()
                .Where(t => t.Id == id)
                .Delete();

            _logger.LogInformation("Deleted team {TeamId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete team {TeamId}", id);
            return false;
        }
    }
}
