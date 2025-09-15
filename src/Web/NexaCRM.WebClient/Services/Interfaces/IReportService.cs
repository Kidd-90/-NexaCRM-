using System.Collections.Generic;
using NexaCRM.WebClient.Models;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface IReportService
    {
        System.Threading.Tasks.Task SaveReportDefinitionAsync(ReportDefinition definition);
        System.Threading.Tasks.Task<IEnumerable<ReportDefinition>> GetReportDefinitionsAsync();
        System.Threading.Tasks.Task<ReportData> GenerateReportAsync(ReportDefinition definition);
        System.Threading.Tasks.Task<ReportData> GetQuarterlyPerformanceAsync();
        System.Threading.Tasks.Task<ReportData> GetLeadSourceAnalyticsAsync();
        System.Threading.Tasks.Task<ReportData> GetTicketVolumeAsync();
        System.Threading.Tasks.Task<ReportData> GetResolutionRateAsync();
        System.Threading.Tasks.Task<ReportData> GetTicketsByCategoryAsync();
    }
}
