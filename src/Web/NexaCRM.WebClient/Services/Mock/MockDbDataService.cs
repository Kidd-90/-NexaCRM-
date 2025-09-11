using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Db;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services.Mock
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
                new DbCustomer { CustomerName = "최잠재", ContactNumber = "010-1111-1111", Status = DbStatus.New, AssignedDate = DateTime.Now.AddDays(-2), LastContactDate = DateTime.Now.AddDays(-2) },
                new DbCustomer { CustomerName = "정대기", ContactNumber = "010-2222-2222", Status = DbStatus.New, AssignedDate = DateTime.Now.AddDays(-3), LastContactDate = DateTime.Now.AddDays(-3) },

                // Assigned to SalesAgent1 ("이영업")
                new DbCustomer { CustomerName = "강신규", ContactNumber = "010-3333-3333", Status = DbStatus.New, AssignedTo = SalesAgent1, Assigner = ManagerName, AssignedDate = DateTime.Now, LastContactDate = DateTime.Now, IsStarred = true },
                new DbCustomer { CustomerName = "조상담", ContactNumber = "010-4444-4444", Status = DbStatus.InProgress, AssignedTo = SalesAgent1, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-1), LastContactDate = DateTime.Now },
                new DbCustomer { CustomerName = "오부재", ContactNumber = "010-5555-5555", Status = DbStatus.NoAnswer, AssignedTo = SalesAgent1, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-5), LastContactDate = DateTime.Now.AddDays(-1) },

                // Assigned to SalesAgent2 ("박세일")
                new DbCustomer { CustomerName = "윤계약", ContactNumber = "010-6666-6666", Status = DbStatus.Completed, AssignedTo = SalesAgent2, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-10), LastContactDate = DateTime.Now.AddDays(-2) },
                new DbCustomer { CustomerName = "황관심", ContactNumber = "010-7777-7777", Status = DbStatus.InProgress, AssignedTo = SalesAgent2, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-4), LastContactDate = DateTime.Now.AddDays(-1), IsStarred = true },
                new DbCustomer { CustomerName = "유신규", ContactNumber = "010-8888-8888", Status = DbStatus.New, AssignedTo = SalesAgent2, Assigner = ManagerName, AssignedDate = DateTime.Now, LastContactDate = DateTime.Now },

                // Assigned to Manager ("김관리")
                new DbCustomer { CustomerName = "김대표", ContactNumber = "010-9999-9999", Status = DbStatus.InProgress, AssignedTo = ManagerName, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-20), LastContactDate = DateTime.Now.AddDays(-5), IsStarred = true },
                new DbCustomer { CustomerName = "박사장", ContactNumber = "010-0000-0000", Status = DbStatus.OnHold, AssignedTo = ManagerName, Assigner = ManagerName, AssignedDate = DateTime.Now.AddDays(-30), LastContactDate = DateTime.Now.AddDays(-10) }
            };
        }

        private Task<IEnumerable<DbCustomer>> FilterData(Func<DbCustomer, bool> predicate)
        {
            return Task.FromResult(_dbCustomers.Where(predicate));
        }

        // Manager Methods
        public Task<IEnumerable<DbCustomer>> GetAllDbListAsync() => Task.FromResult(_dbCustomers.AsEnumerable());
        public Task<IEnumerable<DbCustomer>> GetTeamDbStatusAsync() => FilterData(c => !string.IsNullOrEmpty(c.AssignedTo));
        public Task<IEnumerable<DbCustomer>> GetUnassignedDbListAsync() => FilterData(c => string.IsNullOrEmpty(c.AssignedTo));
        public Task<IEnumerable<DbCustomer>> GetTodaysAssignedDbAsync() => FilterData(c => c.AssignedDate.Date == DateTime.Today);
        public Task<IEnumerable<DbCustomer>> GetDbDistributionStatusAsync() => FilterData(c => !string.IsNullOrEmpty(c.AssignedTo)); // Same as Team Status for this mock

        // Sales Methods
        public Task<IEnumerable<DbCustomer>> GetNewDbListAsync(string salesAgentName) => FilterData(c => c.AssignedTo == salesAgentName && c.Status == DbStatus.New);
        public Task<IEnumerable<DbCustomer>> GetStarredDbListAsync(string salesAgentName) => FilterData(c => c.AssignedTo == salesAgentName && c.IsStarred);
        public Task<IEnumerable<DbCustomer>> GetNewlyAssignedDbAsync(string salesAgentName) => FilterData(c => c.AssignedTo == salesAgentName && c.AssignedDate.Date == DateTime.Today);
        public Task<IEnumerable<DbCustomer>> GetMyAssignmentHistoryAsync(string salesAgentName) => FilterData(c => c.AssignedTo == salesAgentName);

        // Common Method
        public Task<IEnumerable<DbCustomer>> GetMyDbListAsync(string agentName) => FilterData(c => c.AssignedTo == agentName);
    }
}
