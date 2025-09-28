using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NexaCRM.UI.Models.Navigation;

/// <summary>
/// Central navigation schema used by the shell header, sidebar menu and search experiences.
/// </summary>
public static class NavigationCatalog
{
    private static readonly NavigationLinkDefinition[] OverviewLinks =
    {
        new("Home", "bi bi-house", string.Empty, Array.Empty<string>(), Keywords("home", "start", "landing")),
        new("Dashboard", "bi bi-speedometer2", "main-dashboard", Roles("Sales", "Manager", "Admin"), Keywords("overview", "metrics")),
        new("ManagerDashboard", "bi bi-speedometer", "sales-manager-dashboard", Roles("Manager", "Admin"), Keywords("management", "team"))
    };

    private static readonly NavigationLinkDefinition[] SalesWorkspaceLinks =
    {
        new("SalesPipeline", "bi bi-kanban", "sales-pipeline-page", Roles("Sales", "Manager"), Keywords("opportunity", "pipeline")),
        new("Tasks", "bi bi-check2-square", "tasks-page", Roles("Sales", "Manager"), Keywords("todo", "follow up")),
        new("Contacts", "bi bi-person-lines-fill", "contacts", Roles("Sales", "Manager", "Support"), Keywords("people", "customers")),
        new("SalesCalendar", "bi bi-calendar-event", "sales-calendar", Roles("Sales", "Manager"), Keywords("schedule", "calendar"))
    };

    private static readonly NavigationLinkDefinition[] DatabaseLinks =
    {
        new("AllDbList", "bi bi-table", "db/customer/all", Roles("Manager"), Keywords("database", "all")),
        new("NewDbList", "bi bi-plus-circle", "db/customer/new", Roles("Sales"), Keywords("database", "new")),
        new("StarredDbList", "bi bi-star", "db/customer/starred", Roles("Sales"), Keywords("favorite", "starred")),
        new("TodaysAssignedDb", "bi bi-calendar-day", "db/customer/assigned-today", Roles("Manager"), Keywords("assignment", "today")),
        new("UnassignedDbList", "bi bi-x-circle", "db/distribution/unassigned", Roles("Manager"), Keywords("unassigned")),
        new("NewlyAssignedDb", "bi bi-arrow-left-right", "db/distribution/newly-assigned", Roles("Sales"), Keywords("assigned")),
        new("DbDistributionStatus", "bi bi-clipboard-data", "db/distribution/status", Roles("Manager"), Keywords("status")),
        new("MyAssignmentHistory", "bi bi-clock-history", "db/distribution/my-history", Roles("Sales"), Keywords("history")),
        new("TeamDbStatus", "bi bi-people", "db/customer/team-status", Roles("Manager"), Keywords("team", "status")),
        new("DbAdvanced", "bi bi-tools", "db/advanced", Roles("Manager", "Developer"), Keywords("advanced")),
        new("MyDbList", "bi bi-person-lines-fill", "db/customer/my-list", Roles("Manager", "Sales"), Keywords("personal"))
    };

    private static readonly NavigationLinkDefinition[] EngagementLinks =
    {
        new("BulkSms", "bi bi-send", "sms/bulk", Roles("Sales", "Manager"), Keywords("sms", "campaign")),
        new("SmsHistory", "bi bi-clock-history", "sms/history", Roles("Sales", "Manager"), Keywords("sms", "history")),
        new("SmsSchedule", "bi bi-envelope-plus", "schedule/sms", Roles("Sales", "Manager"), Keywords("schedule", "sms")),
        new("ScheduleSentHistory", "bi bi-send-check", "schedule/sent-history", Roles("Sales", "Manager"), Keywords("history", "send")),
        new("Notices", "bi bi-megaphone", "support/notices", Roles("Sales", "Manager", "Admin"), Keywords("support", "notice")),
        new("Faq", "bi bi-question-circle", "support/faq", Roles("Sales", "Manager", "Admin"), Keywords("support", "faq"))
    };

    private static readonly NavigationLinkDefinition[] InsightsLinks =
    {
        new("StatisticsDashboard", "bi bi-graph-up", "statistics/dashboard", Roles("Manager", "Admin", "Developer"), Keywords("statistics", "analytics")),
        new("Reports", "bi bi-file-earmark-bar-graph", "reports-page", Roles("Manager", "Admin", "Developer"), Keywords("reports", "insights"))
    };

