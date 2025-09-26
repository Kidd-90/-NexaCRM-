using System;
using NexaCRM.UI.Models.Enums;

namespace NexaCRM.UI.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public int ContactId { get; set; }
        public ActivityType Type { get; set; }
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string? CreatedBy { get; set; }
    }
}
