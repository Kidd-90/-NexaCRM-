using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Db;

namespace NexaCRM.Service.InMemory;

/// <summary>
/// In-memory implementation of <see cref="IDbDataService"/> used by the Blazor Server host.
/// </summary>
public sealed class InMemoryDbDataService : IDbDataService
{
    private const string ManagerName = "김관리";
    private const string SalesAgent1 = "이영업";
    private const string SalesAgent2 = "박세일";

    private readonly List<DbCustomer> _dbCustomers;

    public InMemoryDbDataService()
    {
        _dbCustomers = new List<DbCustomer>
        {
            new() { ContactId = 1, CustomerName = "John Doe", ContactNumber = "010-1111-1111", Status = DbStatus.New, AssignedDate = DateTime.Now.AddDays(-2), LastContactDate = DateTime.Now.AddDays(-2), Group = "Group A" },
            new() { ContactId = 2, CustomerName = "Jane Smith", ContactNumber = "010-2222-2222", Status = DbStatus.New, AssignedDate = DateTime.Now.AddDays(-3), LastContactDate = DateTime.Now.AddDays(-3), Group = "Group A" },
            new() { ContactId = 3, CustomerName = "Peter Jones", ContactNumber = "010-3333-3333", Status = DbStatus.New, AssignedTo = SalesAgent1, Assigner = ManagerName, AssignedDate = DateTime.Now, LastContactDate = DateTime.Now, IsStarred = true, Group = "Group B" },
            new() { ContactId = 4, CustomerName = "Mary Brown", ContactNumber = "010-4444-4444", Status = DbStatus.InProgress, AssignedTo = SalesAgent1, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-1), LastContactDate = DateTime.Now, Group = "Group B" },
            new() { ContactId = 5, CustomerName = "David Wilson", ContactNumber = "010-5555-5555", Status = DbStatus.NoAnswer, AssignedTo = SalesAgent1, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-5), LastContactDate = DateTime.Now.AddDays(-1), Group = "Group C" },
            new() { ContactId = 6, CustomerName = "Susan Taylor", ContactNumber = "010-6666-6666", Status = DbStatus.Completed, AssignedTo = SalesAgent2, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-10), LastContactDate = DateTime.Now.AddDays(-2), Group = "Group C" },
            new() { ContactId = 7, CustomerName = "Michael Clark", ContactNumber = "010-7777-7777", Status = DbStatus.InProgress, AssignedTo = SalesAgent2, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-4), LastContactDate = DateTime.Now.AddDays(-1), IsStarred = true, Group = "Group C" },
            new() { ContactId = 8, CustomerName = "Linda Harris", ContactNumber = "010-8888-8888", Status = DbStatus.New, AssignedTo = SalesAgent2, Assigner = ManagerName, AssignedDate = DateTime.Now, LastContactDate = DateTime.Now, Group = "Group A" },
            new() { ContactId = 9, CustomerName = "Robert Lee", ContactNumber = "010-9999-9999", Status = DbStatus.InProgress, AssignedTo = ManagerName, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-20), LastContactDate = DateTime.Now.AddDays(-5), IsStarred = true, Group = "Group B" },
            new() { ContactId = 10, CustomerName = "Patricia Walker", ContactNumber = "010-0000-0000", Status = DbStatus.OnHold, AssignedTo = ManagerName, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-30), LastContactDate = DateTime.Now.AddDays(-10), Group = "Group A" }
        };
    }

    public Task<IEnumerable<DbCustomer>> GetAllDbListAsync() =>
        Task.FromResult(_dbCustomers.Where(c => !c.IsArchived).AsEnumerable());

    public Task<IEnumerable<DbCustomer>> GetTeamDbStatusAsync() =>
        FilterAsync(c => !string.IsNullOrWhiteSpace(c.AssignedTo));

    public Task<IEnumerable<DbCustomer>> GetUnassignedDbListAsync() =>
        FilterAsync(c => string.IsNullOrWhiteSpace(c.AssignedTo));

    public Task<IEnumerable<DbCustomer>> GetTodaysAssignedDbAsync() =>
        FilterAsync(c => c.AssignedDate.Date == DateTime.Today);

    public Task<IEnumerable<DbCustomer>> GetDbDistributionStatusAsync() =>
        FilterAsync(c => !string.IsNullOrWhiteSpace(c.AssignedTo));

    public Task AssignDbToAgentAsync(int contactId, string agentName)
    {
        var customer = FindCustomer(contactId);
        if (customer is not null)
        {
            customer.AssignedTo = agentName;
            customer.AssignedDate = DateTime.Now;
            customer.Assigner = ManagerName;
        }

        return Task.CompletedTask;
    }

    public Task ReassignDbAsync(int contactId, string agentName) =>
        AssignDbToAgentAsync(contactId, agentName);

    public Task RecallDbAsync(int contactId)
    {
        var customer = FindCustomer(contactId);
        if (customer is not null)
        {
            customer.AssignedTo = null;
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<DbCustomer>> GetNewDbListAsync(string salesAgentName) =>
        FilterAsync(c => c.AssignedTo == salesAgentName && c.Status == DbStatus.New);

    public Task<IEnumerable<DbCustomer>> GetStarredDbListAsync(string salesAgentName) =>
        FilterAsync(c => c.AssignedTo == salesAgentName && c.IsStarred);

    public Task<IEnumerable<DbCustomer>> GetNewlyAssignedDbAsync(string salesAgentName) =>
        FilterAsync(c => c.AssignedTo == salesAgentName && c.AssignedDate.Date == DateTime.Today);

    public Task<IEnumerable<DbCustomer>> GetMyAssignmentHistoryAsync(string salesAgentName) =>
        FilterAsync(c => c.AssignedTo == salesAgentName);

    public Task<IEnumerable<DbCustomer>> GetMyDbListAsync(string agentName) =>
        FilterAsync(c => c.AssignedTo == agentName);

    public Task ArchiveCustomersAsync(IEnumerable<int> contactIds)
    {
        var set = new HashSet<int>(contactIds);
        foreach (var customer in _dbCustomers.Where(c => set.Contains(c.ContactId)))
        {
            customer.IsArchived = true;
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
        var duplicateSet = new HashSet<int>(duplicateContactIds);
        var primary = FindCustomer(primaryContactId);
        if (primary is null)
        {
            _dbCustomers.RemoveAll(c => duplicateSet.Contains(c.ContactId));
            return Task.CompletedTask;
        }

        var duplicates = _dbCustomers.Where(c => duplicateSet.Contains(c.ContactId)).ToList();
        if (duplicates.Count == 0)
        {
            return Task.CompletedTask;
        }

        void FillIfEmpty(Func<string?> getter, Action<string?> setter, Func<DbCustomer, string?> selector)
        {
            if (!string.IsNullOrWhiteSpace(getter()))
            {
                return;
            }

            var candidate = duplicates
                .Where(c => !string.IsNullOrWhiteSpace(selector(c)))
                .OrderByDescending(c => c.AssignedDate)
                .Select(selector)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(candidate))
            {
                setter(candidate);
            }
        }

        void FillIfNull<T>(Func<T?> getter, Action<T?> setter, Func<DbCustomer, T?> selector) where T : struct
        {
            if (getter().HasValue)
            {
                return;
            }

            var candidate = duplicates
                .Where(c => selector(c).HasValue)
                .OrderByDescending(c => c.AssignedDate)
                .Select(selector)
                .FirstOrDefault();

            if (candidate.HasValue)
            {
                setter(candidate);
            }
        }

        FillIfEmpty(() => primary.Gender, value => primary.Gender = value, c => c.Gender);
        FillIfEmpty(() => primary.Address, value => primary.Address = value, c => c.Address);
        FillIfEmpty(() => primary.JobTitle, value => primary.JobTitle = value, c => c.JobTitle);
        FillIfEmpty(() => primary.MaritalStatus, value => primary.MaritalStatus = value, c => c.MaritalStatus);
        FillIfEmpty(() => primary.ProofNumber, value => primary.ProofNumber = value, c => c.ProofNumber);
        FillIfEmpty(() => primary.Headquarters, value => primary.Headquarters = value, c => c.Headquarters);
        FillIfEmpty(() => primary.InsuranceName, value => primary.InsuranceName = value, c => c.InsuranceName);
        FillIfEmpty(() => primary.Notes, value => primary.Notes = value, c => c.Notes);
        FillIfNull(() => primary.DbPrice, value => primary.DbPrice = value, c => c.DbPrice);
        FillIfNull(() => primary.CarJoinDate, value => primary.CarJoinDate = value, c => c.CarJoinDate);

        _dbCustomers.RemoveAll(c => duplicateSet.Contains(c.ContactId) && c.ContactId != primaryContactId);
        return Task.CompletedTask;
    }

    public Task UpdateCustomerPartialAsync(int contactId, DbCustomer patch, bool overwriteEmptyOnly = false)
    {
        var customer = FindCustomer(contactId);
        if (customer is null)
        {
            return Task.CompletedTask;
        }

        if (!overwriteEmptyOnly || string.IsNullOrWhiteSpace(customer.CustomerName))
        {
            customer.CustomerName = patch.CustomerName;
        }

        if (!overwriteEmptyOnly || string.IsNullOrWhiteSpace(customer.ContactNumber))
        {
            customer.ContactNumber = patch.ContactNumber;
        }

        if (!overwriteEmptyOnly || customer.AssignedTo is null)
        {
            customer.AssignedTo = patch.AssignedTo;
        }

        customer.Status = patch.Status;
        customer.LastContactDate = patch.LastContactDate == default ? customer.LastContactDate : patch.LastContactDate;
        customer.AssignedDate = patch.AssignedDate == default ? customer.AssignedDate : patch.AssignedDate;
        customer.Group = string.IsNullOrWhiteSpace(patch.Group) ? customer.Group : patch.Group;
        customer.Notes = overwriteEmptyOnly && !string.IsNullOrWhiteSpace(customer.Notes) ? customer.Notes : patch.Notes;

        return Task.CompletedTask;
    }

    private Task<IEnumerable<DbCustomer>> FilterAsync(Func<DbCustomer, bool> predicate)
    {
        var results = _dbCustomers
            .Where(c => !c.IsArchived)
            .Where(predicate)
            .ToList()
            .AsEnumerable();

        return Task.FromResult(results);
    }

    private DbCustomer? FindCustomer(int contactId) =>
        _dbCustomers.FirstOrDefault(c => c.ContactId == contactId);
}
