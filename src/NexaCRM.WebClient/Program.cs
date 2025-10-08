// Program.cs
using System;
using System.Globalization;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Db;
using NexaCRM.Services.Admin.Services;
using NexaCRM.WebClient;
// using NexaCRM.WebClient.Pages; // App.razor은 프로젝트 루트에 있으므로 필요 없음
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexaCRM.Services.Admin;
using NexaCRM.Service.DependencyInjection;
using NexaCRM.Service.Supabase;
using NexaCRM.Service.Supabase.Configuration;
using NexaCRM.Service.Supabase.Enterprise;
using NexaCRM.UI.Services;
using NexaCRM.UI.Services.Interfaces;
using NexaCRM.WebClient.Supabase.Environment;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add root components to the app
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient setup: BaseAddress set to host environment address
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }
);

builder.Services.AddAuthorizationCore();
builder.Services.AddLocalization(options => { options.ResourcesPath = "Resources"; });
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddNexaCrmAdminServices();
builder.Services.AddSupabaseClientOptions(builder.Configuration, validateOnStart: false);
builder.Services.AddScoped<SupabaseSessionPersistence>();
builder.Services.AddScoped<ISupabaseClientConfigurator, WebAssemblySupabaseClientConfigurator>();
builder.Services.AddSupabaseClient();
builder.Services.AddScoped<ClientSupabaseAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<ClientSupabaseAuthenticationStateProvider>());
builder.Services.AddScoped<IAuthenticationService>(provider => provider.GetRequiredService<ClientSupabaseAuthenticationStateProvider>());
builder.Services.AddScoped<ActionInterop>();
builder.Services.AddScoped<IMobileInteractionService, MobileInteractionService>();
builder.Services.AddScoped<IGlobalActionService, GlobalActionService>();
builder.Services.AddScoped<IContactService, SupabaseContactService>();
builder.Services.AddScoped<IDealService, SupabaseDealService>();
builder.Services.AddScoped<ITaskService, SupabaseTaskService>();
builder.Services.AddScoped<ISupportTicketService, SupabaseSupportTicketService>();
builder.Services.AddScoped<IAgentService, SupabaseAgentService>();
builder.Services.AddScoped<IMarketingCampaignService, SupabaseMarketingCampaignService>();
builder.Services.AddScoped<IReportService, SupabaseReportService>();
builder.Services.AddScoped<IActivityService, SupabaseActivityService>();
builder.Services.AddScoped<ISalesManagementService, SupabaseSalesManagementService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IDuplicateService, DuplicateService>();
builder.Services.AddSingleton<IDedupeConfigService, DedupeConfigService>();
builder.Services.AddScoped<IDuplicateMonitorService, DuplicateMonitorService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IUserManagementService, SupabaseUserManagementService>();
builder.Services.AddScoped<IDbAdminService, DbAdminService>();
builder.Services.AddScoped<IStatisticsService, SupabaseStatisticsService>();
builder.Services.AddScoped<ICustomerCenterService, CustomerCenterService>();
builder.Services.AddScoped<INoticeService, SupabaseNoticeService>();
builder.Services.AddScoped<ISmsService, SupabaseSmsService>();
builder.Services.AddScoped<ISystemInfoService, SystemInfoService>();
builder.Services.AddScoped<IFaqService, FaqService>();
builder.Services.AddScoped<IUserFavoritesService, UserFavoritesService>();
builder.Services.AddScoped<IEmailTemplateService, SupabaseEmailTemplateService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationFeedService, SupabaseNotificationFeedService>();
builder.Services.AddScoped<ITeamService, SupabaseTeamService>();
builder.Services.AddScoped<INavigationStateService, NavigationStateService>();
builder.Services.AddSingleton<SupabaseEnterpriseDataStore>();
builder.Services.AddScoped<IUserGovernanceService, SupabaseUserGovernanceService>();
builder.Services.AddScoped<ISettingsCustomizationService, SupabaseSettingsCustomizationService>();
builder.Services.AddScoped<IFileHubService, SupabaseFileHubService>();
builder.Services.AddScoped<ICommunicationHubService, SupabaseCommunicationHubService>();
builder.Services.AddScoped<ISyncOrchestrationService, SupabaseSyncOrchestrationService>();

var culture = new CultureInfo("ko-KR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var host = builder.Build();

await StartDuplicateMonitorAsync(host.Services);
await host.RunAsync();

static async Task StartDuplicateMonitorAsync(IServiceProvider services)
{
    try
    {
        var monitor = services.GetRequiredService<IDuplicateMonitorService>();
        await monitor.StartAsync();
    }
    catch (Exception ex)
    {
        var loggerFactory = services.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("Startup");
        logger?.LogError(ex, "Failed to start the duplicate monitor service.");
    }
}
