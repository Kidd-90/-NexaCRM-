using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Organization;
using AgentModel = NexaCRM.Services.Admin.Models.Agent;
using NewUserModel = NexaCRM.Services.Admin.Models.NewUser;

namespace NexaCRM.WebClient.Services.Admin;

public sealed class OrganizationService : IOrganizationService
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

    public Task SetSystemAdminAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.CompletedTask;
        }

        if (!int.TryParse(userId, out var parsedId))
        {
            return Task.CompletedTask;
        }

        var user = _users.FirstOrDefault(u => u.Id == parsedId);
        if (user is null)
        {
            return Task.CompletedTask;
        }

        user.Role = "Admin";
        user.Status = "Active";
        user.ApprovedAt ??= DateTime.UtcNow;
        user.ApprovalMemo = "Elevated to system administrator";

        if (_admins.All(a => a.Id != parsedId))
        {
            _admins.Add(new AgentModel
            {
                Id = parsedId,
                Name = user.Name,
                Email = user.Email,
                Role = "Admin"
            });
        }

        return Task.CompletedTask;
    }

    public Task RegisterUserAsync(NewUserModel user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(user.FullName))
        {
            throw new ArgumentException("Full name is required.", nameof(user));
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            throw new ArgumentException("Email is required.", nameof(user));
        }

        var newUser = new OrganizationUser
        {
            Id = GenerateUserId(),
            Name = user.FullName.Trim(),
            Email = user.Email.Trim(),
            Role = "Member",
            Status = "Pending",
            Department = string.Empty,
            PhoneNumber = string.Empty,
            RegisteredAt = DateTime.UtcNow,
            ApprovedAt = null,
            ApprovalMemo = "Registration requested"
        };

        _users.Add(newUser);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<NewUserModel>> GetPendingUsersAsync()
    {
        var pending = _users
            .Where(u => string.Equals(u.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            .Select(u => new NewUserModel
            {
                FullName = u.Name ?? string.Empty,
                Email = u.Email,
                Password = string.Empty,
                ConfirmPassword = string.Empty,
                TermsAccepted = true
            })
            .ToList();

        return Task.FromResult<IEnumerable<NewUserModel>>(pending);
    }

    public Task InviteUserAsync(NewUserModel newUser)
    {
        if (string.IsNullOrWhiteSpace(newUser.Email))
        {
            return Task.CompletedTask;
        }

        var id = GenerateUserId();
        var name = string.IsNullOrWhiteSpace(newUser.FullName)
            ? $"Invited {id}"
            : newUser.FullName.Trim();
        _users.Add(new OrganizationUser
        {
            Id = id,
            Name = name,
            Email = newUser.Email.Trim(),
            Role = "Member",
            Status = "Pending",
            Department = string.Empty,
            RegisteredAt = DateTime.UtcNow,
            ApprovalMemo = "Invitation sent"
        });

        return Task.CompletedTask;
    }

    private static AgentModel CloneAdmin(AgentModel admin) => new()
    {
        Id = admin.Id,
        Name = admin.Name,
        Email = admin.Email,
        Role = admin.Role
    };

    private static OrganizationUser CloneUser(OrganizationUser user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role,
        Status = user.Status,
        Department = user.Department,
        PhoneNumber = user.PhoneNumber,
        RegisteredAt = user.RegisteredAt,
        ApprovedAt = user.ApprovedAt,
        ApprovalMemo = user.ApprovalMemo
    };

    private int GenerateUnitId() => _organizationUnits.Count == 0 ? 1 : _organizationUnits.Max(u => u.Id) + 1;

    private int GenerateAdminId() => _admins.Count == 0 ? 1 : _admins.Max(a => a.Id) + 1;

    private int GenerateUserId() => _users.Count == 0 ? 1 : _users.Max(u => u.Id) + 1;

    private void RemoveUnitRecursive(int id)
    {
        var toRemove = _organizationUnits.Where(u => u.ParentId == id).Select(u => u.Id).ToList();
        foreach (var childId in toRemove)
        {
            RemoveUnitRecursive(childId);
        }

        _organizationUnits.RemoveAll(u => u.Id == id);
    }
}
