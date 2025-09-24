using System;
using BuildingBlocks.Common.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Common.Authentication;

/// <summary>
/// Helper extensions that wire Supabase JWT authentication for ASP.NET Core services.
/// </summary>
public static class SupabaseAuthenticationServiceCollectionExtensions
{
    /// <summary>
    /// Registers JWT bearer authentication configured to validate Supabase-issued tokens.
    /// The <paramref name="configuration"/> must expose a <c>Supabase</c> section with the URL and secrets.
    /// </summary>
    /// <param name="services">The dependency injection container.</param>
    /// <param name="configuration">Application configuration used to bind Supabase settings.</param>
    /// <param name="scheme">
    /// Optional authentication scheme name. Defaults to <see cref="JwtBearerDefaults.AuthenticationScheme"/> when omitted.
    /// </param>
    /// <param name="configureOptions">Optional callback to further customize the generated <see cref="JwtBearerOptions"/>.</param>
    public static AuthenticationBuilder AddSupabaseJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string? scheme = null,
        Action<JwtBearerOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        scheme ??= JwtBearerDefaults.AuthenticationScheme;

        var supabaseOptions = SupabaseServerOptions.FromConfiguration(configuration);
        var validationSection = configuration.GetSection($"{SupabaseServerOptions.SectionName}:TokenValidation");
        var validationSettings = validationSection.Get<SupabaseTokenValidationSettings>() ?? new SupabaseTokenValidationSettings();

        services.AddSupabaseServerOptions(configuration);

        services
            .AddOptions<SupabaseTokenValidationSettings>()
            .Bind(validationSection)
            .ValidateOnStart();

        var builder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = scheme;
            options.DefaultChallengeScheme = scheme;
        });

        builder.AddJwtBearer(scheme, options =>
        {
            options.TokenValidationParameters = SupabaseTokenValidationParametersFactory.Create(
                supabaseOptions,
                validationSettings);
            options.RequireHttpsMetadata = true;
            options.SaveToken = true;

            configureOptions?.Invoke(options);
        });

        services.AddSingleton<SupabaseJwtValidator>();

        return builder;
    }
}
