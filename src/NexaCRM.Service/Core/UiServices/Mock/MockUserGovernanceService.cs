using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.UI.Models.Governance;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.UI.Services.Mock;

public sealed class MockUserGovernanceService : IUserGovernanceService
{
    private readonly ConcurrentDictionary<Guid, UserAccount> _accounts = new();
    private readonly ConcurrentDictionary<Guid, SecurityPolicy> _policies = new();
    private readonly ConcurrentDictionary<Guid, PasswordResetTicket> _tickets = new();
    private readonly ConcurrentQueue<SecurityAuditLogEntry> _auditLogs = new();

    public Task<UserAccount> CreateUserAsync(
        Guid organizationId,
        string email,
        string displayName,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        var account = new UserAccount
        {
            Id = id,
            OrganizationId = organizationId,
            Email = email ?? string.Empty,
            DisplayName = displayName ?? string.Empty,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            LastSignInAtUtc = null,
            Roles = roles?.ToArray() ?? Array.Empty<string>(),
            Metadata = new Dictionary<string, string>
            {
                ["source"] = "mock"
            }
        };

        _accounts[id] = account;
        _auditLogs.Enqueue(new SecurityAuditLogEntry
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            ActorId = null,
            Action = "UserCreated",
            EntityType = "UserAccount",
            EntityId = id.ToString(),
            PayloadJson = null,
            OccurredAtUtc = DateTime.UtcNow
        });

        return Task.FromResult(account);
    }

    public Task<UserAccount?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var account = _accounts.TryGetValue(userId, out var value) ? value : null;
        return Task.FromResult(account);
    }

    public Task<IReadOnlyList<UserAccount>> GetUsersAsync(
        UserQueryOptions query,
        CancellationToken cancellationToken = default)
    {
        query ??= new UserQueryOptions();

        var results = _accounts.Values
            .Where(account => !query.OrganizationId.HasValue || account.OrganizationId == query.OrganizationId)
            .Where(account => query.IncludeInactive || account.IsActive)
            .Where(account => string.IsNullOrWhiteSpace(query.SearchTerm)
                || account.Email.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase)
                || account.DisplayName.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase))
            .Where(account => query.RoleFilter is null || !query.RoleFilter.Any()
                || account.Roles.Intersect(query.RoleFilter, StringComparer.OrdinalIgnoreCase).Any())
            .OrderBy(account => account.DisplayName)
            .ThenBy(account => account.Email)
            .Skip((Math.Max(1, query.PageNumber) - 1) * Math.Max(1, query.PageSize))
            .Take(Math.Max(1, query.PageSize))
            .ToList();

        return Task.FromResult<IReadOnlyList<UserAccount>>(results);
    }

    public Task AssignRolesAsync(
        Guid userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        if (!_accounts.TryGetValue(userId, out var account))
        {
            return Task.CompletedTask;
        }

        var updated = account with
        {
            Roles = roles?.Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? Array.Empty<string>()
        };

        _accounts[userId] = updated;
        _auditLogs.Enqueue(new SecurityAuditLogEntry
        {
            Id = Guid.NewGuid(),
            OrganizationId = account.OrganizationId,
            ActorId = userId,
            Action = "RolesUpdated",
            EntityType = "UserAccount",
            EntityId = userId.ToString(),
            PayloadJson = null,
            OccurredAtUtc = DateTime.UtcNow
        });

        return Task.CompletedTask;
    }

    public Task<PasswordResetTicket> CreatePasswordResetTicketAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var ticket = new PasswordResetTicket
        {
            TicketId = Guid.NewGuid(),
            UserId = userId,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
            ResetUrl = $"https://example.com/reset/{userId:N}"
        };

        _tickets[ticket.TicketId] = ticket;
        return Task.FromResult(ticket);
    }

    public Task DisableUserAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (_accounts.TryGetValue(userId, out var account))
        {
            _accounts[userId] = account with { IsActive = false };
            _auditLogs.Enqueue(new SecurityAuditLogEntry
            {
                Id = Guid.NewGuid(),
                OrganizationId = account.OrganizationId,
                ActorId = userId,
                Action = "UserDisabled",
                EntityType = "UserAccount",
                EntityId = userId.ToString(),
                PayloadJson = reason,
                OccurredAtUtc = DateTime.UtcNow
            });
        }

        return Task.CompletedTask;
    }

    public Task<SecurityPolicy> GetSecurityPolicyAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var policy = _policies.GetOrAdd(organizationId, id => new SecurityPolicy
        {
            RequireMfa = false,
            SessionTimeoutMinutes = 60,
            PasswordExpiryDays = 120,
            IpAllowList = Array.Empty<string>()
        });

        return Task.FromResult(policy);
    }

    public Task SaveSecurityPolicyAsync(
        Guid organizationId,
        SecurityPolicy policy,
        CancellationToken cancellationToken = default)
    {
        if (policy is null)
        {
            throw new ArgumentNullException(nameof(policy));
        }

        _policies[organizationId] = policy;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<SecurityAuditLogEntry>> GetAuditTrailAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var items = _auditLogs
            .Where(entry => entry.OrganizationId == organizationId)
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .Take(200)
            .ToList();

        return Task.FromResult<IReadOnlyList<SecurityAuditLogEntry>>(items);
    }
}
