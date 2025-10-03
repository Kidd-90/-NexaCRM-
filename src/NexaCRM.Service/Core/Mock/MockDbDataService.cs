using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Models.Db;
using NexaCRM.Services.Admin.Interfaces;

namespace NexaCRM.UI.Services.Mock
{
public class MockDbDataService : IDbDataService
{
    private readonly List<DbCustomer> _dbCustomers;
        private const string ManagerName = "김관리";
        private const string SalesAgent1 = "이영업";
        private const string SalesAgent2 = "박세일";

        public MockDbDataService()
        {
            _dbCustomers = new List<DbCustomer>
            {
                // Unassigned
                new DbCustomer { ContactId = 1, CustomerName = "John Doe", ContactNumber = "010-1111-1111", Status = DbStatus.New, AssignedDate = DateTime.Now.AddDays(-2), LastContactDate = DateTime.Now.AddDays(-2), Group = "Group A" },
                new DbCustomer { ContactId = 2, CustomerName = "Jane Smith", ContactNumber = "010-2222-2222", Status = DbStatus.New, AssignedDate = DateTime.Now.AddDays(-3), LastContactDate = DateTime.Now.AddDays(-3), Group = "Group A" },

                // Assigned to SalesAgent1 ("이영업")
                new DbCustomer { ContactId = 3, CustomerName = "Peter Jones", ContactNumber = "010-3333-3333", Status = DbStatus.New, AssignedTo = SalesAgent1, Assigner = ManagerName, AssignedDate = DateTime.Now, LastContactDate = DateTime.Now, IsStarred = true, Group = "Group B" },
                new DbCustomer { ContactId = 4, CustomerName = "Mary Brown", ContactNumber = "010-4444-4444", Status = DbStatus.InProgress, AssignedTo = SalesAgent1, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-1), LastContactDate = DateTime.Now, Group = "Group B" },
                new DbCustomer { ContactId = 5, CustomerName = "David Wilson", ContactNumber = "010-5555-5555", Status = DbStatus.NoAnswer, AssignedTo = SalesAgent1, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-5), LastContactDate = DateTime.Now.AddDays(-1), Group = "Group C" },

