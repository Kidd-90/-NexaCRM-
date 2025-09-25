using System.Threading;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Models.SupabaseOperations;

namespace NexaCRM.Services.Admin.Interfaces;

public interface ISupabaseMonitoringService
{
    Task<SupabaseMetricSnapshot> GetCurrentMetricsAsync(CancellationToken cancellationToken = default);
}

