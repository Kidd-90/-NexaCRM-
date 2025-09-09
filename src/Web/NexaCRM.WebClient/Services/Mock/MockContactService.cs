using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Services.Interfaces;
using System.Collections.Generic;

namespace NexaCRM.WebClient.Services.Mock
{
    public class MockContactService : IContactService
    {
        public System.Threading.Tasks.Task<IEnumerable<Contact>> GetContactsAsync()
        {
            var contacts = new List<Contact>
            {
                new Contact { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", PhoneNumber = "123-456-7890" },
                new Contact { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", PhoneNumber = "098-765-4321" },
                new Contact { Id = 3, FirstName = "Peter", LastName = "Jones", Email = "peter.jones@example.com", PhoneNumber = "111-222-3333" }
            };
            return System.Threading.Tasks.Task.FromResult<IEnumerable<Contact>>(contacts);
        }
    }
}
