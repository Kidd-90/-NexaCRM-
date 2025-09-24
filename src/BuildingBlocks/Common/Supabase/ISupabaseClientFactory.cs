using Supabase;

namespace BuildingBlocks.Common.Supabase;

/// <summary>
/// Provides a centralised way to materialise configured <see cref="Client"/> instances for NexaCRM services.
/// </summary>
public interface ISupabaseClientFactory
{
    /// <summary>
    /// Returns a cached <see cref="Client"/> initialised with the configured service role key.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when either the Supabase URL or service role key has not been configured.</exception>
    Client GetServiceClient();

    /// <summary>
    /// Returns a cached <see cref="Client"/> initialised with the configured anon key.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when either the Supabase URL or anon key has not been configured.</exception>
    Client GetAnonClient();

    /// <summary>
    /// Materialises a fresh <see cref="Client"/> using the supplied API key.
    /// </summary>
    /// <param name="apiKey">The API key that should be associated with the client.</param>
    /// <param name="configure">Optional delegate that can tweak the generated <see cref="global::Supabase.SupabaseOptions"/>.</param>
    /// <returns>A new <see cref="Client"/> instance using the current Supabase URL.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="apiKey"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the Supabase URL has not been configured.</exception>
    Client CreateClient(string apiKey, Action<global::Supabase.SupabaseOptions>? configure = null);
}
