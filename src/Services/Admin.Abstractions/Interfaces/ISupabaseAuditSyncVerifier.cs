using System.Threading;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Models.SupabaseOperations;

namespace NexaCRM.Services.Admin.Interfaces;

public interface ISupabaseAuditSyncVerifier
{
    Task<SupabaseAuditSyncReport> ValidateAsync(CancellationToken cancellationToken = default);
}

