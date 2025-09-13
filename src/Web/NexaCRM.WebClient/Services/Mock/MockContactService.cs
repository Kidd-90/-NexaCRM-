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
                new Contact { Id = 3, FirstName = "Peter", LastName = "Jones", Email = "peter.jones@example.com", PhoneNumber = "111-222-3333" },
                new Contact { Id = 4, FirstName = "Mary", LastName = "Brown", Email = "mary.brown@example.com", PhoneNumber = "444-555-6666" },
                new Contact { Id = 5, FirstName = "David", LastName = "Wilson", Email = "david.wilson@example.com", PhoneNumber = "777-888-9999" },
                new Contact { Id = 6, FirstName = "Susan", LastName = "Taylor", Email = "susan.taylor@example.com", PhoneNumber = "123-123-1234" },
                new Contact { Id = 7, FirstName = "Michael", LastName = "Clark", Email = "michael.clark@example.com", PhoneNumber = "456-456-4567" },
                new Contact { Id = 8, FirstName = "Linda", LastName = "Harris", Email = "linda.harris@example.com", PhoneNumber = "789-789-7890" },
                new Contact { Id = 9, FirstName = "Robert", LastName = "Lee", Email = "robert.lee@example.com", PhoneNumber = "112-233-4455" },
                new Contact { Id = 10, FirstName = "Patricia", LastName = "Walker", Email = "patricia.walker@example.com", PhoneNumber = "667-788-9900" }
            };
            return System.Threading.Tasks.Task.FromResult<IEnumerable<Contact>>(contacts);
        }
    }
}
