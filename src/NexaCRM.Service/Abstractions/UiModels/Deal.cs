using System;

namespace NexaCRM.UI.Models
{
    public class Deal
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Stage { get; set; }
        public decimal Amount { get; set; }
        public string? Company { get; set; }
        public string? ContactPerson { get; set; }
        public string? Owner { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Today;
    }
}
