using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using BuildingBlocks.Common.Options;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Common.Authentication;

/// <summary>
/// Helper responsible for building <see cref="TokenValidationParameters"/> instances that align with Supabase defaults.
/// </summary>
public static class SupabaseTokenValidationParametersFactory
{
    /// <summary>
    /// Creates the token validation parameters using the provided Supabase configuration and optional validation settings.
    /// </summary>
    public static TokenValidationParameters Create(
        SupabaseServerOptions options,
        SupabaseTokenValidationSettings? validationSettings = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        validationSettings ??= new SupabaseTokenValidationSettings();

        var issuer = BuildIssuer(options.Url!);
        var secretBytes = Encoding.UTF8.GetBytes(options.JwtSecret!);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretBytes),
            ValidateIssuer = validationSettings.ValidateIssuer,
            ValidIssuer = issuer,
            ValidateAudience = validationSettings.ValidateAudience,
            RequireAudience = validationSettings.ValidateAudience,
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = validationSettings.ClockSkew,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ClaimTypes.Role
        };

        if (validationSettings.ValidateAudience)
        {
            var audiences = validationSettings.ValidAudiences;

            if (audiences == null || !audiences.Any())
            {
                audiences = new[] { "authenticated" };
            }

            parameters.ValidAudiences = audiences;
        }

        return parameters;
    }

    private static string BuildIssuer(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("Supabase URL cannot be null or empty.", nameof(baseUrl));
        }

        return $"{baseUrl.TrimEnd('/')}/auth/v1";
    }
}
