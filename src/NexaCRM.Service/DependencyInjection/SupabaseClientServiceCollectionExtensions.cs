using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexaCRM.Service.Supabase;
using NexaCRM.Service.Supabase.Configuration;

namespace NexaCRM.Service.DependencyInjection;

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

    public static IServiceCollection AddSupabaseClient(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped(provider =>
        {
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("SupabaseClientFactory");
            var options = provider.GetRequiredService<IOptions<SupabaseClientOptions>>().Value;
            var configurators = provider.GetServices<ISupabaseClientConfigurator>();

            var url = string.IsNullOrWhiteSpace(options.Url) ? SupabaseClientDefaults.OfflineUrl : options.Url;
            var anonKey = string.IsNullOrWhiteSpace(options.AnonKey)
                ? SupabaseClientDefaults.OfflineAnonKey
                : options.AnonKey;

            if (string.IsNullOrWhiteSpace(options.Url) || string.IsNullOrWhiteSpace(options.AnonKey))
            {
                logger.LogWarning("Supabase configuration is missing or incomplete. Falling back to offline defaults.");
            }

            var supabaseOptions = new global::Supabase.SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            foreach (var configurator in configurators)
            {
                configurator.ConfigureOptions(supabaseOptions);
            }

            var client = new global::Supabase.Client(url!, anonKey!, supabaseOptions);

            foreach (var configurator in configurators)
            {
                configurator.ConfigureClient(client);
            }

            return client;
        });

        services.AddScoped<SupabaseClientProvider>();

        return services;
    }
}
