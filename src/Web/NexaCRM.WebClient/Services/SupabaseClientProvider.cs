using System.Threading;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services;

/// <summary>
/// Coordinates initialization of the Supabase client so that it is executed only once.
/// </summary>
public sealed class SupabaseClientProvider
{
    private readonly Supabase.Client _client;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _initialized;

    public SupabaseClientProvider(Supabase.Client client)
    {
        _client = client;
    }

    public async Task<Supabase.Client> GetClientAsync()
    {
        if (_initialized)
        {
            return _client;
        }

        await _initializationLock.WaitAsync();
        try
        {
            if (!_initialized)
            {
                await _client.InitializeAsync();
                _initialized = true;
            }
        }
        finally
        {
            _initializationLock.Release();
        }

        return _client;
    }
}
