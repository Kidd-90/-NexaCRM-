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
}
