using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BuildingBlocks.Common.Supabase.Data.Contacts;

/// <summary>
/// Provides access to Supabase contact records via PostgREST queries.
/// </summary>
public interface ISupabaseContactRepository
{
    /// <summary>
    /// Retrieves the available contacts from Supabase.
    /// </summary>
    Task<IReadOnlyList<SupabaseContactRecord>> GetContactsAsync(CancellationToken cancellationToken = default);
}
