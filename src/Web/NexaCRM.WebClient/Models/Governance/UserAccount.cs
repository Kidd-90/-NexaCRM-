using System;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Models.Governance;

/// <summary>
/// Represents a managed user account inside NexaCRM.
/// </summary>
public sealed class UserAccount
{
    public Guid Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? LastSignInAt { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();

    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>();
}

/// <summary>
/// Optional query parameters when enumerating users for an organization.
/// </summary>
public sealed class UserQueryOptions
{
    public string? SearchTerm { get; init; }

    public bool IncludeInactive { get; init; }

    public int PageSize { get; init; } = 50;

    public int PageNumber { get; init; } = 1;

    public IReadOnlyCollection<string>? RoleFilter { get; init; }
}

/// <summary>
/// Describes an audit log entry produced by the governance pipeline.
/// </summary>
public sealed class SecurityAuditLogEntry
{
    public Guid Id { get; init; }

    public Guid? ActorId { get; init; }

    public string Action { get; init; } = string.Empty;

    public string EntityType { get; init; } = string.Empty;

    public string? EntityId { get; init; }

    public string? PayloadJson { get; init; }

    public DateTime OccurredAt { get; init; }
}

/// <summary>
/// Captures the outcome of a password reset or recovery flow.
/// </summary>
public sealed class PasswordResetTicket
{
    public Guid TicketId { get; init; }

    public Guid UserId { get; init; }

    public DateTime ExpiresAt { get; init; }

    public string ResetUrl { get; init; } = string.Empty;
}

/// <summary>
/// Defines security controls that should be enforced for end-users.
/// </summary>
public sealed class SecurityPolicy
{
    public bool RequireMfa { get; init; }

    public int SessionTimeoutMinutes { get; init; } = 60;

    public IReadOnlyCollection<string> IpAllowList { get; init; } = Array.Empty<string>();

    public int PasswordExpiryDays { get; init; } = 90;
}
