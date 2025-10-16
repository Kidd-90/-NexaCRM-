using System;
using System.Collections.Generic;
using System.Linq;

namespace NexaCRM.Service.Models.DB
{
    /// <summary>
    /// Represents a lead (potential customer) in the CRM system
    /// </summary>
    public class Lead
    {
        public long Id { get; set; }
        public string? FirstName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? CompanyName { get; set; }
        public string? Title { get; set; }
        
        // Lead-specific properties
        public LeadStatus Status { get; set; } = LeadStatus.New;
        public long? LeadSourceId { get; set; }
        public string? SourceName { get; set; }
        public string? SourceCategory { get; set; }
        public int? LeadScore { get; set; }
        public string? LeadNotes { get; set; }
        
        // Dates
        public DateTime? FollowUpDate { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public DateTime? ConvertedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Relationships
        public Guid? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public Guid? CreatedBy { get; set; }
        public long? ConvertedToCustomerId { get; set; }
        
        // Computed properties
        public string FullName => string.Join(" ", new[] { FirstName, LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
        public bool IsOverdue => FollowUpDate.HasValue && FollowUpDate.Value < DateTime.Today && Status != LeadStatus.Converted;
        public int DaysSinceCreated => (DateTime.UtcNow - CreatedAt).Days;
        public int? DaysSinceLastActivity => LastActivityDate.HasValue ? (DateTime.UtcNow - LastActivityDate.Value).Days : null;
    }

    /// <summary>
    /// Lead status enumeration
    /// </summary>
    public enum LeadStatus
    {
        New,        // 새로운 리드
        Contacted,  // 접촉됨
        Qualified,  // 검증됨 (잠재력 있음)
        Converted,  // 고객으로 전환됨
        Lost,       // 실패
        Customer    // 고객
    }

    /// <summary>
    /// Lead source tracking
    /// </summary>
    public class LeadSource
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; } = true;
        public Dictionary<string, object>? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Computed
        public string DisplayName => !string.IsNullOrWhiteSpace(Category) 
            ? $"{Name} ({Category})" 
            : Name;
    }

    /// <summary>
    /// Lead source analytics data
    /// </summary>
    public class LeadSourceAnalytics
    {
        public long SourceId { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int TotalLeads { get; set; }
        public int ConvertedLeads { get; set; }
        public int ActiveLeads { get; set; }
        public int LostLeads { get; set; }
        public decimal ConversionRate => TotalLeads > 0 ? (decimal)ConvertedLeads / TotalLeads * 100 : 0;
        public decimal AverageLeadScore { get; set; }
        public int AverageDaysToConversion { get; set; }
    }

    /// <summary>
    /// Lead filter criteria
    /// </summary>
    public class LeadFilterCriteria
    {
        public LeadStatus? Status { get; set; }
        public long? SourceId { get; set; }
        public string? SourceCategory { get; set; }
        public int? MinScore { get; set; }
        public int? MaxScore { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public Guid? AssignedTo { get; set; }
        public bool? HasFollowUp { get; set; }
        public bool? IsOverdue { get; set; }
        public string? SearchTerm { get; set; }
    }
}
