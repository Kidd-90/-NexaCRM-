using System.Collections.Generic;
using System.Threading.Tasks;
using NexaCRM.UI.Models.Supabase;

namespace NexaCRM.Services.Admin.Interfaces;

public interface IBizManagementService
{
    // Companies
    Task<List<BizCompanyRecord>> GetCompaniesAsync(long tenantUnitId);
    Task<BizCompanyRecord?> GetCompanyByIdAsync(long id);
    Task<BizCompanyRecord> CreateCompanyAsync(BizCompanyRecord company);
    Task<bool> UpdateCompanyAsync(BizCompanyRecord company);
    Task<bool> DeleteCompanyAsync(long id);
    
    // Branches
    Task<List<BizBranchRecord>> GetBranchesAsync(long tenantUnitId);
    Task<List<BizBranchRecord>> GetBranchesByCompanyAsync(long companyId);
    Task<BizBranchRecord?> GetBranchByIdAsync(long id);
    Task<BizBranchRecord> CreateBranchAsync(BizBranchRecord branch);
    Task<bool> UpdateBranchAsync(BizBranchRecord branch);
    Task<bool> DeleteBranchAsync(long id);
}
