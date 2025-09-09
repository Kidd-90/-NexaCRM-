using NexaCRM.WebClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface IDealService
    {
        Task<IEnumerable<Deal>> GetDealsAsync();
    }
}
