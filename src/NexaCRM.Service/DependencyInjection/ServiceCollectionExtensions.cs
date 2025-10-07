using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using NexaCRM.Service.InMemory;
using NexaCRM.Service.Supabase;
using NexaCRM.Services.Admin;
using NexaCRM.Services.Admin.Interfaces;

namespace NexaCRM.Service.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNexaCrmAdminServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IDbDataService, SupabaseDbDataService>();
        services.AddScoped<IDbAdminService, DbAdminService>();
        services.AddScoped<IDuplicateService, DuplicateService>();
        services.AddSingleton<IDedupeConfigService, DedupeConfigService>();
        // INotificationFeedService는 Program.cs에서 SupabaseNotificationFeedService로 등록됨
        services.AddScoped<IDuplicateMonitorService, DuplicateMonitorService>();
        services.AddSingleton<ICustomerCenterService, CustomerCenterService>();
        services.AddSingleton<IFaqService, FaqService>();
        services.AddSingleton<INoticeService, NoticeService>();
        services.AddScoped<INotificationService, NotificationService>(); // Scoped: SupabaseClientProvider 사용
        services.AddSingleton<IOrganizationService, OrganizationService>();
        services.AddSingleton<IRolePermissionService, RolePermissionService>();
        services.AddSingleton<ISecurityService, SecurityService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<ISmsService, SmsService>();
        services.AddSingleton<IStatisticsService, StatisticsService>();
        services.AddSingleton<ISystemInfoService, SystemInfoService>();
        services.AddHttpClient<ISupabaseMonitoringService, SupabaseMonitoringService>();
        services.AddSingleton<ISupabaseAuditSyncVerifier, SupabaseAuditSyncVerifier>();

        return services;
    }
}
