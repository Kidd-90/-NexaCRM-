using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Governance;

namespace NexaCRM.WebClient.Services.Interfaces;

public interface IUserGovernanceService
{
    Task<UserAccount> CreateUserAsync(
        string email,
        string displayName,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default);

    Task<UserAccount?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserAccount>> GetUsersAsync(
        UserQueryOptions query,
        CancellationToken cancellationToken = default);

    Task AssignRolesAsync(
        Guid userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default);

    Task<PasswordResetTicket> CreatePasswordResetTicketAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task DisableUserAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken = default);

    Task<SecurityPolicy> GetSecurityPolicyAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task SaveSecurityPolicyAsync(
        Guid organizationId,
        SecurityPolicy policy,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SecurityAuditLogEntry>> GetAuditTrailAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
