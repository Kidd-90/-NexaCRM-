using System.Collections.Generic;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Models.Organization;

namespace NexaCRM.Services.Admin.Interfaces;

/// <summary>
/// Service for managing users in the system
/// </summary>
public interface IUserManagementService
{
    /// <summary>
    /// Get all users with their profiles and roles
    /// </summary>
    Task<IEnumerable<OrganizationUser>> GetAllUsersAsync();

    /// <summary>
    /// Get user by CUID
    /// </summary>
    Task<OrganizationUser?> GetUserByCuidAsync(string cuid);

    /// <summary>
    /// Update user information
    /// </summary>
    Task<bool> UpdateUserAsync(OrganizationUser user);

    /// <summary>
    /// Approve pending user
    /// </summary>
    Task<bool> ApproveUserAsync(string cuid);

    /// <summary>
    /// Reject pending user with optional reason
    /// </summary>
    Task<bool> RejectUserAsync(string cuid, string? reason = null);

    /// <summary>
    /// Delete user (soft delete by setting status to Inactive)
    /// </summary>
    Task<bool> DeleteUserAsync(string cuid);

    /// <summary>
    /// Assign roles to user
    /// </summary>
    Task<bool> AssignRolesAsync(string cuid, IEnumerable<string> roleCodes);

    /// <summary>
    /// Remove roles from user
    /// </summary>
    Task<bool> RemoveRolesAsync(string cuid, IEnumerable<string> roleCodes);

    /// <summary>
    /// Get all available roles
    /// </summary>
    Task<IEnumerable<string>> GetAvailableRolesAsync();

    /// <summary>
    /// Get all available teams/departments
    /// </summary>
    Task<IEnumerable<string>> GetAvailableTeamsAsync();
}
