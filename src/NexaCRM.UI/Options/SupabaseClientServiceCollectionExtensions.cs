using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NexaCRM.UI.Options;

public static class SupabaseClientServiceCollectionExtensions
{
    public static IServiceCollection AddSupabaseClientOptions(
        this IServiceCollection services,
        IConfiguration configuration,
        bool validateOnStart = true)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var builder = services
            .AddOptions<SupabaseClientOptions>()
            .Bind(configuration.GetSection(SupabaseClientOptions.SectionName))
            .ValidateDataAnnotations();

        if (validateOnStart)
        {
            builder.ValidateOnStart();
        }
        else
        {
            services.PostConfigure<SupabaseClientOptions>(options =>
            {
                if (!string.IsNullOrWhiteSpace(options.Url) && !string.IsNullOrWhiteSpace(options.AnonKey))
                {
                    return;
                }

                options.Url ??= SupabaseClientDefaults.OfflineUrl;
                options.AnonKey ??= SupabaseClientDefaults.OfflineAnonKey;
            });
        }

        return services;
    }
}
