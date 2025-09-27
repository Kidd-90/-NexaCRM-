using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexaCRM.Service.DependencyInjection;
using NexaCRM.Service.Supabase;
using NexaCRM.Service.Supabase.Configuration;
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

        services.AddSupabaseClientOptions(Configuration, validateOnStart: false);
        services.AddScoped<SupabaseServerSessionPersistence>();
        services.AddScoped<ISupabaseClientConfigurator, SupabaseServerClientConfigurator>();
        services.AddSupabaseClient();
        services.AddScoped<SupabaseAuthenticationStateProvider>(provider =>
        {
            var clientProvider = provider.GetRequiredService<SupabaseClientProvider>();
            var logger = provider.GetRequiredService<ILogger<SupabaseAuthenticationStateProvider>>();
            var options = provider.GetRequiredService<IOptions<SupabaseClientOptions>>();
            return new SupabaseAuthenticationStateProvider(clientProvider, logger, options);
        });
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<SupabaseAuthenticationStateProvider>());
        services.AddScoped<NexaCRM.UI.Services.Interfaces.IAuthenticationService>(sp => sp.GetRequiredService<SupabaseAuthenticationStateProvider>());

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
        var showDetailedErrors = Configuration.GetValue<bool>("Hosting:ShowDetailedErrors");

        if (!env.IsDevelopment())
        {
            if (showDetailedErrors)
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = "text/html; charset=utf-8";

                        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                        var pathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

                        if (exceptionFeature?.Error is not null)
                        {
                            logger.LogError(exceptionFeature.Error, "Unhandled exception while processing request for {Path}", pathFeature?.Path);

                            var builder = new StringBuilder()
                                .Append("<html><body style=\"font-family:monospace;padding:24px;\">")
                                .Append("<h1>Unhandled exception</h1>")
                                .Append("<p><strong>Path:</strong> ")
                                .Append(WebUtility.HtmlEncode(pathFeature?.Path ?? "(unknown)"))
                                .Append("</p><pre>")
                                .Append(WebUtility.HtmlEncode(exceptionFeature.Error.ToString()))
                                .Append("</pre></body></html>");

                            await context.Response.WriteAsync(builder.ToString()).ConfigureAwait(false);
                        }
                        else
                        {
                            await context.Response.WriteAsync("<html><body><h1>An unknown error occurred.</h1></body></html>").ConfigureAwait(false);
                        }
                    });
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        var forceHttps = Configuration.GetValue<bool>("Hosting:ForceHttps");
        if (!env.IsDevelopment() || forceHttps)
        {
            app.UseHttpsRedirection();
        }
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

        var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
        lifetime.ApplicationStarted.Register(() =>
        {
            _ = StartDuplicateMonitorAsync(scopeFactory, logger);
        });
    }

    private static async Task StartDuplicateMonitorAsync(IServiceScopeFactory scopeFactory, ILogger logger)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var monitor = scope.ServiceProvider.GetRequiredService<IDuplicateMonitorService>();
            await monitor.StartAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start the duplicate monitor service.");
        }
    }
}
