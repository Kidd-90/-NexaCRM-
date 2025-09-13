using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Models.Enums;
using NexaCRM.WebClient.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace NexaCRM.WebClient.Services.Mock
{
    public class MockActivityService : IActivityService
    {
        private readonly List<Activity> _activities;

        public MockActivityService()
        {
            _activities = new List<Activity>
            {
                new Activity { Id = 1, ContactId = 1, Type = ActivityType.Note, Content = "Initial contact made. Showed interest in Product A.", Timestamp = DateTime.Now.AddDays(-5), CreatedBy = "이영업" },
                new Activity { Id = 2, ContactId = 1, Type = ActivityType.Call, Content = "Follow-up call. Scheduled a demo for next week.", Timestamp = DateTime.Now.AddDays(-2), CreatedBy = "이영업" },
                new Activity { Id = 3, ContactId = 2, Type = ActivityType.Email, Content = "Sent marketing material about Product B.", Timestamp = DateTime.Now.AddDays(-10), CreatedBy = "박세일" },
                new Activity { Id = 4, ContactId = 3, Type = ActivityType.Meeting, Content = "Met at the conference. Discussed potential partnership.", Timestamp = DateTime.Now.AddDays(-20), CreatedBy = "김관리" },
                new Activity { Id = 5, ContactId = 3, Type = ActivityType.Note, Content = "Needs a follow-up call in a month.", Timestamp = DateTime.Now.AddDays(-19), CreatedBy = "김관리" }
            };
        }

        public Task<IEnumerable<Activity>> GetActivitiesByContactIdAsync(int contactId)
        {
            var activities = _activities.Where(a => a.ContactId == contactId).OrderByDescending(a => a.Timestamp);
            return Task.FromResult(activities.AsEnumerable());
        }

        public Task AddActivityAsync(Activity activity)
        {
            activity.Id = _activities.Any() ? _activities.Max(a => a.Id) + 1 : 1;
            activity.Timestamp = DateTime.Now;
            _activities.Add(activity);
            return Task.CompletedTask;
        }
    }
}
