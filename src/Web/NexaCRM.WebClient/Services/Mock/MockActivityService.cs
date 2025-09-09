using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexaCRM.WebClient.Services.Mock
{
    public class MockActivityService : IActivityService
    {
        private readonly List<Activity> _activities;

        public MockActivityService()
        {
            _activities = new List<Activity>
            {
                new Activity { Id = 1, Type = "New Lead", Description = "John Doe from Acme Inc.", Timestamp = DateTime.Now.AddDays(-1) },
                new Activity { Id = 2, Type = "Sales Call", Description = "Follow up with Jane Smith.", Timestamp = DateTime.Now.AddDays(-2) },
                new Activity { Id = 3, Type = "Closed Deal", Description = "Won a deal with Peter Jones.", Timestamp = DateTime.Now.AddDays(-3) }
            };
        }

        public Task<IEnumerable<Activity>> GetRecentActivitiesAsync()
        {
            return Task.FromResult<IEnumerable<Activity>>(_activities);
        }
    }
}
