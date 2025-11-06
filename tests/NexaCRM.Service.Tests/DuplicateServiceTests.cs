using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.Services.Admin;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Db;
using Xunit;

namespace NexaCRM.Service.Tests;

public class DuplicateServiceTests
{
    [Fact]
    public async Task FindDuplicatesAsync_NormalizesContactNumbers()
    {
        // Arrange
        var today = DateTime.Today;
        var customers = new List<DbCustomer>
        {
            new()
            {
                ContactId = 101,
                CustomerName = "Primary",
                ContactNumber = "010-1234-5678",
                AssignedDate = today,
                LastContactDate = today,
                Status = DbStatus.New
            },
            new()
            {
                ContactId = 102,
                CustomerName = "Formatted",
                ContactNumber = "+82 (10) 1234 5678",
                AssignedDate = today.AddDays(-1),
                LastContactDate = today.AddDays(-1),
                Status = DbStatus.New
            },
            new()
            {
                ContactId = 103,
                CustomerName = "Unique",
                ContactNumber = "010-0000-0000",
                AssignedDate = today,
                LastContactDate = today,
                Status = DbStatus.New
            }
        };

        var dataService = new FakeDbDataService(customers);
        var configService = new FakeDedupeConfigService
        {
            ScoreThreshold = 0,
            UseGender = false,
            UseAddress = false,
            UseJobTitle = false,
            UseMaritalStatus = false,
            UseProofNumber = false,
            UseDbPrice = false,
            UseHeadquarters = false,
            UseInsuranceName = false,
            UseCarJoinDate = false,
            UseNotes = false,
            WeightGender = 1,
            WeightAddress = 1,
            WeightJobTitle = 1,
            WeightMaritalStatus = 1,
            WeightProofNumber = 1,
            WeightDbPrice = 1,
            WeightHeadquarters = 1,
            WeightInsuranceName = 1,
            WeightCarJoinDate = 1,
            WeightNotes = 1
        };

        var sut = new DuplicateService(dataService, configService);

        // Act
        var duplicates = await sut.FindDuplicatesAsync(withinDays: 30, includeFuzzy: false);

        // Assert
        var group = Assert.Single(duplicates);
        Assert.Equal("01012345678", group.Key);
        Assert.Equal("01012345678", group.ContactDisplay);
        Assert.Equal(2, group.Count);
        Assert.Contains(101, group.ContactIds);
        Assert.Contains(102, group.ContactIds);
        Assert.Equal(2, group.Candidates.Count);
        Assert.All(group.Candidates, candidate =>
        {
            Assert.Contains(candidate.ContactId, new[] { 101, 102 });
        });
    }

    private sealed class FakeDbDataService : IDbDataService
    {
        private readonly List<DbCustomer> _customers;

        public FakeDbDataService(IEnumerable<DbCustomer> customers)
        {
            _customers = customers.Select(Clone).ToList();
        }

        public Task<IEnumerable<DbCustomer>> GetAllDbListAsync() =>
            Task.FromResult<IEnumerable<DbCustomer>>(_customers.Select(Clone));

        public Task<IEnumerable<DbCustomer>> GetTeamDbStatusAsync() => EmptyAsync();

        public Task<IEnumerable<DbCustomer>> GetUnassignedDbListAsync() => EmptyAsync();

        public Task<IEnumerable<DbCustomer>> GetTodaysAssignedDbAsync() => EmptyAsync();

        public Task<IEnumerable<DbCustomer>> GetDbDistributionStatusAsync() => EmptyAsync();

        public Task AssignDbToAgentAsync(int contactId, string agentName) => Task.CompletedTask;

        public Task ReassignDbAsync(int contactId, string agentName) => Task.CompletedTask;

        public Task RecallDbAsync(int contactId) => Task.CompletedTask;

        public Task<IEnumerable<DbCustomer>> GetNewDbListAsync(string salesAgentName) => EmptyAsync();

        public Task<IEnumerable<DbCustomer>> GetStarredDbListAsync(string salesAgentName) => EmptyAsync();

        public Task<IEnumerable<DbCustomer>> GetNewlyAssignedDbAsync(string salesAgentName) => EmptyAsync();

        public Task<IEnumerable<DbCustomer>> GetMyAssignmentHistoryAsync(string salesAgentName) => EmptyAsync();

        public Task<IEnumerable<DbCustomer>> GetMyDbListAsync(string agentName) => EmptyAsync();

        public Task ArchiveCustomersAsync(IEnumerable<int> contactIds) => Task.CompletedTask;

        public Task DeleteCustomersAsync(IEnumerable<int> contactIds) => Task.CompletedTask;

        public Task MergeCustomersAsync(int primaryContactId, IEnumerable<int> duplicateContactIds) => Task.CompletedTask;

        public Task UpdateCustomerPartialAsync(int contactId, DbCustomer patch, bool overwriteEmptyOnly = false) => Task.CompletedTask;

        private static Task<IEnumerable<DbCustomer>> EmptyAsync() =>
            Task.FromResult<IEnumerable<DbCustomer>>(Array.Empty<DbCustomer>());

        private static DbCustomer Clone(DbCustomer source) => new()
        {
            Id = source.Id,
            ContactId = source.ContactId,
            CustomerName = source.CustomerName,
            ContactNumber = source.ContactNumber,
            Group = source.Group,
            AssignedTo = source.AssignedTo,
            AssignedDate = source.AssignedDate,
            Status = source.Status,
            LastContactDate = source.LastContactDate,
            IsStarred = source.IsStarred,
            Assigner = source.Assigner,
            IsArchived = source.IsArchived,
            Gender = source.Gender,
            Address = source.Address,
            JobTitle = source.JobTitle,
            MaritalStatus = source.MaritalStatus,
            ProofNumber = source.ProofNumber,
            DbPrice = source.DbPrice,
            Headquarters = source.Headquarters,
            InsuranceName = source.InsuranceName,
            CarJoinDate = source.CarJoinDate,
            Notes = source.Notes
        };
    }

    private sealed class FakeDedupeConfigService : IDedupeConfigService
    {
        public bool Enabled { get; set; } = true;
        public int Days { get; set; } = 30;
        public bool IncludeFuzzy { get; set; }
        public int ScoreThreshold { get; set; } = 0;
        public int MonitorIntervalMinutes { get; set; } = 5;
        public bool NotifyOnSameCount { get; set; }
        public bool UseGender { get; set; } = true;
        public bool UseAddress { get; set; } = true;
        public bool UseJobTitle { get; set; } = true;
        public bool UseMaritalStatus { get; set; } = true;
        public bool UseProofNumber { get; set; } = true;
        public bool UseDbPrice { get; set; } = true;
        public bool UseHeadquarters { get; set; } = true;
        public bool UseInsuranceName { get; set; } = true;
        public bool UseCarJoinDate { get; set; } = true;
        public bool UseNotes { get; set; } = true;
        public int WeightGender { get; set; } = 1;
        public int WeightAddress { get; set; } = 1;
        public int WeightJobTitle { get; set; } = 1;
        public int WeightMaritalStatus { get; set; } = 1;
        public int WeightProofNumber { get; set; } = 1;
        public int WeightDbPrice { get; set; } = 1;
        public int WeightHeadquarters { get; set; } = 1;
        public int WeightInsuranceName { get; set; } = 1;
        public int WeightCarJoinDate { get; set; } = 1;
        public int WeightNotes { get; set; } = 1;
        public event Action? Changed;

        public void RaiseChanged() => Changed?.Invoke();
    }
}
