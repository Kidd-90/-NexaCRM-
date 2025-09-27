using NexaCRM.Service.Supabase.Configuration;
using Supabase;

namespace NexaCRM.WebClient.Supabase.Environment;

public sealed class WebAssemblySupabaseClientConfigurator : ISupabaseClientConfigurator
{
    private readonly SupabaseSessionPersistence _sessionPersistence;

    public WebAssemblySupabaseClientConfigurator(SupabaseSessionPersistence sessionPersistence)
    {
        _sessionPersistence = sessionPersistence;
    }

    public void ConfigureOptions(SupabaseOptions options)
    {
        options.AutoConnectRealtime = false;
        options.SessionHandler = _sessionPersistence;
    }

    public void ConfigureClient(Client client)
    {
        // No additional client-level configuration required for WebAssembly today.
    }
}
