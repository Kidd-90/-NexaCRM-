using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NexaCRM.WebClient.Models.Db;
using NexaCRM.WebClient.Services.Interfaces;

namespace NexaCRM.WebClient.Services
{
    public class DuplicateService : IDuplicateService
    {
        private readonly IDbDataService _db;
        private readonly IDedupeConfigService _cfg;

        public DuplicateService(IDbDataService db, IDedupeConfigService cfg)
        {
            _db = db;
            _cfg = cfg;
        }

        public async Task<IReadOnlyList<DuplicateGroup>> FindDuplicatesAsync(int withinDays, bool includeFuzzy)
        {
            var all = (await _db.GetAllDbListAsync())?.Where(c => !c.IsArchived).ToList() ?? new List<DbCustomer>();
            var cutoff = DateTime.Today.AddDays(-Math.Clamp(withinDays, 1, 365));
            var recent = all.Where(c => c.AssignedDate.Date >= cutoff).ToList();

            var result = new List<DuplicateGroup>();

            // Exact by normalized contact number
            var byContact = recent
                .GroupBy(c => NormalizeDigits(c.ContactNumber))
                .Where(g => !string.IsNullOrWhiteSpace(g.Key) && g.Count() > 1);

            foreach (var g in byContact)
            {
                var ordered = g.OrderByDescending(x => x.AssignedDate).ToList();
                var group = new DuplicateGroup
                {
                    Key = g.Key,
                    ContactDisplay = g.Key,
                    Count = g.Count(),
                    LatestAssigned = ordered.First().AssignedDate,
                    SampleName = ordered.First().CustomerName,
                    ContactIds = ordered.Select(x => x.ContactId).ToList(),
                    Candidates = ordered.Select(x => new DuplicateCandidate
                    {
                        ContactId = x.ContactId,
                        CustomerName = x.CustomerName,
                        AssignedDate = x.AssignedDate
                    }).ToList()
                };
                group.Score = ComputeScore(ordered);
                result.Add(group);
            }

            if (includeFuzzy)
            {
                // Fuzzy: last 4 digits + name prefix (2 chars)
                var byFuzzy = recent
                    .GroupBy(c => new
                    {
                        Last4 = Last4(c.ContactNumber),
                        Name2 = NamePrefix2(c.CustomerName)
                    })
                    .Where(g => !string.IsNullOrWhiteSpace(g.Key.Last4) && g.Count() > 1)
                    .ToList();

                foreach (var g in byFuzzy)
                {
                    // Skip if already captured by exact groups (all same normalized contact)
                    var ids = g.Select(x => x.ContactId).ToList();
                    if (result.Any(r => ids.All(id => r.ContactIds.Contains(id))))
                        continue;

                    var ordered = g.OrderByDescending(x => x.AssignedDate).ToList();
                    var key = $"FUZZY:{g.Key.Last4}:{g.Key.Name2}";
                    var group = new DuplicateGroup
                    {
                        Key = key,
                        ContactDisplay = $"유사({g.Key.Last4}/{g.Key.Name2})",
                        Count = g.Count(),
                        LatestAssigned = ordered.First().AssignedDate,
                        SampleName = ordered.First().CustomerName,
                        ContactIds = ordered.Select(x => x.ContactId).ToList(),
                        Candidates = ordered.Select(x => new DuplicateCandidate
                        {
                            ContactId = x.ContactId,
                            CustomerName = x.CustomerName,
                            AssignedDate = x.AssignedDate
                        }).ToList()
                    };
                    group.Score = ComputeScore(ordered);
                    result.Add(group);
                }
            }

            // Apply threshold
            var threshold = Math.Clamp(_cfg.ScoreThreshold, 0, 100);
            var filtered = result.Where(r => r.Score >= threshold).ToList();

            return filtered
                .OrderByDescending(r => r.Score)
                .ThenByDescending(r => r.Count)
                .ThenByDescending(r => r.LatestAssigned)
                .ToList();
        }

        public Task ArchiveAsync(DuplicateGroup group)
            => _db.ArchiveCustomersAsync(group.ContactIds);

        public Task DeleteAsync(DuplicateGroup group)
            => _db.DeleteCustomersAsync(group.ContactIds);

        public Task MergeAsync(int primaryContactId, IEnumerable<int> duplicateContactIds)
            => _db.MergeCustomersAsync(primaryContactId, duplicateContactIds);

        private static string NormalizeDigits(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            return Regex.Replace(s, "\\D", "");
        }

