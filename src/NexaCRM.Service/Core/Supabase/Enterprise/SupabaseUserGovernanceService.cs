using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.Service.Supabase;
using NexaCRM.UI.Models.Governance;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.Service.Supabase.Enterprise;

/// <summary>
/// In-memory Supabase backed implementation of <see cref="IUserGovernanceService"/> used for development scenarios.
/// </summary>
public sealed class SupabaseUserGovernanceService : IUserGovernanceService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly SupabaseEnterpriseDataStore _store;
    private readonly ILogger<SupabaseUserGovernanceService> _logger;
    private readonly object _auditLock = new();

    public SupabaseUserGovernanceService(
        SupabaseClientProvider clientProvider,
        SupabaseEnterpriseDataStore store,
        ILogger<SupabaseUserGovernanceService> logger)
    {
        _clientProvider = clientProvider;
        _store = store;
        _logger = logger;
    }

    public async Task<UserAccount> CreateUserAsync(
        Guid organizationId,
        string email,
        string displayName,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(organizationId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(roles);

        await EnsureClientAsync(cancellationToken).ConfigureAwait(false);

        var normalizedRoles = roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var now = DateTime.UtcNow;
        var account = new UserAccount
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Email = email,
            DisplayName = displayName,
            IsActive = true,
            CreatedAtUtc = now,
            Roles = normalizedRoles,
            Metadata = new Dictionary<string, string>
            {
                ["provisioned_at"] = now.ToString("O"),
                ["source"] = "supabase-dev"
            }
        };

        _store.Users[account.Id] = account;
        _store.UserRoles[account.Id] = new HashSet<string>(normalizedRoles, StringComparer.OrdinalIgnoreCase);

        LogAudit(account.OrganizationId, account.Id, "user.create", new { account.Email, account.DisplayName });

        return account;
    }

    public Task<UserAccount?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        if (_store.Users.TryGetValue(userId, out var account))
        {
            var roles = ResolveRoles(userId);
            return Task.FromResult<UserAccount?>(account with { Roles = roles });
        }

        return Task.FromResult<UserAccount?>(null);
    }

    public Task<IReadOnlyList<UserAccount>> GetUsersAsync(
        UserQueryOptions query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var users = _store.Users.Values.AsEnumerable();

        if (query.OrganizationId.HasValue)
        {
            users = users.Where(u => u.OrganizationId == query.OrganizationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            users = users.Where(u =>
                u.Email.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                u.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (!query.IncludeInactive)
        {
            users = users.Where(u => u.IsActive);
        }

        if (query.RoleFilter is { Count: > 0 })
        {
            var filter = query.RoleFilter.Select(r => r.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            users = users.Where(u => ResolveRoles(u.Id).Any(filter.Contains));
        }

        var resolved = users
            .Select(u => u with { Roles = ResolveRoles(u.Id) })
            .OrderBy(u => u.Email)
            .ToList();

        return Task.FromResult<IReadOnlyList<UserAccount>>(resolved);
    }

    public Task AssignRolesAsync(
        Guid userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        ArgumentNullException.ThrowIfNull(roles);

        if (!_store.Users.ContainsKey(userId))
        {
            throw new InvalidOperationException($"User {userId} could not be found.");
        }

        var normalizedRoles = roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        _store.UserRoles[userId] = normalizedRoles;
        LogAudit(_store.Users[userId].OrganizationId, userId, "user.roles.assign", normalizedRoles);

        return Task.CompletedTask;
    }

    public Task<PasswordResetTicket> CreatePasswordResetTicketAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        if (!_store.Users.TryGetValue(userId, out var account))
        {
            throw new InvalidOperationException($"User {userId} could not be found.");
        }

        var ticket = new PasswordResetTicket
        {
            TicketId = Guid.NewGuid(),
            UserId = userId,
            OrganizationId = account.OrganizationId,
            IssuedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddHours(2)
        };

        _store.PasswordResetTickets[userId] = ticket;
        LogAudit(account.OrganizationId, userId, "user.password.reset_ticket", new { ticket.TicketId });

        return Task.FromResult(ticket);
    }

    public Task DisableUserAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        if (!_store.Users.TryGetValue(userId, out var account))
        {
            throw new InvalidOperationException($"User {userId} could not be found.");
        }

        var metadata = account.Metadata
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);
        metadata["disabled_reason"] = reason;
        metadata["disabled_at"] = DateTime.UtcNow.ToString("O");

        var updated = account with
        {
            IsActive = false,
            Metadata = metadata
        };

        _store.Users[userId] = updated;
        LogAudit(account.OrganizationId, userId, "user.disable", new { reason });

        return Task.CompletedTask;
    }

    public Task<SecurityPolicy> GetSecurityPolicyAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(organizationId));
        }

        if (_store.SecurityPolicies.TryGetValue(organizationId, out var policy))
        {
            return Task.FromResult(policy);
        }

        return Task.FromResult(new SecurityPolicy());
    }

    public Task SaveSecurityPolicyAsync(
        Guid organizationId,
        SecurityPolicy policy,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(organizationId));
        }

        ArgumentNullException.ThrowIfNull(policy);

        _store.SecurityPolicies[organizationId] = policy;
        LogAudit(organizationId, null, "security.policy.update", policy);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<SecurityAuditLogEntry>> GetAuditTrailAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(organizationId));
        }

        if (_store.AuditLogs.TryGetValue(organizationId, out var list))
        {
            return Task.FromResult<IReadOnlyList<SecurityAuditLogEntry>>(list
                .OrderByDescending(x => x.OccurredAtUtc)
                .ToList());
        }

        return Task.FromResult<IReadOnlyList<SecurityAuditLogEntry>>(Array.Empty<SecurityAuditLogEntry>());
    }

    private async Task EnsureClientAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _clientProvider.GetClientAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Supabase client unavailable; operating against in-memory store.");
        }
    }

    private IReadOnlyCollection<string> ResolveRoles(Guid userId)
    {
        if (_store.UserRoles.TryGetValue(userId, out var roles))
        {
            return roles.ToArray();
        }

        return Array.Empty<string>();
    }

    private void LogAudit(Guid organizationId, Guid? entityId, string action, object payload)
    {
        var entry = new SecurityAuditLogEntry
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            ActorId = null,
            Action = action,
            EntityType = entityId.HasValue ? "user" : "system",
            EntityId = entityId?.ToString(),
            PayloadJson = JsonSerializer.Serialize(payload),
            OccurredAtUtc = DateTime.UtcNow
        };

        lock (_auditLock)
        {
            var log = _store.AuditLogs.GetOrAdd(organizationId, _ => new List<SecurityAuditLogEntry>());
            log.Add(entry);
        }
    }
}
