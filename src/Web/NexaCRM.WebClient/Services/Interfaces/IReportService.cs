using NexaCRM.WebClient.Models;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface IReportService
    {
        Task<ReportData> GetQuarterlyPerformanceAsync();
        Task<ReportData> GetLeadSourceAnalyticsAsync();
        Task<ReportData> GetTicketVolumeAsync();
        Task<ReportData> GetResolutionRateAsync();
        Task<ReportData> GetTicketsByCategoryAsync();
    }
}
