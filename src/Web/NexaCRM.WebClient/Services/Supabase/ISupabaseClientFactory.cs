using System.Threading;
using System.Threading.Tasks;
using NexaCRM.WebClient.Options;
using Supabase;
using Supabase.Gotrue;

namespace NexaCRM.WebClient.Services.Supabase;

public interface ISupabaseClientFactory
{
    Task<Client> CreateClientAsync(SupabaseClientOptions configuration, SupabaseOptions supabaseOptions, CancellationToken cancellationToken = default);
}
