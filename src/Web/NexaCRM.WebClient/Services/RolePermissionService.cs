using System.Security.Claims;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services
{
    public class RolePermissionService : IRolePermissionService
    {
        public Task<bool> CanAccessCustomerManagementAsync(ClaimsPrincipal user)
        {
            // Sales 및 Manager 역할 모두 고객 관리 페이지에 접근 가능
            return Task.FromResult(
                user.Identity?.IsAuthenticated == true && 
                (user.IsInRole("Sales") || user.IsInRole("Manager") || user.IsInRole("Admin"))
            );
        }

        public Task<bool> CanCreateNewCustomerAsync(ClaimsPrincipal user)
        {
            // Manager와 Admin만 신규 고객 등록 가능 (Sales 역할 제한)
            return Task.FromResult(
                user.Identity?.IsAuthenticated == true && 
                (user.IsInRole("Manager") || user.IsInRole("Admin"))
            );
        }

        public Task<bool> CanEditCustomerAsync(ClaimsPrincipal user)
        {
            // Sales, Manager, Admin 모두 고객 정보 수정 가능
            return Task.FromResult(
                user.Identity?.IsAuthenticated == true && 
                (user.IsInRole("Sales") || user.IsInRole("Manager") || user.IsInRole("Admin"))
            );
        }

        public Task<bool> CanViewCustomerAsync(ClaimsPrincipal user)
        {
            // Sales, Manager, Admin 모두 고객 정보 조회 가능
            return Task.FromResult(
                user.Identity?.IsAuthenticated == true && 
                (user.IsInRole("Sales") || user.IsInRole("Manager") || user.IsInRole("Admin"))
            );
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
    }
}