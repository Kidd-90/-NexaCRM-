using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace NexaCRM.WebClient.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private const string UsernameStorageKey = "username";
        private const string RolesStorageKey = "roles";
        private const string DeveloperFlagStorageKey = "isDeveloper";
        private const string DeveloperRoleName = "Developer";

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
                var username = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", UsernameStorageKey);
                var rolesJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", RolesStorageKey);
                var developerFlagRaw = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", DeveloperFlagStorageKey);

                if (!string.IsNullOrEmpty(username) && username != "null")
                {
                    var roles = DeserializeRoles(rolesJson);
                    var normalizedRoles = NormalizeRoles(roles, ParseBooleanFlag(developerFlagRaw));

                    if (normalizedRoles.Length > 0)
                    {
                        SetCurrentUser(username, normalizedRoles);
                        return;
                    }
                }

                // 유효하지 않은 데이터가 있는 경우 localStorage 정리
                await ClearStorageAsync();
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
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UsernameStorageKey);
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", RolesStorageKey);
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", DeveloperFlagStorageKey);
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
                var normalizedRoles = NormalizeRoles(roles, ContainsDeveloperRole(roles));
                SetCurrentUser(username, normalizedRoles);

                // Store in localStorage asynchronously
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UsernameStorageKey, username);
                        await _jsRuntime.InvokeVoidAsync(
                            "localStorage.setItem",
                            RolesStorageKey,
                            JsonSerializer.Serialize(normalizedRoles));
                        await _jsRuntime.InvokeVoidAsync(
                            "localStorage.setItem",
                            DeveloperFlagStorageKey,
                            ContainsDeveloperRole(normalizedRoles) ? "true" : "false");

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
                        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UsernameStorageKey);
                        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", RolesStorageKey);
                        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", DeveloperFlagStorageKey);

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

        private void SetCurrentUser(string username, IEnumerable<string> roles)
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

        private static string[] DeserializeRoles(string? rolesJson)
        {
            if (string.IsNullOrWhiteSpace(rolesJson) || rolesJson == "null")
            {
                return Array.Empty<string>();
            }

            try
            {
                return JsonSerializer.Deserialize<string[]>(rolesJson) ?? Array.Empty<string>();
            }
            catch (JsonException)
            {
                return Array.Empty<string>();
            }
            catch (NotSupportedException)
            {
                return Array.Empty<string>();
            }
        }

        private static string[] NormalizeRoles(IEnumerable<string> roles, bool ensureDeveloperRole)
        {
            var normalized = roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (ensureDeveloperRole && !normalized.Any(role => string.Equals(role, DeveloperRoleName, StringComparison.OrdinalIgnoreCase)))
            {
                normalized.Add(DeveloperRoleName);
            }

            return normalized.ToArray();
        }

        private static bool ParseBooleanFlag(string? value)
        {
            return !string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out var result) && result;
        }

        private static bool ContainsDeveloperRole(IEnumerable<string> roles)
        {
            foreach (var role in roles)
            {
                if (string.Equals(role, DeveloperRoleName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
