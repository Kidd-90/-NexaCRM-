using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NexaCRM.Service.DependencyInjection;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.UI.Services;
using NexaCRM.UI.Services.Interfaces;
using NexaCRM.UI.Services.Mock;
using NexaCRM.WebServer.Services;

namespace NexaCRM.WebServer;

public sealed class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddAuthorizationCore();

        services.AddRazorPages();
        services.AddServerSideBlazor();

        services.AddNexaCrmAdminServices();

        services.AddScoped<ActionInterop>();
        services.AddScoped<IMobileInteractionService, MobileInteractionService>();
        services.AddScoped<IGlobalActionService, GlobalActionService>();
        services.AddScoped<INavigationStateService, NavigationStateService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IUserFavoritesService, UserFavoritesService>();
        services.AddSingleton<ISyncOrchestrationService, MockSyncOrchestrationService>();
        services.AddSingleton<ICommunicationHubService, MockCommunicationHubService>();
        services.AddSingleton<IFileHubService, MockFileHubService>();
        services.AddSingleton<ISettingsCustomizationService, MockSettingsCustomizationService>();
        services.AddSingleton<IUserGovernanceService, MockUserGovernanceService>();

        services.AddScoped<IContactService, MockContactService>();
        services.AddScoped<IDealService, MockDealService>();
        services.AddScoped<ITaskService, MockTaskService>();
        services.AddScoped<ISupportTicketService, MockSupportTicketService>();
        services.AddScoped<IAgentService, MockAgentService>();
        services.AddScoped<IMarketingCampaignService, MockMarketingCampaignService>();
        services.AddScoped<IReportService, MockReportService>();
        services.AddScoped<IActivityService, MockActivityService>();
        services.AddScoped<ISalesManagementService, MockSalesManagementService>();
        services.AddScoped<IEmailTemplateService, MockEmailTemplateService>();
        services.AddScoped<ITeamService, MockTeamService>();

        services.AddScoped<ServerAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<ServerAuthenticationStateProvider>());
        services.AddScoped<IAuthenticationService>(sp => sp.GetRequiredService<ServerAuthenticationStateProvider>());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services, IHostApplicationLifetime lifetime)
    {
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAntiforgery();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });

        lifetime.ApplicationStarted.Register(() =>
        {
            _ = StartDuplicateMonitorAsync(services);
        });
    }

    private static async Task StartDuplicateMonitorAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var monitor = scope.ServiceProvider.GetRequiredService<IDuplicateMonitorService>();
        await monitor.StartAsync();
    }
}
