using System;

namespace NexaCRM.UI.Models
{
    public sealed class DealCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public int StageId { get; set; }
        public decimal Amount { get; set; }
        public string? Company { get; set; }
        public string? ContactName { get; set; }
        public string? Owner { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }
    }
}
