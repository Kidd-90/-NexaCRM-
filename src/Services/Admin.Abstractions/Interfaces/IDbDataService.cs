using NexaCRM.Services.Admin.Models.Db;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.Services.Admin.Interfaces
{
    public interface IDbDataService
    {
        // Methods for Manager
        Task<IEnumerable<DbCustomer>> GetAllDbListAsync();
        Task<IEnumerable<DbCustomer>> GetTeamDbStatusAsync();
        Task<IEnumerable<DbCustomer>> GetUnassignedDbListAsync();
        Task<IEnumerable<DbCustomer>> GetTodaysAssignedDbAsync();
        Task<IEnumerable<DbCustomer>> GetDbDistributionStatusAsync();

        Task AssignDbToAgentAsync(int contactId, string agentName);
        Task ReassignDbAsync(int contactId, string agentName);
        Task RecallDbAsync(int contactId);

        // Methods for Sales
        Task<IEnumerable<DbCustomer>> GetNewDbListAsync(string salesAgentName);
        Task<IEnumerable<DbCustomer>> GetStarredDbListAsync(string salesAgentName);
        Task<IEnumerable<DbCustomer>> GetNewlyAssignedDbAsync(string salesAgentName);
        Task<IEnumerable<DbCustomer>> GetMyAssignmentHistoryAsync(string salesAgentName);

        // Common Method
        Task<IEnumerable<DbCustomer>> GetMyDbListAsync(string agentName);

        // Advanced DB management (duplicates)
        Task ArchiveCustomersAsync(IEnumerable<int> contactIds);
        Task DeleteCustomersAsync(IEnumerable<int> contactIds);
        Task MergeCustomersAsync(int primaryContactId, IEnumerable<int> duplicateContactIds);
        Task UpdateCustomerPartialAsync(int contactId, DbCustomer patch, bool overwriteEmptyOnly = false);
    }
}
