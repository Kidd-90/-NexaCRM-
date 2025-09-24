using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NexaCRM.WebClient.Options;

namespace NexaCRM.WebClient.UnitTests;

public class SupabaseClientOptionsTests
{
    [Fact]
    public void AddSupabaseClientOptions_BindsConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{SupabaseClientOptions.SectionName}:Url"] = "https://tenant.supabase.co",
                [$"{SupabaseClientOptions.SectionName}:AnonKey"] = "anon-key"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSupabaseClientOptions(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<SupabaseClientOptions>>();

        Assert.Equal("https://tenant.supabase.co", options.Value.Url);
        Assert.Equal("anon-key", options.Value.AnonKey);
    }

    [Fact]
    public void AddSupabaseClientOptions_ThrowsWhenMissingValues()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{SupabaseClientOptions.SectionName}:Url"] = "https://tenant.supabase.co"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSupabaseClientOptions(configuration);

        using var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<SupabaseClientOptions>>().Value);
    }
}
