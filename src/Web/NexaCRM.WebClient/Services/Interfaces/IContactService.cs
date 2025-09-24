using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface IContactService
    {
        Task<IEnumerable<Contact>> GetContactsAsync();
        Task<Contact> CreateContactAsync(Contact contact, CancellationToken cancellationToken = default);
    }
}
