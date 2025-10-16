using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexaCRM.Service.Models.DB;

namespace NexaCRM.Service.Abstractions.Services
{
    /// <summary>
    /// Service for managing leads (potential customers)
    /// </summary>
    public interface ILeadService
    {
        // Lead CRUD operations
        Task<IEnumerable<Lead>> GetLeadsAsync(LeadFilterCriteria? filter = null, CancellationToken cancellationToken = default);
        Task<Lead?> GetLeadByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<Lead> CreateLeadAsync(Lead lead, CancellationToken cancellationToken = default);
        Task<Lead> UpdateLeadAsync(Lead lead, CancellationToken cancellationToken = default);
        Task<bool> DeleteLeadAsync(long id, CancellationToken cancellationToken = default);
        
        // Lead status management
        Task<bool> UpdateLeadStatusAsync(long id, LeadStatus newStatus, CancellationToken cancellationToken = default);
        Task<bool> ConvertLeadToCustomerAsync(long id, Guid convertedBy, CancellationToken cancellationToken = default);
        Task<bool> AssignLeadAsync(long id, Guid assignToUserId, CancellationToken cancellationToken = default);
        
        // Lead scoring
        Task<bool> UpdateLeadScoreAsync(long id, int score, CancellationToken cancellationToken = default);
        Task<int> CalculateLeadScoreAsync(Lead lead, CancellationToken cancellationToken = default);
        
        // Lead inbox queries
        Task<IEnumerable<Lead>> GetNewLeadsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Lead>> GetMyLeadsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Lead>> GetOverdueLeadsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Lead>> GetLeadsBySourceAsync(long sourceId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Service for managing lead sources
    /// </summary>
    public interface ILeadSourceService
    {
        // Lead source CRUD
        Task<IEnumerable<LeadSource>> GetLeadSourcesAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
        Task<LeadSource?> GetLeadSourceByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<LeadSource> CreateLeadSourceAsync(LeadSource source, CancellationToken cancellationToken = default);
        Task<LeadSource> UpdateLeadSourceAsync(LeadSource source, CancellationToken cancellationToken = default);
        Task<bool> DeleteLeadSourceAsync(long id, CancellationToken cancellationToken = default);
        
        // Lead source analytics
        Task<IEnumerable<LeadSourceAnalytics>> GetLeadSourceAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
        Task<LeadSourceAnalytics?> GetLeadSourceAnalyticsByIdAsync(long sourceId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
        Task<Dictionary<string, int>> GetLeadCountByCategoryAsync(CancellationToken cancellationToken = default);
    }
}
