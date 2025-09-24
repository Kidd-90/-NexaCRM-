using BuildingBlocks.Common.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Common.UnitTests;

public class SupabaseServerOptionsTests
{
    [Fact]
    public void FromConfiguration_BindsAndValidatesValues()
    {
        var settings = new Dictionary<string, string?>
        {
            [$"{SupabaseServerOptions.SectionName}:Url"] = "https://example.supabase.co",
            [$"{SupabaseServerOptions.SectionName}:ServiceRoleKey"] = "service-role-key",
            [$"{SupabaseServerOptions.SectionName}:JwtSecret"] = "jwt-secret",
            [$"{SupabaseServerOptions.SectionName}:AnonKey"] = "anon-key",
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var options = SupabaseServerOptions.FromConfiguration(configuration);

        Assert.Equal("https://example.supabase.co", options.Url);
        Assert.Equal("service-role-key", options.ServiceRoleKey);
        Assert.Equal("jwt-secret", options.JwtSecret);
        Assert.Equal("anon-key", options.AnonKey);
    }

    [Fact]
    public void AddSupabaseServerOptions_RegistersValidatedOptions()
    {
        var settings = new Dictionary<string, string?>
        {
            [$"{SupabaseServerOptions.SectionName}:Url"] = "https://tenant.supabase.co",
            [$"{SupabaseServerOptions.SectionName}:ServiceRoleKey"] = "role-key",
            [$"{SupabaseServerOptions.SectionName}:JwtSecret"] = "secret",
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddSupabaseServerOptions(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<SupabaseServerOptions>>();

        Assert.Equal("https://tenant.supabase.co", options.Value.Url);
    }

    [Fact]
    public void AddSupabaseServerOptions_ThrowsWhenConfigurationMissing()
    {
        var configuration = new ConfigurationBuilder().Build();

        var services = new ServiceCollection();
        services.AddSupabaseServerOptions(configuration);

        using var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<SupabaseServerOptions>>().Value);
    }
}
