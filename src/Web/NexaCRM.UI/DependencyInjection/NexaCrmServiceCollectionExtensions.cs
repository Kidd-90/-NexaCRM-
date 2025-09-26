using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Db;
using NexaCRM.WebClient.Options;
using NexaCRM.WebClient.Services;
using NexaCRM.WebClient.Services.Admin;
using NexaCRM.WebClient.Services.Interfaces;
using NexaCRM.WebClient.Services.Mock;
using NexaCRM.WebClient.Services.SupabaseEnterprise;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace NexaCRM.WebClient.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring NexaCRM services for different runtime environments.
/// </summary>
public static class NexaCrmServiceCollectionExtensions
{
    /// <summary>
    /// Registers services that are shared between all NexaCRM runtime environments.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddNexaCrmCoreServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddAuthorizationCore();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddCascadingAuthenticationState();
        services.AddSupabaseClientOptions(configuration);

        services.AddScoped<CustomAuthStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthStateProvider>());
        services.AddScoped<ActionInterop>();
        services.AddScoped<IMobileInteractionService, MobileInteractionService>();
        services.AddScoped<IGlobalActionService, GlobalActionService>();
        services.AddScoped<IContactService, SupabaseContactService>();
        services.AddScoped<IDealService, SupabaseDealService>();
        services.AddScoped<ITaskService, SupabaseTaskService>();
        services.AddScoped<ISupportTicketService, SupabaseSupportTicketService>();
        services.AddScoped<IAgentService, SupabaseAgentService>();
        services.AddScoped<IMarketingCampaignService, SupabaseMarketingCampaignService>();
        services.AddScoped<IReportService, SupabaseReportService>();
        services.AddScoped<IActivityService, SupabaseActivityService>();
        services.AddScoped<ISalesManagementService, SupabaseSalesManagementService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();
        services.AddScoped<IDbDataService, MockDbDataService>();
        services.AddScoped<IDuplicateService, DuplicateService>();
        services.AddSingleton<IDedupeConfigService, DedupeConfigService>();
        services.AddScoped<IDuplicateMonitorService, DuplicateMonitorService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<ISecurityService, SecurityService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IDbAdminService, DbAdminService>();
        services.AddScoped<IStatisticsService, SupabaseStatisticsService>();
        services.AddScoped<ICustomerCenterService, CustomerCenterService>();
        services.AddScoped<INoticeService, NoticeService>();
        services.AddScoped<ISmsService, SupabaseSmsService>();
        services.AddScoped<ISystemInfoService, SystemInfoService>();
        services.AddScoped<IFaqService, FaqService>();
        services.AddScoped<IUserFavoritesService, UserFavoritesService>();
        services.AddScoped<IEmailTemplateService, SupabaseEmailTemplateService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationFeedService, SupabaseNotificationFeedService>();
        services.AddScoped<ITeamService, SupabaseTeamService>();
        services.AddScoped<INavigationStateService, NavigationStateService>();
        services.AddSingleton<SupabaseEnterpriseDataStore>();
        services.AddScoped<IUserGovernanceService, SupabaseUserGovernanceService>();
        services.AddScoped<ISettingsCustomizationService, SupabaseSettingsCustomizationService>();
        services.AddScoped<IFileHubService, SupabaseFileHubService>();
        services.AddScoped<ICommunicationHubService, SupabaseCommunicationHubService>();
        services.AddScoped<ISyncOrchestrationService, SupabaseSyncOrchestrationService>();
        services.AddScoped<SupabaseClientProvider>();

        return services;
    }

    /// <summary>
    /// Registers the NexaCRM services required when running as a Blazor WebAssembly application.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="builder">The WebAssembly host builder.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddNexaCrmWebAssemblyRuntime(
        this IServiceCollection services,
        WebAssemblyHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(builder);

        services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        services.AddScoped<SupabaseSessionPersistence>();
        services.AddScoped<IGotrueSessionPersistence<Session>>(provider => provider.GetRequiredService<SupabaseSessionPersistence>());
        services.AddScoped(provider => CreateSupabaseClient(provider));

        return services;

        static Supabase.Client CreateSupabaseClient(IServiceProvider provider)
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
                AutoConnectRealtime = false,
                SessionHandler = persistence
            };

            return new Supabase.Client(options.Url, options.AnonKey, supabaseOptions);
        }
    }

    /// <summary>
    /// Registers the NexaCRM services required when running on the server.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">A delegate used to describe the server runtime configuration.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddNexaCrmServerRuntime(
        this IServiceCollection services,
        Action<NexaCrmServerRuntimeOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new NexaCrmServerRuntimeOptions();
        configure(options);
        options.Validate();

        services.AddScoped(provider => options.HttpClientFactory!(provider));
        services.AddScoped(provider => options.SessionPersistenceFactory!(provider));
        services.AddScoped(provider => options.SupabaseClientFactory!(provider));

        options.ConfigureServices?.Invoke(services);

        return services;
    }
}

/// <summary>
/// Describes the dependencies that must be supplied when NexaCRM runs in a server environment.
/// </summary>
public sealed class NexaCrmServerRuntimeOptions
{
    /// <summary>
    /// Gets or sets the factory used to create the <see cref="HttpClient"/> instance for server-side rendering.
    /// </summary>
    public Func<IServiceProvider, HttpClient>? HttpClientFactory { get; set; }

    /// <summary>
    /// Gets or sets the factory used to create the Supabase session persistence implementation.
    /// </summary>
    public Func<IServiceProvider, IGotrueSessionPersistence<Session>>? SessionPersistenceFactory { get; set; }

    /// <summary>
    /// Gets or sets the factory used to create the Supabase client for the server runtime.
    /// </summary>
    public Func<IServiceProvider, Supabase.Client>? SupabaseClientFactory { get; set; }

    /// <summary>
    /// Gets or sets an optional callback for additional server specific registrations.
    /// </summary>
    public Action<IServiceCollection>? ConfigureServices { get; set; }

    internal void Validate()
    {
        if (HttpClientFactory is null)
        {
            throw new InvalidOperationException("HttpClientFactory must be provided for the server runtime.");
        }

        if (SessionPersistenceFactory is null)
        {
            throw new InvalidOperationException("SessionPersistenceFactory must be provided for the server runtime.");
        }

        if (SupabaseClientFactory is null)
        {
            throw new InvalidOperationException("SupabaseClientFactory must be provided for the server runtime.");
        }
    }
}
