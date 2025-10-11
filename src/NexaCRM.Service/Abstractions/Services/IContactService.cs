using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.UI.Models;

namespace NexaCRM.UI.Services.Interfaces
{
    public interface IContactService
    {
        Task<IEnumerable<Contact>> GetContactsAsync();
        Task<IEnumerable<Contact>> GetContactsByUserAsync(Guid userId);
        Task<Contact> CreateContactAsync(Contact contact, CancellationToken cancellationToken = default);
    }
}
