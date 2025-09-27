using NexaCRM.UI.Models.Enums;
using System;

namespace NexaCRM.UI.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public Priority Priority { get; set; }
        public string? AssignedTo { get; set; }
    }
}
