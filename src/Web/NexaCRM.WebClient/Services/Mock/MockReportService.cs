using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Mock
{
    public class MockReportService : IReportService
    {
        public Task<ReportData> GetQuarterlyPerformanceAsync()
        {
            var data = new ReportData
            {
                Title = "Revenue By Quarter",
                Data = new Dictionary<string, double>
                {
                    { "Q1", 70000 },
                    { "Q2", 90000 },
                    { "Q3", 70000 },
                    { "Q4", 30000 }
                }
            };
            return Task.FromResult(data);
        }

        public Task<ReportData> GetLeadSourceAnalyticsAsync()
        {
            var data = new ReportData
            {
                Title = "Leads By Source",
                Data = new Dictionary<string, double>
                {
                    { "Website", 100 },
                    { "Referral", 60 },
                    { "Social Media", 30 },
                    { "Email Campaign", 60 }
                }
            };
            return Task.FromResult(data);
        }

        public Task<ReportData> GetTicketVolumeAsync()
        {
            var data = new ReportData
            {
                Title = "Ticket Volume",
                Data = new Dictionary<string, double>
                {
                    { "Total", 125 }
                }
            };
            return Task.FromResult(data);
        }

        public Task<ReportData> GetResolutionRateAsync()
        {
            var data = new ReportData
            {
                Title = "Resolution Rate",
                Data = new Dictionary<string, double>
                {
                    { "Rate", 0.85 }
                }
            };
            return Task.FromResult(data);
        }

        public Task<ReportData> GetTicketsByCategoryAsync()
        {
            var data = new ReportData
            {
                Title = "Tickets By Category",
                Data = new Dictionary<string, double>
                {
                    { "Billing", 50 },
                    { "Technical", 40 },
                    { "General", 70 },
                    { "Feedback", 70 }
                }
            };
            return Task.FromResult(data);
        }
    }
}
