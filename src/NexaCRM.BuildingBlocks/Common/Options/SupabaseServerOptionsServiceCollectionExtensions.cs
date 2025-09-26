using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Common.Options;

public static class SupabaseServerOptionsServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="SupabaseServerOptions"/> with the dependency injection container
    /// and validates that the required configuration values are present at startup.
    /// </summary>
    public static IServiceCollection AddSupabaseServerOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<SupabaseServerOptions>()
            .Bind(configuration.GetSection(SupabaseServerOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