        private static string Last4(string? s)
        {
            var d = NormalizeDigits(s);
            return d.Length >= 4 ? d[^4..] : d;
        }

        private static string NamePrefix2(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var t = new string(s.Where(ch => !char.IsWhiteSpace(ch)).ToArray());
            return t.Length >= 2 ? t[..2].ToLowerInvariant() : t.ToLowerInvariant();
        }

        private double ComputeScore(List<DbCustomer> group)
        {
            if (group.Count <= 1) return 0d;
            var refItem = group.OrderByDescending(x => x.AssignedDate).First();

            int totalWeight = 0;
            // Build enabled field descriptors
            var fields = new List<(bool enabled, int weight, Func<DbCustomer, object?> selector, Func<object?, object?, bool> equal)>
            {
                (_cfg.UseGender, _cfg.WeightGender, c => c.Gender, StrEq),
                (_cfg.UseAddress, _cfg.WeightAddress, c => c.Address, StrEq),
                (_cfg.UseJobTitle, _cfg.WeightJobTitle, c => c.JobTitle, StrEq),
                (_cfg.UseMaritalStatus, _cfg.WeightMaritalStatus, c => c.MaritalStatus, StrEq),
                (_cfg.UseProofNumber, _cfg.WeightProofNumber, c => c.ProofNumber, StrEq),
                (_cfg.UseDbPrice, _cfg.WeightDbPrice, c => c.DbPrice, NumEq),
                (_cfg.UseHeadquarters, _cfg.WeightHeadquarters, c => c.Headquarters, StrEq),
                (_cfg.UseInsuranceName, _cfg.WeightInsuranceName, c => c.InsuranceName, StrEq),
                (_cfg.UseCarJoinDate, _cfg.WeightCarJoinDate, c => c.CarJoinDate, DateEq),
                (_cfg.UseNotes, _cfg.WeightNotes, c => c.Notes, StrEq)
            }.Where(f => f.enabled && f.weight > 0).ToList();

            totalWeight = fields.Sum(f => f.weight);
            if (totalWeight == 0) return 0d;

            int pairs = group.Count - 1;
            int matchedSum = 0;

            foreach (var other in group.Where(c => !ReferenceEquals(c, refItem)))
            {
                foreach (var f in fields)
                {
                    var a = f.selector(refItem);
                    var b = f.selector(other);
                    if (f.equal(a, b)) matchedSum += f.weight;
                }
            }

            var maxPossible = totalWeight * pairs;
            if (maxPossible == 0) return 0d;
            var ratio = (double)matchedSum / maxPossible;

            // Name similarity bonus (Levenshtein <= 2 among any pair)
            bool similar = group.Any(other => !ReferenceEquals(other, refItem) && Levenshtein(NormalizeName(refItem.CustomerName), NormalizeName(other.CustomerName)) <= 2);
            var score = ratio * 100d + (similar ? 10d : 0d);
            if (score > 100d) score = 100d;
            return Math.Round(score, 1);
        }

        private static bool StrEq(object? a, object? b)
        {
            var sa = (a?.ToString() ?? string.Empty).Trim();
            var sb = (b?.ToString() ?? string.Empty).Trim();
            return sa.Length > 0 && sb.Length > 0 && string.Equals(sa, sb, StringComparison.OrdinalIgnoreCase);
        }

        private static bool NumEq(object? a, object? b)
        {
            if (a == null || b == null) return false;
            if (decimal.TryParse(a.ToString(), out var da) && decimal.TryParse(b.ToString(), out var db))
            {
                return da == db;
            }
            return false;
        }

        private static bool DateEq(object? a, object? b)
        {
            if (a is DateTime da && b is DateTime db)
            {
                return da.Date == db.Date;
            }
            return false;
        }

        private static string NormalizeName(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            return new string(s.Where(ch => !char.IsWhiteSpace(ch)).ToArray()).ToLowerInvariant();
        }

        private static int Levenshtein(string a, string b)
        {
            if (a == b) return 0;
            if (a.Length == 0) return b.Length;
            if (b.Length == 0) return a.Length;

            var d = new int[a.Length + 1, b.Length + 1];
            for (var i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (var j = 0; j <= b.Length; j++) d[0, j] = j;
            for (var i = 1; i <= a.Length; i++)
            {
                for (var j = 1; j <= b.Length; j++)
                {
                    var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[a.Length, b.Length];
        }
    }
}
