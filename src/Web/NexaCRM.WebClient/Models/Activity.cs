using System;

namespace NexaCRM.WebClient.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
