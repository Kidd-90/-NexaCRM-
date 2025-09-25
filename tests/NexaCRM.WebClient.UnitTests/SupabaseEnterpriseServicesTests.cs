using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NexaCRM.WebClient.Models.Customization;
using NexaCRM.WebClient.Models.FileHub;
using NexaCRM.WebClient.Models.Governance;
using NexaCRM.WebClient.Models.Sync;
using NexaCRM.WebClient.Services;
using NexaCRM.WebClient.Services.SupabaseEnterprise;
using Xunit;

namespace NexaCRM.WebClient.UnitTests;

public sealed class SupabaseEnterpriseServicesTests
{
    [Fact]
    public async Task UserGovernance_CreateUser_AssignsRolesAndLogs()
    {
        // Arrange
        var store = new SupabaseEnterpriseDataStore();
        var provider = CreateClientProvider();
        var service = new SupabaseUserGovernanceService(provider, store, NullLogger<SupabaseUserGovernanceService>.Instance);
        var organizationId = Guid.NewGuid();
        var roles = new[] { "admin", "sales", "Admin" };

        // Act
        var account = await service.CreateUserAsync(organizationId, "user@example.com", "Demo User", roles);
        var auditTrail = await service.GetAuditTrailAsync(organizationId);

        // Assert
        Assert.Equal("Demo User", account.DisplayName);
        Assert.Contains(account.Roles, role => string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(account.Roles, role => string.Equals(role, "sales", StringComparison.OrdinalIgnoreCase));
        Assert.Single(auditTrail.Where(entry => entry.Action == "user.create"));
    }

    [Fact]
    public async Task SettingsCustomization_SavesDashboardInOrder()
    {
        // Arrange
        var store = new SupabaseEnterpriseDataStore();
        var service = new SupabaseSettingsCustomizationService(
            CreateClientProvider(),
            store,
            NullLogger<SupabaseSettingsCustomizationService>.Instance);
        var userId = Guid.NewGuid();

        var widgets = new List<DashboardWidget>
        {
            new() { WidgetId = Guid.NewGuid(), UserId = userId, WidgetType = "kpi" },
            new() { WidgetId = Guid.NewGuid(), UserId = userId, WidgetType = "tasks" },
            new() { WidgetId = Guid.NewGuid(), UserId = userId, WidgetType = "activity" }
        };

        // Act
        await service.SaveDashboardLayoutAsync(userId, widgets);
        var ordered = await service.GetDashboardLayoutAsync(userId);

        // Assert
        Assert.Equal(3, ordered.Count);
        Assert.Equal(new[] { 0, 1, 2 }, ordered.Select(w => w.Order));
    }

    [Fact]
    public async Task FileHub_CreatesDocumentAndThreads()
    {
        // Arrange
        var store = new SupabaseEnterpriseDataStore();
        var service = new SupabaseFileHubService(
            CreateClientProvider(),
            store,
            NullLogger<SupabaseFileHubService>.Instance);
        var organizationId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        // Act
        var document = await service.CreateDocumentAsync(
            organizationId,
            ownerId,
            "Proposal.pdf",
            "sales",
            new Dictionary<string, string> { ["region"] = "APAC" });

        await service.AddVersionAsync(document.DocumentId, ownerId, "/storage/proposal/v1", "hash", 2048);
        var thread = await service.StartThreadAsync(document.DocumentId, "Feedback", new[] { ownerId });
        await service.AppendMessageAsync(thread.ThreadId, ownerId, "internal", "Looks good!");

        var threads = await service.GetThreadsForDocumentAsync(document.DocumentId);
        var messages = await service.GetThreadMessagesAsync(thread.ThreadId);

        // Assert
        Assert.Single(threads);
        Assert.Single(messages);
        Assert.Equal("Looks good!", messages[0].Body);
    }

    [Fact]
    public async Task SyncOrchestration_RegistersEnvelopeAndConflict()
    {
        // Arrange
        var store = new SupabaseEnterpriseDataStore();
        var service = new SupabaseSyncOrchestrationService(
            CreateClientProvider(),
            store,
            NullLogger<SupabaseSyncOrchestrationService>.Instance);
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var items = new[]
        {
            new SyncItem { EnvelopeId = Guid.Empty, EntityType = "contact", EntityId = "42", PayloadJson = "{}" }
        };

        // Act
        var envelope = await service.RegisterEnvelopeAsync(Guid.Empty, organizationId, userId, items);
        await service.RegisterConflictAsync(new SyncConflict
        {
            EnvelopeId = envelope.EnvelopeId,
            EntityType = "contact",
            EntityId = "42",
            ResolutionState = "pending"
        });

        var pending = await service.GetPendingEnvelopesAsync(organizationId);
        var conflicts = await service.GetConflictsAsync(organizationId);

        // Assert
        Assert.Single(pending);
        Assert.Single(conflicts);
        Assert.Equal(envelope.EnvelopeId, conflicts[0].EnvelopeId);
    }

    private static SupabaseClientProvider CreateClientProvider()
    {
        var options = new Supabase.SupabaseOptions
        {
            AutoConnectRealtime = false,
            AutoRefreshToken = false
        };

        var client = new Supabase.Client("https://example.supabase.co", "supabase-test-key", options);
        return new SupabaseClientProvider(client, NullLogger<SupabaseClientProvider>.Instance);
    }
}
