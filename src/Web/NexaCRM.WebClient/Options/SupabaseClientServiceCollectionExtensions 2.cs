using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NexaCRM.WebClient.Options;

public static class SupabaseClientServiceCollectionExtensions
{
    public static IServiceCollection AddSupabaseClientOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<SupabaseClientOptions>()
            .Bind(configuration.GetSection(SupabaseClientOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
