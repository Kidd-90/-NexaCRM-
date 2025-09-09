using NexaCRM.WebClient.Models;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface ISupportTicketService
    {
        System.Threading.Tasks.Task<IEnumerable<SupportTicket>> GetTicketsAsync();
        System.Threading.Tasks.Task<SupportTicket?> GetTicketByIdAsync(int id);
        System.Threading.Tasks.Task<IEnumerable<SupportTicket>> GetLiveInteractionsAsync();
        System.Threading.Tasks.Task CreateTicketAsync(SupportTicket ticket);
        System.Threading.Tasks.Task UpdateTicketAsync(SupportTicket ticket);
        System.Threading.Tasks.Task DeleteTicketAsync(int id);
    }
}
