using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services.Mock
{
    public class MockContactService : IContactService
    {
        public Task<IEnumerable<Contact>> GetContactsAsync(CancellationToken cancellationToken = default)
        {
            var contacts = new List<Contact>
            {
                new Contact { Id = 1, SupabaseId = Guid.Parse("00000000-0000-0000-0000-000000000001"), FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", PhoneNumber = "123-456-7890" },
                new Contact { Id = 2, SupabaseId = Guid.Parse("00000000-0000-0000-0000-000000000002"), FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", PhoneNumber = "098-765-4321" },
                new Contact { Id = 3, SupabaseId = Guid.Parse("00000000-0000-0000-0000-000000000003"), FirstName = "Peter", LastName = "Jones", Email = "peter.jones@example.com", PhoneNumber = "111-222-3333" },
                new Contact { Id = 4, SupabaseId = Guid.Parse("00000000-0000-0000-0000-000000000004"), FirstName = "Mary", LastName = "Brown", Email = "mary.brown@example.com", PhoneNumber = "444-555-6666" },
                new Contact { Id = 5, SupabaseId = Guid.Parse("00000000-0000-0000-0000-000000000005"), FirstName = "David", LastName = "Wilson", Email = "david.wilson@example.com", PhoneNumber = "777-888-9999" },
                new Contact { Id = 6, SupabaseId = Guid.Parse("00000000-0000-0000-0000-000000000006"), FirstName = "Susan", LastName = "Taylor", Email = "susan.taylor@example.com", PhoneNumber = "123-123-1234" },
                new Contact { Id = 7, SupabaseId = Guid.Parse("00000000-0000-0000-0000-000000000007"), FirstName = "Michael", LastName = "Clark", Email = "michael.clark@example.com", PhoneNumber = "456-456-4567" },
                new Contact { Id = 8, SupabaseId = Guid.Parse("00000000-0000-0000-0000-000000000008"), FirstName = "Linda", LastName = "Harris", Email = "linda.harris@example.com", PhoneNumber = "789-789-7890" },
                new Contact { Id = 9, SupabaseId = Guid.Parse("00000000-0000-0000-0000-000000000009"), FirstName = "Robert", LastName = "Lee", Email = "robert.lee@example.com", PhoneNumber = "112-233-4455" },
                new Contact { Id = 10, SupabaseId = Guid.Parse("00000000-0000-0000-0000-000000000010"), FirstName = "Patricia", LastName = "Walker", Email = "patricia.walker@example.com", PhoneNumber = "667-788-9900" }
            };
            return Task.FromResult<IEnumerable<Contact>>(contacts);
        }
    }
}
