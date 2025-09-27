using Microsoft.AspNetCore.Components.Authorization;
using NexaCRM.Service.DependencyInjection;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.UI.Services;
using NexaCRM.UI.Services.Interfaces;
using NexaCRM.UI.Services.Mock;
using NexaCRM.WebServer.Services;
using NexaCRM.WebServer;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseStaticWebAssets();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddAuthorizationCore();

builder.Services.AddNexaCrmAdminServices();

builder.Services.AddScoped<ActionInterop>();
builder.Services.AddScoped<IMobileInteractionService, MobileInteractionService>();
builder.Services.AddScoped<IGlobalActionService, GlobalActionService>();
builder.Services.AddScoped<INavigationStateService, NavigationStateService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IUserFavoritesService, UserFavoritesService>();
builder.Services.AddSingleton<ISyncOrchestrationService, MockSyncOrchestrationService>();
builder.Services.AddSingleton<ICommunicationHubService, MockCommunicationHubService>();
builder.Services.AddSingleton<IFileHubService, MockFileHubService>();
builder.Services.AddSingleton<ISettingsCustomizationService, MockSettingsCustomizationService>();
builder.Services.AddSingleton<IUserGovernanceService, MockUserGovernanceService>();

builder.Services.AddScoped<IContactService, MockContactService>();
builder.Services.AddScoped<IDealService, MockDealService>();
builder.Services.AddScoped<ITaskService, MockTaskService>();
builder.Services.AddScoped<ISupportTicketService, MockSupportTicketService>();
builder.Services.AddScoped<IAgentService, MockAgentService>();
builder.Services.AddScoped<IMarketingCampaignService, MockMarketingCampaignService>();
builder.Services.AddScoped<IReportService, MockReportService>();
builder.Services.AddScoped<IActivityService, MockActivityService>();
builder.Services.AddScoped<ISalesManagementService, MockSalesManagementService>();
builder.Services.AddScoped<IEmailTemplateService, MockEmailTemplateService>();
builder.Services.AddScoped<ITeamService, MockTeamService>();

builder.Services.AddScoped<ServerAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<ServerAuthenticationStateProvider>());
builder.Services.AddScoped<IAuthenticationService>(sp => sp.GetRequiredService<ServerAuthenticationStateProvider>());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(NexaCRM.UI.Shared.MainLayout).Assembly);

await StartDuplicateMonitorAsync(app.Services);

app.Run();

static async Task StartDuplicateMonitorAsync(IServiceProvider services)
{
    await using var scope = services.CreateAsyncScope();
    var monitor = scope.ServiceProvider.GetRequiredService<IDuplicateMonitorService>();
    await monitor.StartAsync();
}
