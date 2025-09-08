using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace NexaCRM.WebClient.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        public void UpdateAuthenticationState(string? username, string[]? roles)
        {
            ClaimsPrincipal claimsPrincipal;

            if (!string.IsNullOrEmpty(username))
            {
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username)
                }, "FakeAuth");

                if (roles != null && roles.Length > 0)
                {
                    foreach (var role in roles)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }

                claimsPrincipal = new ClaimsPrincipal(identity);
            }
            else
            {
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            }

            _currentUser = claimsPrincipal;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void Logout()
        {
            UpdateAuthenticationState(string.Empty, new string[0]);
        }
    }
}
