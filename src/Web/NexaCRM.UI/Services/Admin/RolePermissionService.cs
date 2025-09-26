using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;

namespace NexaCRM.WebClient.Services;

public sealed class RolePermissionService : IRolePermissionService
{
    private const string DeveloperRoleName = "Developer";

    private static readonly string[] CustomerManagementRoles =
    {
        "Sales",
        "Manager",
        "Admin",
        DeveloperRoleName
    };

    private static readonly string[] CreateCustomerRoles =
    {
        "Manager",
        "Admin",
        DeveloperRoleName
    };

    private static readonly string[] EditCustomerRoles =
    {
        "Sales",
        "Manager",
        "Admin",
        DeveloperRoleName
    };

    private static readonly string[] ViewCustomerRoles =
    {
        "Sales",
        "Manager",
        "Admin",
        DeveloperRoleName
    };

    public Task<bool> CanAccessCustomerManagementAsync(ClaimsPrincipal user)
    {
        return Task.FromResult(HasRequiredRole(user, CustomerManagementRoles));
    }

    public Task<bool> CanCreateNewCustomerAsync(ClaimsPrincipal user)
    {
        return Task.FromResult(HasRequiredRole(user, CreateCustomerRoles));
    }

    public Task<bool> CanEditCustomerAsync(ClaimsPrincipal user)
    {
        return Task.FromResult(HasRequiredRole(user, EditCustomerRoles));
    }

    public Task<bool> CanViewCustomerAsync(ClaimsPrincipal user)
    {
        return Task.FromResult(HasRequiredRole(user, ViewCustomerRoles));
    }

    public Task<string[]> GetUserRolesAsync(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult(Array.Empty<string>());
        }

        var roles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray();

        return Task.FromResult(roles);
    }

    private static bool HasRequiredRole(ClaimsPrincipal user, IEnumerable<string> allowedRoles)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        foreach (var role in allowedRoles)
        {
            if (user.IsInRole(role))
            {
                return true;
            }
        }

        return false;
    }
}