                // Assigned to SalesAgent2 ("박세일")
                new DbCustomer { ContactId = 6, CustomerName = "Susan Taylor", ContactNumber = "010-6666-6666", Status = DbStatus.Completed, AssignedTo = SalesAgent2, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-10), LastContactDate = DateTime.Now.AddDays(-2), Group = "Group C" },
                new DbCustomer { ContactId = 7, CustomerName = "Michael Clark", ContactNumber = "010-7777-7777", Status = DbStatus.InProgress, AssignedTo = SalesAgent2, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-4), LastContactDate = DateTime.Now.AddDays(-1), IsStarred = true, Group = "Group C" },
                new DbCustomer { ContactId = 8, CustomerName = "Linda Harris", ContactNumber = "010-8888-8888", Status = DbStatus.New, AssignedTo = SalesAgent2, Assigner = ManagerName, AssignedDate = DateTime.Now, LastContactDate = DateTime.Now, Group = "Group A" },

                // Assigned to Manager ("김관리")
                new DbCustomer { ContactId = 9, CustomerName = "Robert Lee", ContactNumber = "010-9999-9999", Status = DbStatus.InProgress, AssignedTo = ManagerName, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-20), LastContactDate = DateTime.Now.AddDays(-5), IsStarred = true, Group = "Group B" },
                new DbCustomer { ContactId = 10, CustomerName = "Patricia Walker", ContactNumber = "010-0000-0000", Status = DbStatus.OnHold, AssignedTo = ManagerName, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-30), LastContactDate = DateTime.Now.AddDays(-10), Group = "Group A" }
            };
        }

        private Task<IEnumerable<DbCustomer>> FilterData(Func<DbCustomer, bool> predicate)
        {
            return Task.FromResult(_dbCustomers.Where(c => !c.IsArchived).Where(predicate));
        }

        // Manager Methods
        public Task<IEnumerable<DbCustomer>> GetAllDbListAsync() => Task.FromResult(_dbCustomers.Where(c => !c.IsArchived).AsEnumerable());
        public Task<IEnumerable<DbCustomer>> GetTeamDbStatusAsync() => FilterData(c => !string.IsNullOrEmpty(c.AssignedTo));
        public Task<IEnumerable<DbCustomer>> GetUnassignedDbListAsync() => FilterData(c => string.IsNullOrEmpty(c.AssignedTo));
        public Task<IEnumerable<DbCustomer>> GetTodaysAssignedDbAsync() => FilterData(c => c.AssignedDate.Date == DateTime.Today);
        public Task<IEnumerable<DbCustomer>> GetDbDistributionStatusAsync() => FilterData(c => !string.IsNullOrEmpty(c.AssignedTo)); // Same as Team Status for this mock

        public Task AssignDbToAgentAsync(int contactId, string agentName)
        {
            var customer = _dbCustomers.FirstOrDefault(c => c.ContactId == contactId);
            if (customer != null)
            {
                customer.AssignedTo = agentName;
                customer.AssignedDate = DateTime.Now;
                customer.Assigner = ManagerName;
            }
            return Task.CompletedTask;
        }

        public Task ReassignDbAsync(int contactId, string agentName)
            => AssignDbToAgentAsync(contactId, agentName);

        public Task RecallDbAsync(int contactId)
        {
            var customer = _dbCustomers.FirstOrDefault(c => c.ContactId == contactId);
            if (customer != null)
            {
                customer.AssignedTo = null;
            }
            return Task.CompletedTask;
        }

        // Sales Methods
        public Task<IEnumerable<DbCustomer>> GetNewDbListAsync(string salesAgentName) => FilterData(c => c.AssignedTo == salesAgentName && c.Status == DbStatus.New);
        public Task<IEnumerable<DbCustomer>> GetStarredDbListAsync(string salesAgentName) => FilterData(c => c.AssignedTo == salesAgentName && c.IsStarred);
        public Task<IEnumerable<DbCustomer>> GetNewlyAssignedDbAsync(string salesAgentName) => FilterData(c => c.AssignedTo == salesAgentName && c.AssignedDate.Date == DateTime.Today);
        public Task<IEnumerable<DbCustomer>> GetMyAssignmentHistoryAsync(string salesAgentName) => FilterData(c => c.AssignedTo == salesAgentName);

        // Common Method
    public Task<IEnumerable<DbCustomer>> GetMyDbListAsync(string agentName) => FilterData(c => c.AssignedTo == agentName);

    // Advanced management mock ops
    public Task ArchiveCustomersAsync(IEnumerable<int> contactIds)
    {
        var set = new HashSet<int>(contactIds);
        foreach (var c in _dbCustomers.Where(c => set.Contains(c.ContactId)))
        {
            c.IsArchived = true;
        }
        return Task.CompletedTask;
    }

    public Task DeleteCustomersAsync(IEnumerable<int> contactIds)
    {
        var set = new HashSet<int>(contactIds);
        _dbCustomers.RemoveAll(c => set.Contains(c.ContactId));
        return Task.CompletedTask;
    }

    public Task MergeCustomersAsync(int primaryContactId, IEnumerable<int> duplicateContactIds)
    {
        var dupSet = new HashSet<int>(duplicateContactIds);
        var primary = _dbCustomers.FirstOrDefault(c => c.ContactId == primaryContactId);
        if (primary is null)
        {
            _dbCustomers.RemoveAll(c => dupSet.Contains(c.ContactId));
            return Task.CompletedTask;
        }

        var dups = _dbCustomers.Where(c => dupSet.Contains(c.ContactId)).ToList();
        // Simple merge policy: prefer primary; if empty/null, fill from newest duplicate
        DbCustomer? Latest(Func<DbCustomer, object?> sel)
            => dups.OrderByDescending(x => x.AssignedDate).FirstOrDefault(x => sel(x) != null && !string.IsNullOrWhiteSpace(sel(x)?.ToString()));

        primary.Gender = string.IsNullOrWhiteSpace(primary.Gender) ? Latest(c => c.Gender)?.Gender ?? primary.Gender : primary.Gender;
        primary.Address = string.IsNullOrWhiteSpace(primary.Address) ? Latest(c => c.Address)?.Address ?? primary.Address : primary.Address;
        primary.JobTitle = string.IsNullOrWhiteSpace(primary.JobTitle) ? Latest(c => c.JobTitle)?.JobTitle ?? primary.JobTitle : primary.JobTitle;
        primary.MaritalStatus = string.IsNullOrWhiteSpace(primary.MaritalStatus) ? Latest(c => c.MaritalStatus)?.MaritalStatus ?? primary.MaritalStatus : primary.MaritalStatus;
        primary.ProofNumber = string.IsNullOrWhiteSpace(primary.ProofNumber) ? Latest(c => c.ProofNumber)?.ProofNumber ?? primary.ProofNumber : primary.ProofNumber;
        if (!primary.DbPrice.HasValue)
            primary.DbPrice = dups.OrderByDescending(x => x.AssignedDate).FirstOrDefault(x => x.DbPrice.HasValue)?.DbPrice;
        primary.Headquarters = string.IsNullOrWhiteSpace(primary.Headquarters) ? Latest(c => c.Headquarters)?.Headquarters ?? primary.Headquarters : primary.Headquarters;
        primary.InsuranceName = string.IsNullOrWhiteSpace(primary.InsuranceName) ? Latest(c => c.InsuranceName)?.InsuranceName ?? primary.InsuranceName : primary.InsuranceName;
        if (!primary.CarJoinDate.HasValue)
            primary.CarJoinDate = dups.OrderByDescending(x => x.AssignedDate).FirstOrDefault(x => x.CarJoinDate.HasValue)?.CarJoinDate;
        // Notes: append unique snippets
        if (dups.Any(d => !string.IsNullOrWhiteSpace(d.Notes)))
        {
            var segments = new List<string>();
            if (!string.IsNullOrWhiteSpace(primary.Notes)) segments.Add(primary.Notes!);
            segments.AddRange(dups.Select(d => d.Notes).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!.Trim()));
            primary.Notes = string.Join(" | ", segments.Distinct());
        }

        // Remove duplicates after merge
        _dbCustomers.RemoveAll(c => dupSet.Contains(c.ContactId));
        return Task.CompletedTask;
    }

    public Task UpdateCustomerPartialAsync(int contactId, DbCustomer patch, bool overwriteEmptyOnly = false)
    {
        var target = _dbCustomers.FirstOrDefault(c => c.ContactId == contactId);
        if (target is null) return Task.CompletedTask;

        if (!string.IsNullOrWhiteSpace(patch.Gender) && (!overwriteEmptyOnly || string.IsNullOrWhiteSpace(target.Gender)))
            target.Gender = patch.Gender;
        if (!string.IsNullOrWhiteSpace(patch.Address) && (!overwriteEmptyOnly || string.IsNullOrWhiteSpace(target.Address)))
            target.Address = patch.Address;
        if (!string.IsNullOrWhiteSpace(patch.JobTitle) && (!overwriteEmptyOnly || string.IsNullOrWhiteSpace(target.JobTitle)))
            target.JobTitle = patch.JobTitle;
        if (!string.IsNullOrWhiteSpace(patch.MaritalStatus) && (!overwriteEmptyOnly || string.IsNullOrWhiteSpace(target.MaritalStatus)))
            target.MaritalStatus = patch.MaritalStatus;
        if (!string.IsNullOrWhiteSpace(patch.ProofNumber) && (!overwriteEmptyOnly || string.IsNullOrWhiteSpace(target.ProofNumber)))
            target.ProofNumber = patch.ProofNumber;
        if (patch.DbPrice.HasValue && (!overwriteEmptyOnly || !target.DbPrice.HasValue))
            target.DbPrice = patch.DbPrice;
        if (!string.IsNullOrWhiteSpace(patch.Headquarters) && (!overwriteEmptyOnly || string.IsNullOrWhiteSpace(target.Headquarters)))
            target.Headquarters = patch.Headquarters;
        if (!string.IsNullOrWhiteSpace(patch.InsuranceName) && (!overwriteEmptyOnly || string.IsNullOrWhiteSpace(target.InsuranceName)))
            target.InsuranceName = patch.InsuranceName;
        if (patch.CarJoinDate.HasValue && (!overwriteEmptyOnly || !target.CarJoinDate.HasValue))
            target.CarJoinDate = patch.CarJoinDate;
        if (!string.IsNullOrWhiteSpace(patch.Notes) && (!overwriteEmptyOnly || string.IsNullOrWhiteSpace(target.Notes)))
            target.Notes = patch.Notes;

        return Task.CompletedTask;
    }
    }
}
