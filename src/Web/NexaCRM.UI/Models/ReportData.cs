using System.Collections.Generic;

namespace NexaCRM.WebClient.Models
{
    public class ReportData
    {
        public string? Title { get; set; }
        public Dictionary<string, double>? Data { get; set; }
    }
}
