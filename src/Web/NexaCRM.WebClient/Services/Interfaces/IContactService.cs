using NexaCRM.WebClient.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface IContactService
    {
        Task<IEnumerable<Contact>> GetContactsAsync(CancellationToken cancellationToken = default);
    }
}
