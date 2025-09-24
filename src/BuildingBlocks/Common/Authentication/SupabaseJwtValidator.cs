using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BuildingBlocks.Common.Options;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Common.Authentication;

/// <summary>
/// Validates Supabase JWT tokens using the configured <see cref="SupabaseServerOptions"/> and validation settings.
/// </summary>
public sealed class SupabaseJwtValidator
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly IOptionsMonitor<SupabaseServerOptions> _supabaseOptionsMonitor;
    private readonly IOptionsMonitor<SupabaseTokenValidationSettings>? _validationSettingsMonitor;

    public SupabaseJwtValidator(
        IOptionsMonitor<SupabaseServerOptions> supabaseOptionsMonitor,
        IOptionsMonitor<SupabaseTokenValidationSettings>? validationSettingsMonitor = null)
    {
        _supabaseOptionsMonitor = supabaseOptionsMonitor ?? throw new ArgumentNullException(nameof(supabaseOptionsMonitor));
        _validationSettingsMonitor = validationSettingsMonitor;
    }

    /// <summary>
    /// Validates the supplied JWT string and returns the associated <see cref="ClaimsPrincipal"/> when successful.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the supplied token string is null or whitespace.</exception>
    /// <exception cref="Microsoft.IdentityModel.Tokens.SecurityTokenException">Thrown when validation fails.</exception>
    public ClaimsPrincipal ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Supabase access token must not be null or whitespace.", nameof(token));
        }

        var supabaseOptions = _supabaseOptionsMonitor.CurrentValue;
        var validationSettings = _validationSettingsMonitor?.CurrentValue ?? new SupabaseTokenValidationSettings();

        var parameters = SupabaseTokenValidationParametersFactory.Create(supabaseOptions, validationSettings);

        return _tokenHandler.ValidateToken(token, parameters, out _);
    }
}
