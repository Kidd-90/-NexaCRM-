using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Organization;
using NexaCRM.WebClient.Services.Interfaces;
using AgentModel = NexaCRM.WebClient.Models.Agent;
using NewUserModel = NexaCRM.WebClient.Models.NewUser;

namespace NexaCRM.WebClient.Services;

public class OrganizationService : IOrganizationService
{
    private readonly List<OrganizationUnit> _organizationUnits =
    [
        new(1, "Head Office", null),
        new(2, "Sales", 1),
        new(3, "Marketing", 1),
        new(4, "Customer Success", 2)
    ];

    private readonly List<OrganizationStats> _stats =
    [
        new("Head Office", 12),
        new("Sales", 8),
        new("Marketing", 6),
        new("Customer Success", 5)
    ];

    private readonly List<OrganizationUser> _users =
    [
        new()
        {
            Id = 1,
            Name = "Alice Kim",
            Email = "alice.kim@example.com",
            Role = "Admin",
            Status = "Active",
            Department = "Operations",
            PhoneNumber = "010-1234-5678",
            RegisteredAt = DateTime.Today.AddMonths(-12),
            ApprovedAt = DateTime.Today.AddMonths(-12),
            ApprovalMemo = "최초 관리자 계정"
        },
        new()
        {
            Id = 2,
            Name = "Brian Lee",
            Email = "brian.lee@example.com",
            Role = "Manager",
            Status = "Active",
            Department = "Sales",
            PhoneNumber = "010-3456-7890",
            RegisteredAt = DateTime.Today.AddMonths(-6),
            ApprovedAt = DateTime.Today.AddMonths(-6),
            ApprovalMemo = "영업 총괄 승인"
        },
        new()
        {
            Id = 3,
            Name = "Chloe Park",
            Email = "chloe.park@example.com",
            Role = "Analyst",
            Status = "Inactive",
            Department = "Marketing",
            PhoneNumber = "010-9876-5432",
            RegisteredAt = DateTime.Today.AddMonths(-2),
            ApprovedAt = DateTime.Today.AddMonths(-2),
            ApprovalMemo = "휴직 처리"
        }
    ];

    private readonly List<AgentModel> _admins =
    [
        new() { Id = 1, Name = "Alice Kim", Email = "alice.kim@example.com", Role = "Admin" },
        new() { Id = 2, Name = "Brian Lee", Email = "brian.lee@example.com", Role = "Manager" }
    ];

    public Task<IEnumerable<OrganizationUnit>> GetOrganizationStructureAsync() =>
        Task.FromResult<IEnumerable<OrganizationUnit>>(_organizationUnits.ToList());

    public Task SaveOrganizationUnitAsync(OrganizationUnit unit)
    {
        if (unit.Id == 0)
        {
            var newUnit = unit with { Id = GenerateUnitId() };
            _organizationUnits.Add(newUnit);
        }
        else
        {
            var index = _organizationUnits.FindIndex(u => u.Id == unit.Id);
            if (index >= 0)
            {
                _organizationUnits[index] = unit;
            }
            else
            {
                _organizationUnits.Add(unit);
            }
        }

        return Task.CompletedTask;
    }

    public Task DeleteOrganizationUnitAsync(int id)
    {
        RemoveUnitRecursive(id);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<OrganizationStats>> GetOrganizationStatsAsync() =>
        Task.FromResult<IEnumerable<OrganizationStats>>(_stats.ToList());

    public Task<IEnumerable<AgentModel>> GetAdminsAsync() =>
        Task.FromResult<IEnumerable<AgentModel>>(_admins.Select(CloneAdmin).ToList());

    public Task AddAdminAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.CompletedTask;
        }

        if (_admins.Any(a => a.Id.ToString() == userId))
        {
            return Task.CompletedTask;
        }

        var id = int.TryParse(userId, out var parsed) ? parsed : GenerateAdminId();
        _admins.Add(new AgentModel
        {
            Id = id,
            Name = $"Admin {id}",
            Email = $"admin{id}@example.com",
            Role = "Admin"
        });

        return Task.CompletedTask;
    }

    public Task RemoveAdminAsync(string userId)
    {
        _admins.RemoveAll(a => a.Id.ToString() == userId);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<OrganizationUser>> GetUsersAsync() =>
        Task.FromResult<IEnumerable<OrganizationUser>>(_users.Select(CloneUser).ToList());

    public Task UpdateUserAsync(OrganizationUser user)
    {
        var existing = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existing is not null)
        {
            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.Role = user.Role;
            existing.Status = user.Status;
            existing.Department = user.Department;
            existing.PhoneNumber = user.PhoneNumber;
            existing.RegisteredAt = user.RegisteredAt;
            existing.ApprovedAt = user.ApprovedAt;
            existing.ApprovalMemo = user.ApprovalMemo;
        }

        return Task.CompletedTask;
    }

    public Task DeleteUserAsync(int userId)
    {
        _users.RemoveAll(u => u.Id == userId);
        return Task.CompletedTask;
    }

    public Task ApproveUserAsync(int userId)
    {
        var existing = _users.FirstOrDefault(u => u.Id == userId);
        if (existing is not null)
        {
            existing.Status = "Active";
            existing.ApprovedAt = DateTime.Now;
            existing.ApprovalMemo = null;
        }

        return Task.CompletedTask;
    }

    public Task RejectUserAsync(int userId, string? reason)
    {
        var existing = _users.FirstOrDefault(u => u.Id == userId);
        if (existing is not null)
        {
            existing.Status = "Rejected";
            existing.ApprovedAt = null;
            existing.ApprovalMemo = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        }

        return Task.CompletedTask;
    }

    public Task SetSystemAdminAsync(string userId) => AddAdminAsync(userId);

    public Task RegisterUserAsync(NewUserModel user)
    {
        ArgumentNullException.ThrowIfNull(user);

        _users.Add(new OrganizationUser
        {
            Id = GenerateUserId(),
            Name = user.FullName,
            Email = user.Email,
            Role = "Member",
            Status = "Pending",
            Department = "미지정",
            RegisteredAt = DateTime.Now
        });

        return Task.CompletedTask;
    }

    private int GenerateUnitId() =>
        _organizationUnits.Count == 0 ? 1 : _organizationUnits.Max(u => u.Id) + 1;

    private int GenerateUserId() =>
        _users.Count == 0 ? 1 : _users.Max(u => u.Id) + 1;

    private int GenerateAdminId() =>
        _admins.Count == 0 ? 1 : _admins.Max(a => a.Id) + 1;

    private void RemoveUnitRecursive(int id)
    {
        var children = _organizationUnits.Where(u => u.ParentId == id).Select(u => u.Id).ToList();
        _organizationUnits.RemoveAll(u => u.Id == id);
        foreach (var child in children)
        {
            RemoveUnitRecursive(child);
        }
    }

    private static OrganizationUser CloneUser(OrganizationUser source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        Email = source.Email,
        Role = source.Role,
        Status = source.Status,
        Department = source.Department,
        PhoneNumber = source.PhoneNumber,
        RegisteredAt = source.RegisteredAt,
        ApprovedAt = source.ApprovedAt,
        ApprovalMemo = source.ApprovalMemo
    };

    private static AgentModel CloneAdmin(AgentModel source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        Email = source.Email,
        Role = source.Role
    };
}
