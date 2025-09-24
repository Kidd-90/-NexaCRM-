using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BuildingBlocks.Common.Authentication;
using BuildingBlocks.Common.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace BuildingBlocks.Common.UnitTests;

public class SupabaseJwtValidatorTests
{
    private const string SupabaseUrl = "https://example.supabase.co";
    private const string ServiceRoleKey = "service-role-key";
    private const string JwtSecret = "secret-value-used-for-tests";

    private static SupabaseServerOptions CreateOptions() => new()
    {
        Url = SupabaseUrl,
        ServiceRoleKey = ServiceRoleKey,
        JwtSecret = JwtSecret
    };

    [Fact]
    public void ValidateToken_ReturnsPrincipal_WhenTokenIsValid()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<SupabaseServerOptions>().Configure(options =>
        {
            options.Url = SupabaseUrl;
            options.ServiceRoleKey = ServiceRoleKey;
            options.JwtSecret = JwtSecret;
        });
        services.AddOptions<SupabaseTokenValidationSettings>();

        var provider = services.BuildServiceProvider();
        var validator = new SupabaseJwtValidator(
            provider.GetRequiredService<IOptionsMonitor<SupabaseServerOptions>>(),
            provider.GetRequiredService<IOptionsMonitor<SupabaseTokenValidationSettings>>());

        var token = CreateJwtToken();

        // Act
        var principal = validator.ValidateToken(token);

        // Assert
        Assert.True(principal.Identity?.IsAuthenticated);
        Assert.Equal("123", principal.FindFirstValue(JwtRegisteredClaimNames.Sub));
        Assert.Equal("demo@example.com", principal.FindFirstValue(JwtRegisteredClaimNames.Email));
    }

    [Fact]
    public void ValidateToken_Throws_WhenTokenInvalid()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<SupabaseServerOptions>().Configure(options =>
        {
            options.Url = SupabaseUrl;
            options.ServiceRoleKey = ServiceRoleKey;
            options.JwtSecret = JwtSecret;
        });

        var provider = services.BuildServiceProvider();
        var validator = new SupabaseJwtValidator(
            provider.GetRequiredService<IOptionsMonitor<SupabaseServerOptions>>());

        var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken());

        // Act & Assert
        Assert.Throws<SecurityTokenException>(() => validator.ValidateToken(token));
    }

    [Fact]
    public void CreateParameters_UsesSupabaseConventions()
    {
        var options = CreateOptions();
        var validation = new SupabaseTokenValidationSettings
        {
            ValidateAudience = true,
            ValidAudiences = new List<string> { "authenticated", "service_role" },
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        var parameters = SupabaseTokenValidationParametersFactory.Create(options, validation);

        Assert.True(parameters.ValidateIssuerSigningKey);
        Assert.Equal("https://example.supabase.co/auth/v1", parameters.ValidIssuer);
        Assert.True(parameters.ValidateAudience);
        Assert.Contains("service_role", parameters.ValidAudiences!);
        Assert.Equal(TimeSpan.FromMinutes(1), parameters.ClockSkew);
    }

    [Fact]
    public void AddSupabaseJwtAuthentication_ConfiguresJwtBearerOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{SupabaseServerOptions.SectionName}:Url"] = SupabaseUrl,
                [$"{SupabaseServerOptions.SectionName}:ServiceRoleKey"] = ServiceRoleKey,
                [$"{SupabaseServerOptions.SectionName}:JwtSecret"] = JwtSecret
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSupabaseJwtAuthentication(configuration);

        var provider = services.BuildServiceProvider();
        var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
        var options = optionsMonitor.Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.Equal("https://example.supabase.co/auth/v1", options.TokenValidationParameters.ValidIssuer);
        Assert.True(options.TokenValidationParameters.ValidateIssuerSigningKey);

        var validator = provider.GetRequiredService<SupabaseJwtValidator>();
        var token = CreateJwtToken();

        var principal = validator.ValidateToken(token);

        Assert.Equal("123", principal.FindFirstValue(JwtRegisteredClaimNames.Sub));
    }

    private static string CreateJwtToken()
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "123"),
            new(JwtRegisteredClaimNames.Email, "demo@example.com")
        };

        var token = new JwtSecurityToken(
            issuer: "https://example.supabase.co/auth/v1",
            audience: "authenticated",
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
