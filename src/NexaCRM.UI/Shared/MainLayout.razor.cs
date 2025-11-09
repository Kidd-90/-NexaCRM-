using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.UI.Shared;

public sealed partial class MainLayout : LayoutComponentBase, IDisposable
{
    private static readonly string[] DetailRoutePrefixes =
    {
        "contacts/",
        "db/distribution/assign/",
        "email-template-builder/"
    };

    private static readonly Dictionary<string, string> DetailParentRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["contacts/"] = "/contacts",
        ["db/distribution/assign/"] = "/db/distribution/status",
        ["email-template-builder/"] = "/email-template-builder"
    };

    private static readonly Dictionary<string, string> PageTitleOverrides = new(StringComparer.OrdinalIgnoreCase)
    {
        [""] = "Home",
        ["main-dashboard"] = "Home",
        ["sales-manager-dashboard"] = "Manager Dashboard",
        ["statistics/dashboard"] = "Statistics",
        ["statistics"] = "Statistics",
        ["reports-page"] = "Reports",
        ["sales-pipeline-page"] = "Sales Pipeline",
        ["contacts"] = "Contacts",
        ["tasks-page"] = "Tasks",
        ["settings"] = "Settings",
        ["settings-page"] = "Theme Settings",
        ["profile-settings-page"] = "Profile",
        ["settings/company-info"] = "Company Info",
        ["settings/security"] = "Security",
        ["settings/sms"] = "SMS Settings",
        ["sms/senders"] = "Sender Numbers",
        ["notification-settings-page"] = "Notifications",
        ["organization/structure"] = "Organization Structure",
        ["organization/biz-management"] = "Business Management",
        ["organization/stats"] = "Organization Stats",
        ["organization/system-admin"] = "System Admin",
        ["db/customer/all"] = "All Customers",
        ["db/customer/new"] = "New Customers",
        ["db/customer/starred"] = "Starred Customers",
        ["db/customer/assigned-today"] = "Today's Assignments",
        ["db/distribution/unassigned"] = "Unassigned DB",
        ["db/distribution/newly-assigned"] = "Newly Assigned",
        ["db/distribution/status"] = "Distribution Status",
        ["db/distribution/my-history"] = "Assignment History",
        ["db/customer/team-status"] = "Team DB Status",
        ["db/advanced"] = "Advanced DB",
        ["db/customer/my-list"] = "My DB List",
        ["customer-support-dashboard"] = "Support Dashboard",
        ["customer-support-ticket-management-interface"] = "Support Tickets",
        ["customer-support-knowledge-base"] = "Support Knowledge Base",
        ["marketing-campaign-management-interface"] = "Marketing Campaigns",
        ["system/info"] = "System Info",
        ["email-template-builder"] = "Email Templates"
    };

    private static readonly Dictionary<string, string> HeaderIconOverrides = new(StringComparer.OrdinalIgnoreCase)
    {
        ["contacts"] = "bi bi-person-lines-fill",
        ["reports-page"] = "bi bi-bar-chart-line",
        ["statistics"] = "bi bi-graph-up",
        ["statistics/dashboard"] = "bi bi-graph-up",
        ["sales-pipeline-page"] = "bi bi-kanban",
        ["tasks-page"] = "bi bi-check2-square",
        ["settings"] = "bi bi-gear-wide-connected",
        ["settings-page"] = "bi bi-brightness-high",
        ["profile-settings-page"] = "bi bi-person-circle",
        ["organization"] = "bi bi-diagram-3",
        ["marketing-campaign-management-interface"] = "bi bi-bullseye",
        ["customer-support-dashboard"] = "bi bi-headset",
        ["customer-support-ticket-management-interface"] = "bi bi-card-checklist",
        ["system"] = "bi bi-hdd-network",
        ["sms"] = "bi bi-chat-dots",
        ["db"] = "bi bi-database",
        ["sales-manager-dashboard"] = "bi bi-speedometer2",
        ["email-template-builder"] = "bi bi-envelope-open"
    };

    private static readonly MobileNavigationItem[] MobileNavigationBlueprint =
    {
        new("홈", "bi bi-house-door-fill", "/"),
        new("DB", "bi bi-database", "/db/customer/all"),
        new("할 일", "bi bi-check2-square", "/tasks-page"),
        new("설정", "bi bi-gear-fill", "/settings")
    };

    private bool isDetailPage;
    private string currentPageTitle = "Home";
    private string headerActionIcon = "bi bi-grid-3x3-gap";
    private int unreadNotificationsCount;
    private string userDisplayName = "Guest";
    private string userInitials = "G";
    private string? parentPagePath;
    private string? parentPageTitle;
    private DevicePlatform currentDevicePlatform = DevicePlatform.Desktop;
    private bool devicePlatformInitialized;
    private bool isUserAuthenticated;
    private IJSObjectReference? layoutModule;
    private IReadOnlyList<MobileNavigationLink> mobileNavigationLinks = Array.Empty<MobileNavigationLink>();

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
        NotificationFeed.UnreadCountChanged += OnUnreadChanged;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            UpdateUserInfo(authState.User);
        }
        catch
        {
            UpdateUserInfo(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        UpdateLayoutState();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        try
        {
            layoutModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "/_content/NexaCRM.UI/js/layout.js");
            await layoutModule.InvokeVoidAsync("initializeShell");
            await UpdateUnreadNotificationsAsync();
            await layoutModule.InvokeVoidAsync("refreshThemeToggle");
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.Error?.WriteLine($"Failed to import layout module: {ex.Message}");

            try
            {
                await JSRuntime.InvokeVoidAsync("initializeShell");
                await UpdateUnreadNotificationsAsync();
                await JSRuntime.InvokeVoidAsync("refreshThemeToggle");
                StateHasChanged();
            }
            catch (Exception inner)
            {
                Console.Error?.WriteLine($"Fallback initializeShell failed: {inner.Message}");
            }
        }

        await InitializeDevicePlatformAsync();
    }

    private async Task InitializeDevicePlatformAsync()
    {
        if (devicePlatformInitialized)
        {
            return;
        }

        var resolvedPlatform = DevicePlatform.Desktop;

        try
        {
            resolvedPlatform = await DeviceService.GetPlatformAsync();
        }
        catch (JSException)
        {
            resolvedPlatform = DevicePlatform.Desktop;
        }
        catch (InvalidOperationException)
        {
            resolvedPlatform = DevicePlatform.Desktop;
        }

        currentDevicePlatform = resolvedPlatform;
        devicePlatformInitialized = true;
        StateHasChanged();
    }

    private bool ShouldRenderMobileShell => devicePlatformInitialized && currentDevicePlatform != DevicePlatform.Desktop && isUserAuthenticated;

    private bool ShouldDecorateAsMobileChrome => devicePlatformInitialized && currentDevicePlatform != DevicePlatform.Desktop;

    private string GetDevicePlatformToken()
    {
        if (!devicePlatformInitialized)
        {
            return "desktop";
        }

        return currentDevicePlatform.ToString().ToLowerInvariant();
    }

    private void NavigateBack()
    {
        if (!string.IsNullOrWhiteSpace(parentPagePath))
        {
            NavigationManager.NavigateTo(parentPagePath);
            return;
        }

        _ = JSRuntime.InvokeVoidAsync("history.back");
    }

    private void NavigateToMobile(string? targetUri)
    {
        var destination = string.IsNullOrWhiteSpace(targetUri) ? NavigationManager.BaseUri : targetUri;
        NavigationManager.NavigateTo(destination);
    }

    private Task HandleSearchNavigate() => Task.CompletedTask;

    private void UpdateLayoutState()
    {
        var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        var normalized = NormalizePath(relativePath);

        isDetailPage = false;
        parentPagePath = null;
        parentPageTitle = null;

        foreach (var prefix in DetailRoutePrefixes)
        {
            if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && normalized.Length > prefix.Length)
            {
                isDetailPage = true;
                if (DetailParentRoutes.TryGetValue(prefix, out var explicitParent))
                {
                    parentPagePath = explicitParent;
                }
                break;
            }
        }

        if (isDetailPage)
        {
            var basePath = normalized;
            if (!string.IsNullOrEmpty(parentPagePath))
            {
                basePath = NormalizePath(parentPagePath);
            }
            else
            {
                var lastSeparator = normalized.LastIndexOf('/');
                if (lastSeparator > 0)
                {
                    basePath = normalized[..lastSeparator];
                    parentPagePath = "/" + basePath;
                }
            }

            parentPageTitle = ResolvePageTitle(basePath, false);
        }

        currentPageTitle = ResolvePageTitle(normalized, isDetailPage);
        headerActionIcon = ResolveHeaderActionIcon(normalized);
        mobileNavigationLinks = BuildMobileNavigation(normalized);
    }

    private IReadOnlyList<MobileNavigationLink> BuildMobileNavigation(string normalizedPath)
    {
        return MobileNavigationBlueprint
            .Select(item => new MobileNavigationLink(item.Label, item.Icon, item.TargetUri, IsActiveMobileNav(item.TargetUri, normalizedPath)))
            .ToArray();
    }

    private static bool IsActiveMobileNav(string? targetUri, string currentPath)
    {
        var normalizedCurrent = NormalizePath(currentPath);

        if (string.IsNullOrWhiteSpace(targetUri))
        {
            return string.IsNullOrEmpty(normalizedCurrent);
        }

        var normalizedTarget = NormalizePath(targetUri);
        if (string.IsNullOrEmpty(normalizedTarget))
        {
            return string.IsNullOrEmpty(normalizedCurrent);
        }

        if (string.Equals(normalizedCurrent, normalizedTarget, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return normalizedCurrent.StartsWith(normalizedTarget + "/", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        path = path.Trim('/');
        return path.Replace("\\", "/", StringComparison.OrdinalIgnoreCase);
    }

    private string ResolvePageTitle(string normalizedPath, bool isDetail)
    {
        if (PageTitleOverrides.TryGetValue(normalizedPath, out var title))
        {
            return title;
        }

        if (isDetail)
        {
            var segments = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 0)
            {
                return ToTitleCase(segments.Last());
            }
        }

        return ToTitleCase(normalizedPath);
    }

    private string ResolveHeaderActionIcon(string normalizedPath)
    {
        if (HeaderIconOverrides.TryGetValue(normalizedPath, out var icon))
        {
            return icon;
        }

        if (string.IsNullOrEmpty(normalizedPath))
        {
            return "bi bi-grid-3x3-gap";
        }

        var segments = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in new[] { normalizedPath, segments.First(), segments.Last() })
        {
            if (HeaderIconOverrides.TryGetValue(segment, out icon))
            {
                return icon;
            }
        }

        return "bi bi-grid-3x3-gap";
    }

    private static string ToTitleCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Home";
        }

        var lowerInvariant = value.ToLower(CultureInfo.CurrentCulture);
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lowerInvariant);
    }

    private async Task UpdateUnreadNotificationsAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                unreadNotificationsCount = Math.Max(0, await NotificationFeed.GetUnreadCountAsync());
            }
            catch (Exception ex)
            {
                Console.Error?.WriteLine($"Failed to update unread notifications: {ex.Message}");
                unreadNotificationsCount = 0;
            }
        }
        else
        {
            unreadNotificationsCount = 0;
        }
    }

    private void OnNotificationsClick() => NavigationManager.NavigateTo("/notifications");

    private async void OnUnreadChanged(int count)
    {
        try
        {
            unreadNotificationsCount = Math.Max(0, count);
            await InvokeAsync(StateHasChanged);
            if (ShouldRenderMobileShell)
            {
                mobileNavigationLinks = BuildMobileNavigation(NormalizePath(NavigationManager.ToBaseRelativePath(NavigationManager.Uri)));
            }
        }
        catch
        {
        }
    }

    private void UpdateUserInfo(ClaimsPrincipal user)
    {
        isUserAuthenticated = user?.Identity?.IsAuthenticated == true;

        if (isUserAuthenticated)
        {
            userDisplayName = string.IsNullOrWhiteSpace(user.Identity?.Name) ? "User" : user.Identity!.Name!;

            userInitials = string.Join(string.Empty, userDisplayName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(segment => segment.Length > 0)
                .Take(2)
                .Select(segment => char.ToUpperInvariant(segment[0])));

            if (string.IsNullOrWhiteSpace(userInitials))
            {
                var firstChar = userDisplayName.FirstOrDefault();
                userInitials = firstChar == default ? "U" : char.ToUpperInvariant(firstChar).ToString();
            }
        }
        else
        {
            userDisplayName = "Guest";
            userInitials = "G";
        }
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        try
        {
            UpdateLayoutState();
            await UpdateUnreadNotificationsAsync();
            await InvokeAsync(StateHasChanged);
            if (layoutModule is not null)
            {
                await layoutModule.InvokeVoidAsync("refreshThemeToggle");
            }
        }
        catch (Exception ex)
        {
            Console.Error?.WriteLine($"Failed to update layout on navigation: {ex.Message}");
        }
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        NotificationFeed.UnreadCountChanged -= OnUnreadChanged;
    }

    private sealed record MobileNavigationItem(string Label, string Icon, string TargetUri);

    internal sealed record MobileNavigationLink(string Label, string Icon, string TargetUri, bool IsActive);
}
