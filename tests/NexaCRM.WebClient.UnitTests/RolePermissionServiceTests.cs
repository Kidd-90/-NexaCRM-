using System.Security.Claims;
using System.Threading.Tasks;
using NexaCRM.WebClient.Services;
using Xunit;

namespace NexaCRM.WebClient.UnitTests
{
    public class RolePermissionServiceTests
    {
        private readonly RolePermissionService _service = new();

        [Fact]
        public async Task CanAccessCustomerManagementAsync_DeveloperRole_ReturnsTrue()
        {
            var user = CreateAuthenticatedUser("Developer");

            var result = await _service.CanAccessCustomerManagementAsync(user);

            Assert.True(result);
        }

        [Fact]
        public async Task CanCreateNewCustomerAsync_DeveloperRole_ReturnsTrue()
        {
            var user = CreateAuthenticatedUser("Developer");

            var result = await _service.CanCreateNewCustomerAsync(user);

            Assert.True(result);
        }

        [Fact]
        public async Task CanEditCustomerAsync_DeveloperRole_ReturnsTrue()
        {
            var user = CreateAuthenticatedUser("Developer");

            var result = await _service.CanEditCustomerAsync(user);

            Assert.True(result);
        }

        [Fact]
        public async Task CanViewCustomerAsync_DeveloperRole_ReturnsTrue()
        {
            var user = CreateAuthenticatedUser("Developer");

            var result = await _service.CanViewCustomerAsync(user);

            Assert.True(result);
        }

        [Fact]
        public async Task GetUserRolesAsync_IncludesDeveloperRole()
        {
            var user = CreateAuthenticatedUser("Developer", "Sales");

            var roles = await _service.GetUserRolesAsync(user);

            Assert.Contains("Developer", roles);
            Assert.Contains("Sales", roles);
        }

        [Fact]
        public async Task DeveloperPermissions_RequireAuthentication()
        {
            var unauthenticatedUser = new ClaimsPrincipal(new ClaimsIdentity());

            Assert.False(await _service.CanAccessCustomerManagementAsync(unauthenticatedUser));
            Assert.False(await _service.CanCreateNewCustomerAsync(unauthenticatedUser));
            Assert.False(await _service.CanEditCustomerAsync(unauthenticatedUser));
            Assert.False(await _service.CanViewCustomerAsync(unauthenticatedUser));
        }

        private static ClaimsPrincipal CreateAuthenticatedUser(params string[] roles)
        {
            var identity = new ClaimsIdentity("TestAuth");
            identity.AddClaim(new Claim(ClaimTypes.Name, "test-user"));

            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            return new ClaimsPrincipal(identity);
        }
    }
}
