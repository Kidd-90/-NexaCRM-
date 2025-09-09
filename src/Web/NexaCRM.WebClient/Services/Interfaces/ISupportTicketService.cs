using NexaCRM.WebClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface ISupportTicketService
    {
        Task<IEnumerable<SupportTicket>> GetTicketsAsync();
        Task<SupportTicket> GetTicketByIdAsync(int id);
        Task<IEnumerable<SupportTicket>> GetLiveInteractionsAsync();
        Task CreateTicketAsync(SupportTicket ticket);
        Task UpdateTicketAsync(SupportTicket ticket);
        Task DeleteTicketAsync(int id);
    }
}
