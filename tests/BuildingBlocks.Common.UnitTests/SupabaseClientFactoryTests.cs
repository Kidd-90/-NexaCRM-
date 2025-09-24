using System.Reflection;
using BuildingBlocks.Common.Supabase;
using Microsoft.Extensions.DependencyInjection;
using Supabase;

namespace BuildingBlocks.Common.UnitTests;

public class SupabaseClientFactoryTests
{
    private static ServiceProvider BuildProvider(Action<SupabaseSettings> configure)
    {
        var services = new ServiceCollection();
        services.AddSupabaseCore(configure);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void GetServiceClient_ReturnsCachedClientWithConfiguredKey()
    {
        using var provider = BuildProvider(options =>
        {
            options.Url = "https://unit-test.supabase.co";
            options.ServiceRoleKey = "service-key";
        });

        var factory = provider.GetRequiredService<ISupabaseClientFactory>();

        var first = factory.GetServiceClient();
        var second = factory.GetServiceClient();

        Assert.Same(first, second);
        Assert.Equal("service-key", GetPrivateField<string>(first, "_supabaseKey"));
        Assert.Equal("https://unit-test.supabase.co", GetPrivateField<string>(first, "_supabaseUrl"));

        var options = GetPrivateField<global::Supabase.SupabaseOptions>(first, "_options");
        Assert.False(options.AutoConnectRealtime);
        Assert.True(options.AutoRefreshToken);
    }

    [Fact]
    public void GetServiceClient_ThrowsWhenServiceKeyMissing()
    {
        using var provider = BuildProvider(options =>
        {
            options.Url = "https://unit-test.supabase.co";
        });

        var factory = provider.GetRequiredService<ISupabaseClientFactory>();

        Assert.Throws<InvalidOperationException>(() => factory.GetServiceClient());
    }

    [Fact]
    public void GetAnonClient_ThrowsWhenAnonKeyMissing()
    {
        using var provider = BuildProvider(options =>
        {
            options.Url = "https://unit-test.supabase.co";
        });

        var factory = provider.GetRequiredService<ISupabaseClientFactory>();

        Assert.Throws<InvalidOperationException>(() => factory.GetAnonClient());
    }

    [Fact]
    public void CreateClient_UsesCustomOptionsAndDoesNotCache()
    {
        using var provider = BuildProvider(options =>
        {
            options.Url = "https://unit-test.supabase.co";
            options.AnonKey = "anon-key";
            options.Client.AutoConnectRealtime = true;
        });

        var factory = provider.GetRequiredService<ISupabaseClientFactory>();

        var first = factory.CreateClient("custom-key", o =>
        {
            o.AutoConnectRealtime = false;
            o.AutoRefreshToken = false;
        });
        var second = factory.CreateClient("custom-key");

        Assert.NotSame(first, second);
        Assert.Equal("custom-key", GetPrivateField<string>(first, "_supabaseKey"));

        var firstOptions = GetPrivateField<global::Supabase.SupabaseOptions>(first, "_options");
        Assert.False(firstOptions.AutoConnectRealtime);
        Assert.False(firstOptions.AutoRefreshToken);

        var secondOptions = GetPrivateField<global::Supabase.SupabaseOptions>(second, "_options");
        Assert.True(secondOptions.AutoConnectRealtime);
        Assert.True(secondOptions.AutoRefreshToken);
    }

    private static T GetPrivateField<T>(Client client, string fieldName)
    {
        var field = client.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        var value = field!.GetValue(client);
        Assert.IsType<T>(value);
        return (T)value;
    }
}
