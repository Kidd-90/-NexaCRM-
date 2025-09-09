using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Models.Enums;
using NexaCRM.WebClient.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Mock
{
    public class MockSupportTicketService : ISupportTicketService
    {
        private readonly List<SupportTicket> _tickets;

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

        public Task<IEnumerable<SupportTicket>> GetTicketsAsync()
        {
            return Task.FromResult<IEnumerable<SupportTicket>>(_tickets);
        }

        public Task<SupportTicket> GetTicketByIdAsync(int id)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == id);
            return Task.FromResult(ticket);
        }

        public Task<IEnumerable<SupportTicket>> GetLiveInteractionsAsync()
        {
            return Task.FromResult<IEnumerable<SupportTicket>>(_tickets.Where(t => t.Status == TicketStatus.InProgress || t.Status == TicketStatus.Open));
        }

        public Task CreateTicketAsync(SupportTicket ticket)
        {
            ticket.Id = _tickets.Max(t => t.Id) + 1;
            _tickets.Add(ticket);
            return Task.CompletedTask;
        }

        public Task UpdateTicketAsync(SupportTicket ticket)
        {
            var existingTicket = _tickets.FirstOrDefault(t => t.Id == ticket.Id);
            if (existingTicket != null)
            {
                existingTicket.Subject = ticket.Subject;
                existingTicket.Description = ticket.Description;
                existingTicket.Status = ticket.Status;
                existingTicket.Priority = ticket.Priority;
                existingTicket.CustomerName = ticket.CustomerName;
                existingTicket.AgentName = ticket.AgentName;
                existingTicket.Category = ticket.Category;
            }
            return Task.CompletedTask;
        }

        public Task DeleteTicketAsync(int id)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == id);
            if (ticket != null)
            {
                _tickets.Remove(ticket);
            }
            return Task.CompletedTask;
        }
    }
}
