using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.Service.Abstractions.Models.Supabase;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Organization;
using NexaCRM.Services.Admin.Models.Supabase;
using NexaCRM.UI.Models.Supabase;
using Supabase;

namespace NexaCRM.Services.Admin.Services;

public class SupabaseUserManagementService : IUserManagementService
{
    private readonly Client _supabaseClient;
    private readonly ILogger<SupabaseUserManagementService> _logger;

    public SupabaseUserManagementService(
        Client supabaseClient,
        ILogger<SupabaseUserManagementService> logger)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
    }

    public async Task<IEnumerable<OrganizationUser>> GetAllUsersAsync()
    {
        var response = await _supabaseClient
            .From<UserAccountOverviewRecord>()
            .Get();

        var users = response.Models
            .Select(record =>
            {
                _logger.LogInformation("[GetAllUsersAsync] CUID: {Cuid}, Email: {Email}, FullName: {FullName}", 
                    record.Cuid, record.Email ?? "(null)", record.FullName ?? "(null)");
                return MapToOrganizationUser(record);
            })
            .ToList();

        return users;
    }

    public async Task<OrganizationUser?> GetUserByCuidAsync(string cuid)
    {
        try
        {
            var response = await _supabaseClient
                .From<UserAccountOverviewRecord>()
                .Where(u => u.Cuid == cuid)
                .Single();

            return response != null ? MapToOrganizationUser(response) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user by CUID: {Cuid}", cuid);
            throw;
        }
    }

    public async Task<bool> UpdateUserAsync(OrganizationUser user)
    {
        try
        {
            if (string.IsNullOrEmpty(user.UserId))
            {
                _logger.LogWarning("Cannot update user without UserId (CUID)");
                return false;
            }

            // Update app_users table
            var appUserUpdate = new AppUserRecord
            {
                Cuid = user.UserId,
                Email = user.Email ?? string.Empty,
                Status = user.Status ?? "Pending"
            };

            await _supabaseClient
                .From<AppUserRecord>()
                .Where(u => u.Cuid == user.UserId)
                .Update(appUserUpdate);

            // Update user_infos table
            var userInfoUpdate = new UserInfoRecord
            {
                UserCuid = user.UserId,
                FullName = user.Name,
                Department = user.Department,
                PhoneNumber = user.PhoneNumber
            };

            await _supabaseClient
                .From<UserInfoRecord>()
                .Where(u => u.UserCuid == user.UserId)
                .Update(userInfoUpdate);

            _logger.LogInformation("Successfully updated user: {UserId}", user.UserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user: {UserId}", user.UserId);
            return false;
        }
    }

    public async Task<bool> ApproveUserAsync(string cuid)
    {
        try
        {
            var update = new AppUserRecord
            {
                Cuid = cuid,
                Status = "Active"
            };

            await _supabaseClient
                .From<AppUserRecord>()
                .Where(u => u.Cuid == cuid)
                .Update(update);

            _logger.LogInformation("Successfully approved user: {Cuid}", cuid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve user: {Cuid}", cuid);
            return false;
        }
    }

    public async Task<bool> RejectUserAsync(string cuid, string? reason = null)
    {
        try
        {
            var update = new AppUserRecord
            {
                Cuid = cuid,
                Status = "Rejected"
            };

            await _supabaseClient
                .From<AppUserRecord>()
                .Where(u => u.Cuid == cuid)
                .Update(update);

            // Store rejection reason in user_infos if provided
            if (!string.IsNullOrWhiteSpace(reason))
            {
                // Note: You may need to add a rejection_reason column to user_infos
                // For now, we'll just log it
                _logger.LogInformation("User {Cuid} rejected with reason: {Reason}", cuid, reason);
            }

            _logger.LogInformation("Successfully rejected user: {Cuid}", cuid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reject user: {Cuid}", cuid);
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(string cuid)
    {
        try
        {
            // Soft delete by setting status to Inactive
            var update = new AppUserRecord
            {
                Cuid = cuid,
                Status = "Inactive"
            };

            await _supabaseClient
                .From<AppUserRecord>()
                .Where(u => u.Cuid == cuid)
                .Update(update);

            _logger.LogInformation("Successfully deleted (soft) user: {Cuid}", cuid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user: {Cuid}", cuid);
            return false;
        }
    }

    public async Task<bool> AssignRolesAsync(string cuid, IEnumerable<string> roleCodes)
    {
        try
        {
            foreach (var roleCode in roleCodes)
            {
                var userRole = new UserRoleRecord
                {
                    UserCuid = cuid,
                    RoleCode = roleCode
                };

                await _supabaseClient
                    .From<UserRoleRecord>()
                    .Insert(userRole);
            }

            _logger.LogInformation("Successfully assigned roles to user: {Cuid}", cuid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign roles to user: {Cuid}", cuid);
            return false;
        }
    }

    public async Task<bool> RemoveRolesAsync(string cuid, IEnumerable<string> roleCodes)
    {
        try
        {
            foreach (var roleCode in roleCodes)
            {
                await _supabaseClient
                    .From<UserRoleRecord>()
                    .Where(ur => ur.UserCuid == cuid && ur.RoleCode == roleCode)
                    .Delete();
            }

            _logger.LogInformation("Successfully removed roles from user: {Cuid}", cuid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove roles from user: {Cuid}", cuid);
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetAvailableRolesAsync()
    {
        try
        {
            var response = await _supabaseClient
                .From<RoleDefinitionRecord>()
                .Get();

            if (response?.Models == null)
            {
                return Enumerable.Empty<string>();
            }

            return response.Models.Select(r => r.RoleCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available roles");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetAvailableTeamsAsync()
    {
        try
        {
            var response = await _supabaseClient
                .From<TeamRecord>()
                .Where(t => t.IsActive)
                .Get();

            if (response?.Models == null)
            {
                return Enumerable.Empty<string>();
            }

            return response.Models
                .OrderBy(t => t.Name)
                .Select(t => t.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available teams");
            throw;
        }
    }

    private static OrganizationUser MapToOrganizationUser(UserAccountOverviewRecord record)
    {
        var status = record.Status ?? "Pending";
        var roleCodes = record.RoleCodes ?? Array.Empty<string>();
        
        // Ensure email is properly set - use Email field from record
        var email = !string.IsNullOrWhiteSpace(record.Email) 
            ? record.Email 
            : null;
        
        return new OrganizationUser
        {
            Id = 0, // Not used with CUID-based system
            UserId = record.Cuid,
            Name = record.FullName,
            Email = email,
            Role = roleCodes.Length > 0
                ? string.Join(", ", roleCodes)
                : null,
            Status = status,
            Department = record.Department,
            PhoneNumber = record.PhoneNumber,
            RegisteredAt = record.AccountCreatedAt ?? DateTime.UtcNow,
            ApprovedAt = string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase) 
                ? (record.AccountUpdatedAt ?? DateTime.UtcNow) 
                : null,
            ApprovalMemo = null
        };
    }
}
