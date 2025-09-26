using NexaCRM.WebClient.Models.Enums;

namespace NexaCRM.WebClient.Models
{
    public class ConsultationNote
    {
        public int Id { get; set; }
        public int ContactId { get; set; }
        public string? ContactName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? Tags { get; set; }
        public ConsultationPriority Priority { get; set; }
        public bool IsFollowUpRequired { get; set; }
        public DateTime? FollowUpDate { get; set; }
    }
}