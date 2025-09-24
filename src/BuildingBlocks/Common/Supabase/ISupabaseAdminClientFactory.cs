using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Common.Options;
using Supabase;
using Supabase.Gotrue;

namespace BuildingBlocks.Common.Supabase;

/// <summary>
/// Creates configured Supabase clients for server-side scenarios that require the service role key.
/// </summary>
public interface ISupabaseAdminClientFactory
{
    Task<Client> CreateClientAsync(SupabaseServerOptions options, SupabaseOptions supabaseOptions, CancellationToken cancellationToken = default);
}
