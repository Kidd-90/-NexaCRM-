using System.Security.Claims;

namespace NexaCRM.Services.Admin.Interfaces
{
    public interface IRolePermissionService
    {
        /// <summary>
        /// 사용자가 고객 관리 페이지들에 접근할 수 있는지 확인 (Sales, Manager, Admin, Developer 허용)
        /// </summary>
        Task<bool> CanAccessCustomerManagementAsync(ClaimsPrincipal user);

        /// <summary>
        /// 사용자가 신규 고객 등록 기능을 사용할 수 있는지 확인 (Sales 역할 제한, Developer 허용)
        /// </summary>
        Task<bool> CanCreateNewCustomerAsync(ClaimsPrincipal user);

        /// <summary>
        /// 사용자가 고객 정보를 수정할 수 있는지 확인 (Developer 포함)
        /// </summary>
        Task<bool> CanEditCustomerAsync(ClaimsPrincipal user);

        /// <summary>
        /// 사용자가 고객 정보를 조회할 수 있는지 확인 (Developer 포함)
        /// </summary>
        Task<bool> CanViewCustomerAsync(ClaimsPrincipal user);

        /// <summary>
        /// 사용자의 역할을 확인
        /// </summary>
        Task<string[]> GetUserRolesAsync(ClaimsPrincipal user);
    }
}
