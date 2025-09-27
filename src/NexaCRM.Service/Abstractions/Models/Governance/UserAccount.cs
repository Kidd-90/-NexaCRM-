using System;
using System.Collections.Generic;

namespace NexaCRM.UI.Models.Governance;

/// <summary>
/// Represents a managed user account inside NexaCRM.
/// </summary>
public sealed record UserAccount
{
    public Guid Id { get; init; }

    public Guid OrganizationId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? LastSignInAtUtc { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();

    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>();
}

/// <summary>
/// Optional query parameters when enumerating users for an organization.
/// </summary>
public sealed record UserQueryOptions
{
    public Guid? OrganizationId { get; init; }

    public string? SearchTerm { get; init; }

    public bool IncludeInactive { get; init; }

    public IReadOnlyCollection<string>? RoleFilter { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 50;
}

/// <summary>
/// Captures the outcome of a password reset flow.
/// </summary>
public sealed record PasswordResetTicket
{
    public Guid TicketId { get; init; }

    public Guid UserId { get; init; }

    public Guid OrganizationId { get; set; }
    
    public DateTime IssuedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; init; }

    public string ResetUrl { get; init; } = string.Empty;
}

/// <summary>
/// Describes an audit log entry produced by the governance pipeline.
/// </summary>
public sealed record SecurityAuditLogEntry
{
    public Guid Id { get; init; }

    public Guid OrganizationId { get; init; }

    public Guid? ActorId { get; init; }

    public string Action { get; init; } = string.Empty;

    public string EntityType { get; init; } = string.Empty;

    public string? EntityId { get; init; }

    public string? PayloadJson { get; init; }

    public DateTime OccurredAtUtc { get; init; }
}

/// <summary>
/// Defines security controls that should be enforced for end-users.
/// </summary>
public sealed record SecurityPolicy
{
    public bool RequireMfa { get; init; }

    public int SessionTimeoutMinutes { get; init; } = 60;

    public int PasswordExpiryDays { get; init; } = 90;

    public IReadOnlyCollection<string> IpAllowList { get; init; } = Array.Empty<string>();
}
