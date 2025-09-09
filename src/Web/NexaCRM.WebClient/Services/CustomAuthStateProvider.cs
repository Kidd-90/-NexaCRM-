using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace NexaCRM.WebClient.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        private bool _initialized = false;

        public CustomAuthStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!_initialized)
            {
                await LoadAuthenticationStateFromStorage();
                _initialized = true;
            }
            return new AuthenticationState(_currentUser);
        }

        private async Task LoadAuthenticationStateFromStorage()
        {
            try
            {
                var username = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "username");
                var rolesJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "roles");

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(rolesJson) && 
                    username != "null" && rolesJson != "null")
                {
                    var roles = System.Text.Json.JsonSerializer.Deserialize<string[]>(rolesJson);
                    if (roles != null && roles.Length > 0)
                    {
                        SetCurrentUser(username, roles);
                    }
                    else
                    {
                        // 역할이 없거나 유효하지 않은 경우 localStorage 정리
                        await ClearStorageAsync();
                    }
                }
                else
                {
                    // 유효하지 않은 데이터가 있는 경우 localStorage 정리
                    await ClearStorageAsync();
                }
            }
            catch
            {
                // JavaScript interop might not be available during prerendering
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                try
                {
                    await ClearStorageAsync();
                }
                catch
                {
                    // Silently handle errors during cleanup
                }
            }
        }

        private async Task ClearStorageAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "username");
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "roles");
            }
            catch
            {
                // Handle JavaScript interop errors silently
            }
        }

        public void UpdateAuthenticationState(string? username, string[]? roles)
        {
            if (!string.IsNullOrEmpty(username) && roles != null && roles.Length > 0)
            {
                SetCurrentUser(username, roles);
                
                // Store in localStorage asynchronously
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "username", username);
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "roles", System.Text.Json.JsonSerializer.Serialize(roles));
                        
                        // 세션 타임아웃 리셋 (authManager가 로드된 경우)
                        await _jsRuntime.InvokeVoidAsync("eval", @"
                            if (window.authManager && window.authManager.resetSessionTimeout) {
                                window.authManager.resetSessionTimeout();
                            }
                        ");
                    }
                    catch
                    {
                        // Handle JavaScript interop errors silently
                    }
                });
            }
            else
            {
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                
                // Clear localStorage asynchronously
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "username");
                        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "roles");
                        
                        // 세션 타임아웃 클리어 (authManager가 로드된 경우)
                        await _jsRuntime.InvokeVoidAsync("eval", @"
                            if (window.authManager && window.authManager.sessionTimeoutId) {
                                clearTimeout(window.authManager.sessionTimeoutId);
                                window.authManager.sessionTimeoutId = null;
                            }
                        ");
                    }
                    catch
                    {
                        // Handle JavaScript interop errors silently
                    }
                });
            }

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        private void SetCurrentUser(string username, string[] roles)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username)
            }, "FakeAuth");

            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            _currentUser = new ClaimsPrincipal(identity);
        }

        public void Logout()
        {
            UpdateAuthenticationState(null, null);
        }

        public bool IsAuthenticated => _currentUser.Identity?.IsAuthenticated ?? false;
    }
}
