using NexaCRM.WebClient.Models.Enums;
using System;

namespace NexaCRM.WebClient.Models
{
    public class SupportTicket
    {
        public int Id { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }
        public string? CustomerName { get; set; }
        public string? AgentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Category { get; set; }
    }
}
