using System;
using BuildingBlocks.Common.Options;
using Microsoft.Extensions.Configuration;
using BuildingBlocks.Common.Supabase.Data.Contacts;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Common.Supabase;

public static class SupabaseServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Supabase admin client provider and required dependencies.
    /// </summary>
    public static IServiceCollection AddSupabaseAdminClient(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSupabaseServerOptions(configuration);
        services.AddSingleton<ISupabaseAdminClientFactory, SupabaseAdminClientFactory>();
        services.AddSingleton<SupabaseAdminClientProvider>();
        services.AddSingleton<ISupabaseContactRepository, SupabaseContactRepository>();

        return services;
    }
}
