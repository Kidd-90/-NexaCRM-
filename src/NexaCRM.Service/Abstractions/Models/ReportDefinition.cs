using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NexaCRM.UI.Models
{
    public class ReportDefinition
    {
        [Required(ErrorMessage = "Report name is required")]
        public string? Name { get; set; }
        public List<string> SelectedFields { get; set; } = new();
        public Dictionary<string, string> Filters { get; set; } = new();
    }
}
