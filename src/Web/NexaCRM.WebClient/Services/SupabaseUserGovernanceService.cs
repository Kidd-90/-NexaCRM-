using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NexaCRM.WebClient.Models.Governance;
using NexaCRM.WebClient.Models.Supabase;
using NexaCRM.WebClient.Services.Interfaces;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseUserGovernanceService : IUserGovernanceService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseUserGovernanceService> _logger;

    public SupabaseUserGovernanceService(
        SupabaseClientProvider clientProvider,
        ILogger<SupabaseUserGovernanceService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<UserAccount> CreateUserAsync(
        string email,
        string displayName,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(roles);

        try
        {
            var client = await _clientProvider.GetClientAsync();

            var randomPassword = $"{Guid.NewGuid():N}!Aa1";
            await client.Auth.SignUp(email, randomPassword);

            var record = new UserAccountRecord
            {
                Id = Guid.NewGuid(),
                Email = email,
                DisplayName = displayName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                MetadataJson = JsonConvert.SerializeObject(new { ProvisionedAt = DateTime.UtcNow })
            };

            await client.From<UserAccountRecord>().Insert(record, cancellationToken: cancellationToken);

            await AssignRolesInternalAsync(client, record.Id, roles, cancellationToken);
            await LogAuditAsync(client, record.Id, "user.create", cancellationToken);

            return MapToDomain(record, roles.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Supabase user account.");
            throw;
        }
    }

    public async Task<UserAccount?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<UserAccountRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, userId)
                .Single(cancellationToken: cancellationToken);

            if (response.Model is null)
            {
                return null;
            }

            var roles = await LoadRolesAsync(client, userId, cancellationToken);
            return MapToDomain(response.Model, roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load user account {UserId} from Supabase.", userId);
            throw;
        }
    }

    public async Task<IReadOnlyList<UserAccount>> GetUsersAsync(
        UserQueryOptions query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var table = client.From<UserAccountRecord>();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                table = table.Filter(x => x.Email, PostgrestOperator.ILike, $"%{query.SearchTerm!}%");
            }

            if (!query.IncludeInactive)
            {
                table = table.Filter(x => x.IsActive, PostgrestOperator.Equals, true);
            }

            var pageSize = Math.Clamp(query.PageSize, 1, 200);
            var offset = (Math.Max(query.PageNumber, 1) - 1) * pageSize;

            var response = await table
                .Order(x => x.CreatedAt, PostgrestOrdering.Descending)
                .Range(offset, offset + pageSize - 1)
                .Get(cancellationToken: cancellationToken);

            var records = response.Models ?? new List<UserAccountRecord>();
            var roleMap = await LoadRolesAsync(client, records.Select(x => x.Id).ToArray(), cancellationToken);

            return records
                .Select(record =>
                {
                    var roles = roleMap.TryGetValue(record.Id, out var assigned)
                        ? assigned
                        : Array.Empty<string>();
                    return MapToDomain(record, roles);
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enumerate Supabase users.");
            throw;
        }
    }

    public async Task AssignRolesAsync(
        Guid userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(roles);
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            await AssignRolesInternalAsync(client, userId, roles, cancellationToken);
            await LogAuditAsync(client, userId, "user.roles.update", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign roles for user {UserId}.", userId);
            throw;
        }
    }

    public async Task<PasswordResetTicket> CreatePasswordResetTicketAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var ticket = new PasswordResetTicketRecord
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                ResetUrl = $"/reset-password/{Guid.NewGuid():N}"
            };

            await client.From<PasswordResetTicketRecord>().Insert(ticket, cancellationToken: cancellationToken);
            await LogAuditAsync(client, userId, "user.password.reset", cancellationToken);

            return new PasswordResetTicket
            {
                TicketId = ticket.Id,
                UserId = ticket.UserId,
                ExpiresAt = ticket.ExpiresAt,
                ResetUrl = ticket.ResetUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create password reset ticket for {UserId}.", userId);
            throw;
        }
    }

    public async Task DisableUserAsync(
        Guid userId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<UserAccountRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, userId)
                .Single(cancellationToken: cancellationToken);

            if (response.Model is null)
            {
                return;
            }

            response.Model.IsActive = false;
            response.Model.MetadataJson = JsonConvert.SerializeObject(new
            {
                DisabledReason = reason,
                DisabledAt = DateTime.UtcNow
            });

            await client.From<UserAccountRecord>().Update(response.Model);

            await LogAuditAsync(client, userId, "user.disable", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable user {UserId}.", userId);
            throw;
        }
    }

    public async Task<SecurityPolicy> GetSecurityPolicyAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(organizationId));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<SecurityPolicyRecord>()
                .Filter(x => x.OrganizationId, PostgrestOperator.Equals, organizationId)
                .Single(cancellationToken: cancellationToken);

            if (response.Model is null)
            {
                return new SecurityPolicy();
            }

            return new SecurityPolicy
            {
                RequireMfa = response.Model.RequireMfa,
                SessionTimeoutMinutes = response.Model.SessionTimeoutMinutes,
                PasswordExpiryDays = response.Model.PasswordExpiryDays,
                IpAllowList = DeserializeList(response.Model.IpAllowList)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load security policy for organization {OrgId}.", organizationId);
            throw;
        }
    }

    public async Task SaveSecurityPolicyAsync(
        Guid organizationId,
        SecurityPolicy policy,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(organizationId));
        }

        ArgumentNullException.ThrowIfNull(policy);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var existing = await client.From<SecurityPolicyRecord>()
                .Filter(x => x.OrganizationId, PostgrestOperator.Equals, organizationId)
                .Single(cancellationToken: cancellationToken);

            var record = new SecurityPolicyRecord
            {
                Id = existing.Model?.Id ?? Guid.NewGuid(),
                OrganizationId = organizationId,
                RequireMfa = policy.RequireMfa,
                SessionTimeoutMinutes = policy.SessionTimeoutMinutes,
                PasswordExpiryDays = policy.PasswordExpiryDays,
                IpAllowList = JsonConvert.SerializeObject(policy.IpAllowList ?? Array.Empty<string>())
            };

            if (existing.Model is null)
            {
                await client.From<SecurityPolicyRecord>().Insert(record, cancellationToken: cancellationToken);
            }
            else
            {
                await client.From<SecurityPolicyRecord>().Update(record);
            }

            await LogAuditAsync(client, organizationId, "security.policy.update", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist security policy for {OrgId}.", organizationId);
            throw;
        }
    }

    public async Task<IReadOnlyList<SecurityAuditLogEntry>> GetAuditTrailAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id cannot be empty.", nameof(organizationId));
        }

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<AuditLogRecord>()
                .Filter(x => x.EntityId, PostgrestOperator.Equals, organizationId.ToString())
                .Order(x => x.CreatedAt, PostgrestOrdering.Descending)
                .Range(0, 99)
                .Get(cancellationToken: cancellationToken);

            return response.Models
                .Select(record => new SecurityAuditLogEntry
                {
                    Id = record.Id,
                    ActorId = record.ActorId,
                    Action = record.Action,
                    EntityType = record.EntityType,
                    EntityId = record.EntityId,
                    PayloadJson = record.PayloadJson,
                    OccurredAt = record.CreatedAt ?? DateTime.UtcNow
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load audit log for organization {OrgId}.", organizationId);
            throw;
        }
    }

    private static UserAccount MapToDomain(UserAccountRecord record, IReadOnlyCollection<string> roles)
    {
        var metadata = string.IsNullOrWhiteSpace(record.MetadataJson)
            ? new Dictionary<string, string>()
            : JsonConvert.DeserializeObject<Dictionary<string, string>>(record.MetadataJson!)
                ?? new Dictionary<string, string>();

        return new UserAccount
        {
            Id = record.Id,
            Email = record.Email,
            DisplayName = record.DisplayName,
            IsActive = record.IsActive ?? true,
            CreatedAt = record.CreatedAt ?? DateTime.UtcNow,
            LastSignInAt = record.LastSignInAt,
            Roles = roles,
            Metadata = metadata
        };
    }

    private static IReadOnlyCollection<string> DeserializeList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        return JsonConvert.DeserializeObject<List<string>>(json!) ?? new List<string>();
    }

    private static async Task LogAuditAsync(
        Supabase.Client client,
        Guid entityId,
        string action,
        CancellationToken cancellationToken)
    {
        var record = new AuditLogRecord
        {
            Id = Guid.NewGuid(),
            EntityType = "user",
            EntityId = entityId.ToString(),
            Action = action,
            CreatedAt = DateTime.UtcNow
        };

        await client.From<AuditLogRecord>().Insert(record, cancellationToken: cancellationToken);
    }

    private static async Task AssignRolesInternalAsync(
        Supabase.Client client,
        Guid userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken)
    {
        var roleList = roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        await client.From<UserRoleRecord>()
            .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
            .Delete(cancellationToken: cancellationToken);

        if (roleList.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var records = roleList.Select(role => new UserRoleRecord
        {
            UserId = userId,
            RoleCode = role,
            AssignedAt = now
        });

        await client.From<UserRoleRecord>().Insert(records, cancellationToken: cancellationToken);
    }

    private static async Task<IReadOnlyCollection<string>> LoadRolesAsync(
        Supabase.Client client,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var response = await client.From<UserRoleRecord>()
            .Filter(x => x.UserId, PostgrestOperator.Equals, userId)
            .Get(cancellationToken: cancellationToken);

        return response.Models.Select(record => record.RoleCode).ToList();
    }

    private static async Task<Dictionary<Guid, IReadOnlyCollection<string>>> LoadRolesAsync(
        Supabase.Client client,
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<Guid, IReadOnlyCollection<string>>();
        }

        var response = await client.From<UserRoleRecord>()
            .Filter(x => x.UserId, PostgrestOperator.In, userIds.Select(id => id.ToString()).ToArray())
            .Get(cancellationToken: cancellationToken);

        return response.Models
            .GroupBy(record => record.UserId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyCollection<string>)group.Select(record => record.RoleCode).ToList());
    }
}
