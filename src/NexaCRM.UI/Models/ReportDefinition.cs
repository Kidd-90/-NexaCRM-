using System.Collections.Generic;

namespace NexaCRM.UI.Models
{
    public class ReportDefinition
    {
        public string? Name { get; set; }
        public List<string> SelectedFields { get; set; } = new();
        public Dictionary<string, string> Filters { get; set; } = new();
    }
}
