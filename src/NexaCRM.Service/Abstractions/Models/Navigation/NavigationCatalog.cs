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
    private static readonly NavigationLinkDefinition[] ProjectLinks =
    {
        new("Dashboard", "bi bi-house", "main-dashboard", Roles(), Keywords("overview", "metrics")),
        // Ensure these project-level links are visible to all users by using an explicit empty role list
        new("StatusAlerts", "bi bi-bell", "notifications", Array.Empty<string>(), Keywords("alerts", "notifications")),
        //new("StatusUpdates", "bi bi-arrow-repeat", "notifications/updates", Roles(), Keywords("updates", "notifications")),
        new("StatusAnnouncements", "bi bi-megaphone", "notifications/announcements", Array.Empty<string>(), Keywords("announcements", "notifications")),
        //new("HistoryRecent", "bi bi-clock-history", "history/recent", Array.Empty<string>(), Keywords("history", "recent"))
    };

    private static readonly NavigationLinkDefinition[] SalesWorkspaceLinks =
    {
        new("SalesPipeline", "bi bi-kanban", "sales-pipeline-page", Roles("Admin", "Sales", "Manager"), Keywords("opportunity", "pipeline")),
        new("Tasks", "bi bi-check2-square", "tasks-page", Roles("Admin", "Sales", "Manager"), Keywords("todo", "follow up")),
        new("Contacts", "bi bi-person-lines-fill", "contacts", Roles("Admin", "Sales", "Manager", "Support"), Keywords("people", "customers")),
        new("SalesCalendar", "bi bi-calendar-event", "sales-calendar", Roles("Admin", "Sales", "Manager"), Keywords("schedule", "calendar"))
    };

    private static readonly NavigationLinkDefinition[] DatabaseLinks =
    {
        new("AllDbList", "bi bi-table", "db/customer/all", Roles("Admin", "Manager"), Keywords("database", "all")),
        new("NewDbList", "bi bi-plus-circle", "db/customer/new", Roles("Admin", "Sales"), Keywords("database", "new")),
        new("StarredDbList", "bi bi-star", "db/customer/starred", Roles("Admin", "Sales"), Keywords("favorite", "starred")),
        new("TodaysAssignedDb", "bi bi-calendar-day", "db/customer/assigned-today", Roles("Admin", "Manager"), Keywords("assignment", "today")),
        new("UnassignedDbList", "bi bi-x-circle", "db/distribution/unassigned", Roles("Admin", "Manager"), Keywords("unassigned")),
        new("NewlyAssignedDb", "bi bi-arrow-left-right", "db/distribution/newly-assigned", Roles("Admin", "Sales"), Keywords("assigned")),
        new("DbDistributionStatus", "bi bi-clipboard-data", "db/distribution/status", Roles("Admin", "Manager"), Keywords("status")),
        new("MyAssignmentHistory", "bi bi-clock-history", "db/distribution/my-history", Roles("Admin", "Sales"), Keywords("history")),
        new("TeamDbStatus", "bi bi-people", "db/customer/team-status", Roles("Admin", "Manager"), Keywords("team", "status")),
        new("DbAdvanced", "bi bi-tools", "db/advanced", Roles("Admin", "Manager", "Developer"), Keywords("advanced")),
        new("MyDbList", "bi bi-person-lines-fill", "db/customer/my-list", Roles("Admin", "Manager", "Sales"), Keywords("personal"))
    };

    private static readonly NavigationLinkDefinition[] EngagementLinks =
    {
        new("BulkSms", "bi bi-send", "sms/bulk", Roles("Admin", "Sales", "Manager"), Keywords("sms", "campaign")),
        new("SmsHistory", "bi bi-clock-history", "sms/history", Roles("Admin", "Sales", "Manager"), Keywords("sms", "history")),
        new("SmsSchedule", "bi bi-envelope-plus", "schedule/sms", Roles("Admin", "Sales", "Manager"), Keywords("schedule", "sms")),
        new("ScheduleSentHistory", "bi bi-send-check", "schedule/sent-history", Roles("Admin", "Sales", "Manager"), Keywords("history", "send")),
        new("Notices", "bi bi-megaphone", "notifications/announcements", Roles("Admin", "Sales", "Manager"), Keywords("support", "notice")),
        new("Faq", "bi bi-question-circle", "support/faq", Roles("Admin", "Sales", "Manager"), Keywords("support", "faq"))
    };

    private static readonly NavigationLinkDefinition[] InsightsLinks =
    {
        new("StatisticsDashboard", "bi bi-graph-up", "statistics/dashboard", Roles("Manager", "Admin", "Developer"), Keywords("statistics", "analytics")),
        //new("Reports", "bi bi-file-earmark-bar-graph", "reports-page", Roles("Manager", "Admin", "Developer"), Keywords("reports", "insights"))
    };

    private static readonly NavigationLinkDefinition[] SettingsLinks =
    {
        new("PersonalInfo", "bi bi-person-circle", "profile-settings-page", Roles("Admin", "Sales", "Manager", "Developer"), Keywords("profile", "account")),
        new("CompanyInfo", "bi bi-building", "settings/company-info", Roles("Admin", "Manager", "Developer"), Keywords("company", "settings")),
        new("ThemeSettings", "bi bi-palette", "settings-page", Roles("Admin", "Manager", "Developer"), Keywords("theme", "appearance")),
        new("SecuritySettings", "bi bi-shield-lock", "settings/security", Roles("Admin", "Developer"), Keywords("security")),
        //new("SmsSettings", "bi bi-chat-dots", "settings/sms", Roles("Admin", "Sales", "Manager"), Keywords("sms", "configuration")),
        new("SenderNumbers", "bi bi-telephone", "sms/senders", Roles("Admin", "Developer"), Keywords("sms", "sender")),
        new("TemplateManagement", "bi bi-envelope-open", "email-template-builder", Roles("Admin", "Manager", "Developer"), Keywords("email", "template")),
        new("NotificationSettings", "bi bi-bell", "notification-settings-page", Roles("Admin", "Manager", "Developer", "Sales"), Keywords("notification"))
    };

    private static readonly NavigationLinkDefinition[] OrganizationLinks =
    {
        new("OrganizationStructure", "bi bi-diagram-3-fill", "organization/structure", Roles("Manager", "Admin"), Keywords("organization", "structure")),
        new("BizManagement", "bi bi-building-fill-gear", "organization/biz-management", Roles("Manager", "Admin"), Keywords("business", "franchise", "company", "branch")),
        new("TeamManagement", "bi bi-people-fill", "organization/team-management", Roles("Manager", "Admin"), Keywords("team", "management")),
        new("TeamMemberManagement", "bi bi-person-vcard-fill", "organization/team-members", Roles("Manager", "Admin"), Keywords("members")),
        new("UserManagement", "bi bi-person-fill-add", "organization/user-management", Roles("Manager", "Admin"), Keywords("user")),
        new("OrganizationStats", "bi bi-bar-chart-fill", "organization/stats", Roles("Manager", "Admin"), Keywords("organization", "statistics")),
        new("SystemAdmin", "bi bi-people-fill", "organization/system-admin", Roles("Manager", "Admin"), Keywords("admin", "employee", "staff"))
    };

    private static readonly NavigationLinkDefinition[] SystemLinks =
    {
        new("SystemInformation", "bi bi-hdd-network", "system/info", Roles("Admin", "Developer"), Keywords("system", "info"))
    };

    // Define the full set of navigation groups
    private static readonly NavigationGroupDefinition[] GroupsInternal =
    {
        new("NavigationProjects", "bi bi-kanban", Array.Empty<string>(), ProjectLinks),
        // Add an explicit Notifications group (appears under main navigation/dashboard)
        new("SalesWorkspace", "bi bi-briefcase", Roles("Admin", "Sales", "Manager"), SalesWorkspaceLinks),
        new("DbManagement", "bi bi-people", Roles("Admin", "Sales", "Manager", "Developer"), DatabaseLinks),
        // Use a distinct chat icon for Engagement so it doesn't conflict with Organization/people icons
        new("Engagement", "bi bi-chat-left-text", Roles("Admin", "Sales", "Manager"), EngagementLinks),
        new("Insights", "bi bi-graph-up", Roles("Admin", "Manager", "Developer"), InsightsLinks),
        new("BasicSettings", "bi bi-sliders", Roles("Admin", "Sales", "Manager", "Developer"), SettingsLinks),
        new("OrganizationManagement", "bi bi-building-fill", Roles("Admin", "Manager", "Developer"), OrganizationLinks),
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
            return ProjectLinks.FirstOrDefault();
        }

        var normalized = Normalize(relativeUri);
        return AllLinksReadOnly.FirstOrDefault(link => Normalize(link.Href) == normalized);
    }

    // Normalize a relative URI for matching against defined Href values.
    // - Removes query string and fragment (anything after '?' or '#')
    // - Trims leading/trailing slashes
    // - Converts to lower-case invariant
    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        // Strip query string and fragment
        var idxQuery = value.IndexOf('?');
        var idxFrag = value.IndexOf('#');
        var endIdx = value.Length;
        if (idxQuery >= 0 && idxFrag >= 0)
        {
            endIdx = Math.Min(idxQuery, idxFrag);
        }
        else if (idxQuery >= 0)
        {
            endIdx = idxQuery;
        }
        else if (idxFrag >= 0)
        {
            endIdx = idxFrag;
        }

        var core = value.Substring(0, endIdx);
        return core.Trim('/').ToLowerInvariant();
    }

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
