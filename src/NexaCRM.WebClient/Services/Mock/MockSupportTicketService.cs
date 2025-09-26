using NexaCRM.UI.Models;
using NexaCRM.UI.Models.Enums;
using NexaCRM.UI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NexaCRM.WebClient.Services.Mock
{
    public class MockSupportTicketService : ISupportTicketService
    {
        private readonly List<SupportTicket> _tickets;

        public event Action<SupportTicket>? TicketUpserted;
        public event Action<int>? TicketDeleted;
        public event Action<int>? LiveTicketCountChanged;

        public MockSupportTicketService()
        {
            _tickets = new List<SupportTicket>
            {
                new SupportTicket { Id = 1, Subject = "Payment Issue", Description = "Customer is having trouble with payment.", Status = TicketStatus.InProgress, Priority = TicketPriority.High, CustomerName = "Sophia Clark", AgentName = "Ethan Harper", CreatedAt = DateTime.Now.AddMinutes(-5), Category = "Billing" },
                new SupportTicket { Id = 2, Subject = "Product Inquiry", Description = "Customer has a question about a product.", Status = TicketStatus.Open, Priority = TicketPriority.Medium, CustomerName = "Liam Carter", AgentName = "Olivia Bennett", CreatedAt = DateTime.Now.AddMinutes(-12), Category = "General" },
                new SupportTicket { Id = 3, Subject = "Technical Support", Description = "Customer needs help with a technical issue.", Status = TicketStatus.Resolved, Priority = TicketPriority.High, CustomerName = "Ava Reynolds", AgentName = "Noah Foster", CreatedAt = DateTime.Now.AddMinutes(-20), Category = "Technical" },
                new SupportTicket { Id = 4, Subject = "Refund Request", Description = "Customer wants to request a refund.", Status = TicketStatus.InProgress, Priority = TicketPriority.Medium, CustomerName = "Jackson Hayes", AgentName = "Isabella Reed", CreatedAt = DateTime.Now.AddMinutes(-25), Category = "Billing" },
                new SupportTicket { Id = 5, Subject = "Account Access", Description = "Customer is having trouble accessing their account.", Status = TicketStatus.Open, Priority = TicketPriority.Low, CustomerName = "Chloe Morgan", AgentName = "Lucas Coleman", CreatedAt = DateTime.Now.AddMinutes(-30), Category = "Technical" }
            };
        }

        public System.Threading.Tasks.Task<IEnumerable<SupportTicket>> GetTicketsAsync()
        {
            return System.Threading.Tasks.Task.FromResult<IEnumerable<SupportTicket>>(_tickets);
        }

        public System.Threading.Tasks.Task<SupportTicket?> GetTicketByIdAsync(int id)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == id);
            return System.Threading.Tasks.Task.FromResult(ticket);
        }

        public System.Threading.Tasks.Task<IEnumerable<SupportTicket>> GetLiveInteractionsAsync()
        {
            return System.Threading.Tasks.Task.FromResult<IEnumerable<SupportTicket>>(_tickets.Where(t => t.Status == TicketStatus.InProgress || t.Status == TicketStatus.Open));
        }

        public System.Threading.Tasks.Task CreateTicketAsync(SupportTicket ticket)
        {
            ticket.Id = _tickets.Max(t => t.Id) + 1;
            _tickets.Add(ticket);
            TicketUpserted?.Invoke(ticket);
            NotifyLiveCount();
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task UpdateTicketAsync(SupportTicket ticket)
        {
            var existingTicket = _tickets.FirstOrDefault(t => t.Id == ticket.Id);
            if (existingTicket != null)
            {
                existingTicket.Subject = ticket.Subject;
                existingTicket.Description = ticket.Description;
                existingTicket.Status = ticket.Status;
                existingTicket.Priority = ticket.Priority;
                existingTicket.CustomerName = ticket.CustomerName;
                existingTicket.AgentId = ticket.AgentId;
                existingTicket.AgentName = ticket.AgentName;
                existingTicket.Category = ticket.Category;
                TicketUpserted?.Invoke(existingTicket);
                NotifyLiveCount();
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task DeleteTicketAsync(int id)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == id);
            if (ticket != null)
            {
                _tickets.Remove(ticket);
                TicketDeleted?.Invoke(id);
                NotifyLiveCount();
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private void NotifyLiveCount()
        {
            var liveCount = _tickets.Count(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress);
            LiveTicketCountChanged?.Invoke(liveCount);
        }
    }
}
