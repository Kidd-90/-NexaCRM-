using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Services.Interfaces;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Services.Mock
{
    public class MockReportService : IReportService
    {
        public System.Threading.Tasks.Task<ReportData> GetQuarterlyPerformanceAsync()
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
            return System.Threading.Tasks.Task.FromResult(data);
        }

        public System.Threading.Tasks.Task<ReportData> GetLeadSourceAnalyticsAsync()
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
            return System.Threading.Tasks.Task.FromResult(data);
        }

        public System.Threading.Tasks.Task<ReportData> GetTicketVolumeAsync()
        {
            var data = new ReportData
            {
                Title = "Ticket Volume",
                Data = new Dictionary<string, double>
                {
                    { "Total", 125 }
                }
            };
            return System.Threading.Tasks.Task.FromResult(data);
        }

        public System.Threading.Tasks.Task<ReportData> GetResolutionRateAsync()
        {
            var data = new ReportData
            {
                Title = "Resolution Rate",
                Data = new Dictionary<string, double>
                {
                    { "Rate", 0.85 }
                }
            };
            return System.Threading.Tasks.Task.FromResult(data);
        }

        public System.Threading.Tasks.Task<ReportData> GetTicketsByCategoryAsync()
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
            return System.Threading.Tasks.Task.FromResult(data);
        }
    }
}