    private static readonly NavigationLinkDefinition[] SettingsLinks =
    {
        new("PersonalInfo", "bi bi-person-circle", "profile-settings-page", Roles("Sales", "Manager", "Admin", "Developer"), Keywords("profile", "account")),
        new("CompanyInfo", "bi bi-building", "settings/company-info", Roles("Manager", "Admin", "Developer"), Keywords("company", "settings")),
        new("ThemeSettings", "bi bi-palette", "settings-page", Roles("Manager", "Admin", "Developer"), Keywords("theme", "appearance")),
        new("SecuritySettings", "bi bi-shield-lock", "settings/security", Roles("Admin", "Developer"), Keywords("security")),
        new("SmsSettings", "bi bi-chat-dots", "settings/sms", Roles("Sales", "Manager"), Keywords("sms", "configuration")),
        new("SenderNumbers", "bi bi-telephone", "sms/senders", Roles("Admin", "Developer"), Keywords("sms", "sender")),
        new("TemplateManagement", "bi bi-envelope-open", "email-template-builder", Roles("Manager", "Admin", "Developer"), Keywords("email", "template")),
        new("NotificationSettings", "bi bi-bell", "notification-settings-page", Roles("Manager", "Admin", "Developer", "Sales"), Keywords("notification"))
    };

    private static readonly NavigationLinkDefinition[] OrganizationLinks =
    {
        new("OrganizationStructure", "bi bi-diagram-3", "organization/structure", Roles("Manager", "Admin"), Keywords("organization", "structure")),
        new("TeamManagement", "bi bi-people-fill", "organization/team-management", Roles("Manager", "Admin"), Keywords("team", "management")),
        new("TeamMemberManagement", "bi bi-person-badge", "organization/team-members", Roles("Manager", "Admin"), Keywords("members")),
        new("UserManagement", "bi bi-person-plus", "organization/user-management", Roles("Manager", "Admin"), Keywords("user")),
        new("OrganizationStats", "bi bi-bar-chart", "organization/stats", Roles("Manager", "Admin"), Keywords("organization", "statistics")),
        new("SystemAdmin", "bi bi-person-gear", "organization/system-admin", Roles("Admin", "Developer"), Keywords("admin", "system"))
    };

    private static readonly NavigationLinkDefinition[] SystemLinks =
    {
        new("SystemInformation", "bi bi-hdd-network", "system/info", Roles("Admin", "Developer"), Keywords("system", "info"))
    };

    private static readonly NavigationGroupDefinition[] GroupsInternal =
    {
        new("NavigationOverview", "bi bi-compass", Array.Empty<string>(), OverviewLinks),
        new("SalesWorkspace", "bi bi-briefcase", Roles("Sales", "Manager"), SalesWorkspaceLinks),
        new("DbManagement", "bi bi-database", Roles("Sales", "Manager", "Developer"), DatabaseLinks),
    // Use a distinct chat icon for Engagement so it doesn't conflict with Organization/people icons
    new("Engagement", "bi bi-chat-left-text", Roles("Sales", "Manager", "Admin"), EngagementLinks),
        new("Insights", "bi bi-graph-up", Roles("Manager", "Admin", "Developer"), InsightsLinks),
        new("BasicSettings", "bi bi-sliders", Roles("Sales", "Manager", "Admin", "Developer"), SettingsLinks),
        new("OrganizationManagement", "bi bi-people", Roles("Manager", "Admin", "Developer"), OrganizationLinks),
        new("SystemInfo", "bi bi-cpu", Roles("Admin", "Developer"), SystemLinks)
    };

    private static readonly IReadOnlyList<NavigationGroupDefinition> GroupsReadOnly = new ReadOnlyCollection<NavigationGroupDefinition>(GroupsInternal);
    private static readonly IReadOnlyList<NavigationLinkDefinition> AllLinksReadOnly =
        new ReadOnlyCollection<NavigationLinkDefinition>(GroupsInternal.SelectMany(g => g.Links).ToArray());

    public static IReadOnlyList<NavigationGroupDefinition> Groups => GroupsReadOnly;

    public static IReadOnlyList<NavigationLinkDefinition> AllLinks => AllLinksReadOnly;

    public static NavigationLinkDefinition? FindByUri(string? relativeUri)
    {
        if (string.IsNullOrWhiteSpace(relativeUri))
        {
            return OverviewLinks.FirstOrDefault(l => string.IsNullOrEmpty(l.Href));
        }

        var normalized = Normalize(relativeUri);
        return AllLinksReadOnly.FirstOrDefault(link => Normalize(link.Href) == normalized);
    }

    private static string Normalize(string value) => value.Trim('/').ToLowerInvariant();

    private static string[] Roles(params string[] roles) => roles;

    private static string[] Keywords(params string[] keywords) => keywords;
}

public sealed record NavigationGroupDefinition(
    string ResourceKey,
    string IconCssClass,
    IReadOnlyList<string> RequiredRoles,
    IReadOnlyList<NavigationLinkDefinition> Links);

public sealed record NavigationLinkDefinition(
    string ResourceKey,
    string IconCssClass,
    string Href,
    IReadOnlyList<string> RequiredRoles,
    IReadOnlyList<string> Keywords);
