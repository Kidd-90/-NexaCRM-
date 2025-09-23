using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Db;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface IDuplicateService
    {
        Task<IReadOnlyList<DuplicateGroup>> FindDuplicatesAsync(int withinDays, bool includeFuzzy);
        Task ArchiveAsync(DuplicateGroup group);
        Task DeleteAsync(DuplicateGroup group);
        Task MergeAsync(int primaryContactId, IEnumerable<int> duplicateContactIds);
    }

    public sealed class DuplicateGroup
    {
        public string Key { get; set; } = string.Empty; // group key (contact or fuzzy key)
        public string ContactDisplay { get; set; } = string.Empty; // shown in UI
        public int Count { get; set; }
        public double Score { get; set; } // 0-100 dedupe confidence score
        public DateTime LatestAssigned { get; set; }
        public string? SampleName { get; set; }
        public List<int> ContactIds { get; set; } = new();
        public List<DuplicateCandidate> Candidates { get; set; } = new();
    }

    public sealed class DuplicateCandidate
    {
        public int ContactId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
    }
}
