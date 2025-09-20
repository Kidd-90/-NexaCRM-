using System;
using NexaCRM.WebClient.Models.Enums;

namespace NexaCRM.WebClient.Models
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
