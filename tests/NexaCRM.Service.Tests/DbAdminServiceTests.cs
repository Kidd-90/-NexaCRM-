using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Db;
using NexaCRM.Services.Admin;
using Xunit;

namespace NexaCRM.Service.Tests;

public class DbAdminServiceTests
{
    [Fact]
    public async Task SearchAsync_FiltersByDateAndDuplicates()
    {
        // Arrange
        var today = DateTime.Today;
        var customers = new List<DbCustomer>
        {
            new()
            {
                ContactId = 1,
                CustomerName = "Alpha",
                ContactNumber = "010-1111-2222",
                AssignedDate = today,
                LastContactDate = today,
                Status = DbStatus.New
            },
            new()
            {
                ContactId = 2,
                CustomerName = "Beta",
                ContactNumber = "010-1111-2222",
                AssignedDate = today.AddDays(-1),
                LastContactDate = today.AddDays(-1),
                Status = DbStatus.New
            },
            new()
            {
                ContactId = 3,
                CustomerName = "Gamma",
                ContactNumber = "010-3333-4444",
                AssignedDate = today.AddDays(-10),
                LastContactDate = today.AddDays(-10),
                Status = DbStatus.InProgress
            }
        };

        var dataService = new FakeDbDataService(customers);
        var sut = new DbAdminService(dataService);
        var criteria = new DbSearchCriteria
        {
            CheckDuplicates = true,
            From = today.AddDays(-2),
            To = today
        };

        // Act
        var results = await sut.SearchAsync(criteria);

        // Assert
        var list = results.ToList();
        Assert.Equal(2, list.Count);
        Assert.All(list, item => Assert.Equal("01011112222", NormalizeDigits(item.ContactNumber)));
    }

    [Fact]
    public async Task ExportToExcelAsync_ProducesCsvWithSelectedFields()
    {
        // Arrange
        var today = DateTime.Today;
        var customers = new List<DbCustomer>
        {
            new()
            {
                ContactId = 1,
                CustomerName = "Alpha",
                ContactNumber = "010-1111-2222",
                AssignedDate = today,
                LastContactDate = today,
                Status = DbStatus.New
            }
        };

        var dataService = new FakeDbDataService(customers);
        var sut = new DbAdminService(dataService);
        var settings = new DbExportSettings(new List<string>
        {
            nameof(DbCustomer.CustomerName),
            nameof(DbCustomer.Status)
        });

        // Act
        var bytes = await sut.ExportToExcelAsync(settings);

        // Assert
        var csv = Encoding.UTF8.GetString(bytes);
        var lines = csv.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
        Assert.Equal("CustomerName,Status", lines[0].Trim());
        Assert.Equal($"Alpha,{DbStatus.New}", lines[1].Trim());
    }

    [Fact]
    public async Task SearchAsync_FiltersByStatusAndSearchTerm()
    {
        // Arrange
        var today = DateTime.Today;
        var customers = new List<DbCustomer>
        {
            new()
            {
                ContactId = 10,
                CustomerName = "홍길동",
                ContactNumber = "010-9999-8888",
                AssignedDate = today,
                LastContactDate = today,
                Status = DbStatus.New
            },
            new()
            {
                ContactId = 11,
                CustomerName = "김철수",
                ContactNumber = "010-1234-5678",
                AssignedDate = today,
                LastContactDate = today,
                Status = DbStatus.Completed
            },
            new()
            {
                ContactId = 12,
                CustomerName = "이영희",
                ContactNumber = "02-555-7777",
                AssignedDate = today.AddDays(-1),
                LastContactDate = today.AddDays(-1),
                Status = DbStatus.Completed
            }
        };

        var dataService = new FakeDbDataService(customers);
        var sut = new DbAdminService(dataService);
        var criteria = new DbSearchCriteria
        {
            SearchTerm = "555", // matches contact number for ContactId 12
            Status = DbStatus.Completed
        };

        // Act
        var results = await sut.SearchAsync(criteria);

        // Assert
        var list = results.ToList();
        Assert.Single(list);
        Assert.Equal(12, list[0].ContactId);
    }

