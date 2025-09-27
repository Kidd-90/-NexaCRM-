using System;
using NexaCRM.UI.Models;
using System.Collections.Generic;

namespace NexaCRM.UI.Services.Interfaces
{
    public interface ISupportTicketService
    {
        event Action<SupportTicket>? TicketUpserted;
        event Action<int>? TicketDeleted;
        event Action<int>? LiveTicketCountChanged;

        System.Threading.Tasks.Task<IEnumerable<SupportTicket>> GetTicketsAsync();
        System.Threading.Tasks.Task<SupportTicket?> GetTicketByIdAsync(int id);
        System.Threading.Tasks.Task<IEnumerable<SupportTicket>> GetLiveInteractionsAsync();
        System.Threading.Tasks.Task CreateTicketAsync(SupportTicket ticket);
        System.Threading.Tasks.Task UpdateTicketAsync(SupportTicket ticket);
        System.Threading.Tasks.Task DeleteTicketAsync(int id);
    }
}
