using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexaCRM.Service.DependencyInjection;
using NexaCRM.Service.Supabase;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.UI.Options;
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

        services.AddSupabaseClientOptions(Configuration, validateOnStart: false);
        services.AddScoped<SupabaseServerSessionPersistence>();
        services.AddScoped<Supabase.Client>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("SupabaseClientSetup");
            var options = provider.GetRequiredService<IOptions<SupabaseClientOptions>>().Value;
            var supabaseUrl = options.Url;
            var supabaseAnonKey = options.AnonKey;
            var isOfflineMode = string.IsNullOrWhiteSpace(supabaseUrl) || string.IsNullOrWhiteSpace(supabaseAnonKey);

            if (!isOfflineMode)
            {
                isOfflineMode =
                    string.Equals(supabaseUrl, SupabaseClientDefaults.OfflineUrl, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(supabaseAnonKey, SupabaseClientDefaults.OfflineAnonKey, StringComparison.Ordinal);
            }

            if (isOfflineMode)
            {
                logger.LogWarning("Supabase configuration is missing or incomplete. NexaCRM.WebServer will run in offline mode.");
                supabaseUrl = SupabaseClientDefaults.OfflineUrl;
                supabaseAnonKey = SupabaseClientDefaults.OfflineAnonKey;
            }

            var persistence = provider.GetRequiredService<SupabaseServerSessionPersistence>();

            var supabaseOptions = new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false,
                SessionHandler = persistence
            };

            return new Supabase.Client(supabaseUrl, supabaseAnonKey, supabaseOptions);
        });
        services.AddScoped<SupabaseClientProvider>();
        services.AddScoped<SupabaseAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<SupabaseAuthenticationStateProvider>());
        services.AddScoped<IAuthenticationService>(sp => sp.GetRequiredService<SupabaseAuthenticationStateProvider>());

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

    }

    public void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env,
        IServiceProvider services,
        IHostApplicationLifetime lifetime,
        ILogger<Startup> logger)
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

        var schemeProvider = services.GetService<IAuthenticationSchemeProvider>();
        if (schemeProvider is not null)
        {
            var schemes = schemeProvider.GetAllSchemesAsync().GetAwaiter().GetResult();
            if (schemes.Any())
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }
            else
            {
                logger.LogInformation("No authentication schemes registered. Skipping UseAuthentication and UseAuthorization middleware.");
            }
        }

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });

        lifetime.ApplicationStarted.Register(() =>
        {
            _ = StartDuplicateMonitorAsync(services, logger);
        });
    }

    private static async Task StartDuplicateMonitorAsync(IServiceProvider services, ILogger logger)
    {
        try
        {
            await using var scope = services.CreateAsyncScope();
            var monitor = scope.ServiceProvider.GetRequiredService<IDuplicateMonitorService>();
            await monitor.StartAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start the duplicate monitor service.");
        }
    }
}
