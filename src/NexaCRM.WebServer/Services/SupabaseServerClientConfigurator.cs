using NexaCRM.Service.Supabase.Configuration;
using Supabase;

namespace NexaCRM.WebServer.Services;

public sealed class SupabaseServerClientConfigurator : ISupabaseClientConfigurator
{
    private readonly SupabaseServerSessionPersistence _sessionPersistence;

    public SupabaseServerClientConfigurator(SupabaseServerSessionPersistence sessionPersistence)
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
        // No server-specific client adjustments required yet.
    }
}
