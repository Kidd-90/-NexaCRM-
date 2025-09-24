using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Common.Supabase;

/// <summary>
/// Helper extensions that register shared Supabase infrastructure for NexaCRM services.
/// </summary>
public static class SupabaseServiceCollectionExtensions
{
    /// <summary>
    /// Binds <see cref="SupabaseSettings"/> from the default <c>"Supabase"</c> section and wires up the <see cref="ISupabaseClientFactory"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>An <see cref="OptionsBuilder{SupabaseSettings}"/> that can be further customised.</returns>
    public static OptionsBuilder<SupabaseSettings> AddSupabaseCore(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        return services.AddSupabaseCore(configuration, SupabaseSettings.SectionName);
    }

    /// <summary>
    /// Binds <see cref="SupabaseSettings"/> from a custom section name and wires up the <see cref="ISupabaseClientFactory"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="sectionName">The configuration section that contains the Supabase settings.</param>
    /// <returns>An <see cref="OptionsBuilder{SupabaseSettings}"/> that can be further customised.</returns>
    public static OptionsBuilder<SupabaseSettings> AddSupabaseCore(this IServiceCollection services, IConfiguration configuration, string sectionName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

        var builder = services
            .AddOptions<SupabaseSettings>()
            .Bind(configuration.GetSection(sectionName))
            .PostConfigure(TrimSettings);

        builder = ApplyValidation(builder);

        RegisterFactory(services);
        return builder;
    }

    /// <summary>
    /// Registers Supabase infrastructure using an inline configuration delegate.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Delegate that will populate <see cref="SupabaseSettings"/>.</param>
    /// <returns>An <see cref="OptionsBuilder{SupabaseSettings}"/> that can be further customised.</returns>
    public static OptionsBuilder<SupabaseSettings> AddSupabaseCore(this IServiceCollection services, Action<SupabaseSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = services
            .AddOptions<SupabaseSettings>()
            .Configure(configure)
            .PostConfigure(TrimSettings);

        builder = ApplyValidation(builder);

        RegisterFactory(services);
        return builder;
    }

    private static void RegisterFactory(IServiceCollection services)
    {
        services.TryAddSingleton<ISupabaseClientFactory, SupabaseClientFactory>();
    }

    private static OptionsBuilder<SupabaseSettings> ApplyValidation(OptionsBuilder<SupabaseSettings> builder)
    {
        return builder
            .ValidateDataAnnotations()
            .Validate(
                settings => settings.Database is not null && !string.IsNullOrWhiteSpace(settings.Database.ConnectionString),
                "Supabase database connection string must be configured.")
            .Validate(
                settings => settings.Client is not null,
                "Supabase client options must be configured.")
            .ValidateOnStart();
    }

    private static void TrimSettings(SupabaseSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        settings.Url = settings.Url?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(settings.ServiceRoleKey))
        {
            settings.ServiceRoleKey = settings.ServiceRoleKey.Trim();
        }

        if (!string.IsNullOrWhiteSpace(settings.AnonKey))
        {
            settings.AnonKey = settings.AnonKey.Trim();
        }

        if (settings.Database is null)
        {
            settings.Database = new SupabaseDatabaseSettings();
        }
        else if (!string.IsNullOrWhiteSpace(settings.Database.ConnectionString))
        {
            settings.Database.ConnectionString = settings.Database.ConnectionString.Trim();
        }

        settings.Client ??= new SupabaseClientOptions();
    }
}
