// Program.cs
using System;
using System.Globalization;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NexaCRM.WebClient;
// using NexaCRM.WebClient.Pages; // App.razor은 프로젝트 루트에 있으므로 필요 없음
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using NexaCRM.WebClient.Services;
using NexaCRM.WebClient.Services.Interfaces;
using NexaCRM.WebClient.Services.Mock;
using NexaCRM.WebClient.Models.Db;
using NexaCRM.WebClient.Options;

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
builder.Services.AddSupabaseClientOptions(builder.Configuration);
builder.Services.AddScoped<SupabaseSessionPersistence>();
builder.Services.AddScoped<Supabase.Client>(provider =>
{
    var options = provider.GetRequiredService<IOptions<SupabaseClientOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.Url) || string.IsNullOrWhiteSpace(options.AnonKey))
    {
        throw new InvalidOperationException("Supabase configuration must include Url and AnonKey.");
    }

    var persistence = provider.GetRequiredService<SupabaseSessionPersistence>();

    var supabaseOptions = new Supabase.SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = true,
        SessionHandler = persistence
    };

    return new Supabase.Client(options.Url, options.AnonKey, supabaseOptions);
});
builder.Services.AddScoped<SupabaseClientProvider>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddScoped<IContactService, SupabaseContactService>();
builder.Services.AddScoped<IDealService, SupabaseDealService>();
builder.Services.AddScoped<ITaskService, SupabaseTaskService>();
builder.Services.AddScoped<ISupportTicketService, MockSupportTicketService>();
builder.Services.AddScoped<IAgentService, MockAgentService>();
builder.Services.AddScoped<IMarketingCampaignService, MockMarketingCampaignService>();
builder.Services.AddScoped<IReportService, MockReportService>();
builder.Services.AddScoped<IActivityService, MockActivityService>();
builder.Services.AddScoped<ISalesManagementService, MockSalesManagementService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IDbDataService, MockDbDataService>();
builder.Services.AddScoped<IDuplicateService, DuplicateService>();
builder.Services.AddSingleton<IDedupeConfigService, DedupeConfigService>();
builder.Services.AddScoped<IDuplicateMonitorService, DuplicateMonitorService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IDbAdminService, DbAdminService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<ICustomerCenterService, CustomerCenterService>();
builder.Services.AddScoped<INoticeService, NoticeService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<ISystemInfoService, SystemInfoService>();
builder.Services.AddScoped<IFaqService, FaqService>();
builder.Services.AddScoped<IUserFavoritesService, UserFavoritesService>();
builder.Services.AddScoped<IEmailTemplateService, MockEmailTemplateService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationFeedService, MockNotificationFeedService>();
builder.Services.AddScoped<ITeamService, MockTeamService>();

var culture = new CultureInfo("ko-KR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var host = builder.Build();
// Kick off duplicate monitor
var monitor = host.Services.GetRequiredService<IDuplicateMonitorService>();
await monitor.StartAsync();
await host.RunAsync();