    [Fact]
    public async Task SearchAsync_WithDigitOnlyTerm_MatchesFormattedContactNumber()
    {
        // Arrange
        var today = DateTime.Today;
        var customers = new List<DbCustomer>
        {
            new()
            {
                ContactId = 21,
                CustomerName = "Primary",
                ContactNumber = "010-1234-5678",
                AssignedDate = today,
                LastContactDate = today,
                Status = DbStatus.New
            },
            new()
            {
                ContactId = 22,
                CustomerName = "Other",
                ContactNumber = "+82 (10) 9876-5432",
                AssignedDate = today,
                LastContactDate = today,
                Status = DbStatus.New
            },
            new()
            {
                ContactId = 23,
                CustomerName = "Similar",
                ContactNumber = "010-1234-5670",
                AssignedDate = today,
                LastContactDate = today,
                Status = DbStatus.New
            }
        };

        var dataService = new FakeDbDataService(customers);
        var sut = new DbAdminService(dataService);
        var criteria = new DbSearchCriteria
        {
            SearchTerm = "01012345678"
        };

        // Act
        var results = await sut.SearchAsync(criteria);

        // Assert
        var list = results.ToList();
        Assert.Single(list);
        Assert.Equal(21, list[0].ContactId);
    }

    [Fact]
    public async Task ExportToExcelAsync_AppliesCriteriaFilter()
    {
        // Arrange
        var today = DateTime.Today;
        var customers = new List<DbCustomer>
        {
            new()
            {
                ContactId = 1,
                CustomerName = "Alpha",
                ContactNumber = "010-1111-2222",
                AssignedDate = today,
                LastContactDate = today,
                Status = DbStatus.New
            },
            new()
            {
                ContactId = 2,
                CustomerName = "Beta",
                ContactNumber = "010-3333-4444",
                AssignedDate = today.AddDays(-5),
                LastContactDate = today.AddDays(-5),
                Status = DbStatus.Completed
            }
        };

        var dataService = new FakeDbDataService(customers);
        var sut = new DbAdminService(dataService);
        var settings = new DbExportSettings(new List<string>
        {
            nameof(DbCustomer.CustomerName)
        });
        var criteria = new DbSearchCriteria
        {
            From = today.AddDays(-1),
            To = today,
            Status = DbStatus.New
        };

        // Act
        var bytes = await sut.ExportToExcelAsync(settings, criteria);

        // Assert
        var csv = Encoding.UTF8.GetString(bytes);
        var lines = csv.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
        Assert.Equal("CustomerName", lines[0].Trim());
        Assert.Equal("Alpha", lines[1].Trim());
    }

    [Fact]
    public async Task DeleteEntryAsync_DelegatesToDataService()
    {
        // Arrange
        var dataService = new FakeDbDataService();
        var sut = new DbAdminService(dataService);

        // Act
        await sut.DeleteEntryAsync(5);

        // Assert
        Assert.Contains(5, dataService.DeletedContactIds);
    }

    private static string NormalizeDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return new string(value.Where(char.IsDigit).ToArray());
    }

    private sealed class FakeDbDataService : IDbDataService
    {
        private readonly List<DbCustomer> _customers;

        public FakeDbDataService()
            : this(Enumerable.Empty<DbCustomer>())
        {
        }

        public FakeDbDataService(IEnumerable<DbCustomer> customers)
        {
            _customers = customers.Select(Clone).ToList();
        }

        public List<int> DeletedContactIds { get; } = new();

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

        public Task DeleteCustomersAsync(IEnumerable<int> contactIds)
        {
            foreach (var id in contactIds)
            {
                DeletedContactIds.Add(id);
                _customers.RemoveAll(c => c.ContactId == id);
            }

            return Task.CompletedTask;
        }

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
}
