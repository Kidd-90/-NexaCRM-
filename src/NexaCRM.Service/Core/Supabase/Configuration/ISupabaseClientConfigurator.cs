using Supabase;

namespace NexaCRM.Service.Supabase.Configuration;

/// <summary>
/// Allows host environments to customize Supabase client options and instance configuration without
/// duplicating the core service implementations.
/// </summary>
public interface ISupabaseClientConfigurator
{
    /// <summary>
    /// Mutates the options prior to the Supabase client being constructed.
    /// </summary>
    /// <param name="options">The Supabase options instance to configure.</param>
    void ConfigureOptions(SupabaseOptions options);

    /// <summary>
    /// Performs additional setup after the Supabase client has been created.
    /// </summary>
    /// <param name="client">The constructed Supabase client.</param>
    void ConfigureClient(Client client);
}
