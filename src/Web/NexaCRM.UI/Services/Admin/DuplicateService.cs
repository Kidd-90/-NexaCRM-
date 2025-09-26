using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Db;

namespace NexaCRM.WebClient.Services.Admin;

public sealed class DuplicateService : IDuplicateService
{
    private readonly IDbDataService _db;
    private readonly IDedupeConfigService _config;

    public DuplicateService(IDbDataService db, IDedupeConfigService config)
    {
        _db = db;
        _config = config;
    }

    public async Task<IReadOnlyList<DuplicateGroup>> FindDuplicatesAsync(int withinDays, bool includeFuzzy)
    {
        var all = (await _db.GetAllDbListAsync())?.Where(c => !c.IsArchived).ToList() ?? new List<DbCustomer>();
        var cutoff = DateTime.Today.AddDays(-Math.Clamp(withinDays, 1, 365));
        var recent = all.Where(c => c.AssignedDate.Date >= cutoff).ToList();

        var result = new List<DuplicateGroup>();

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
                var ids = g.Select(x => x.ContactId).ToList();
                if (result.Any(r => ids.All(id => r.ContactIds.Contains(id))))
                {
                    continue;
                }

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

        var threshold = Math.Clamp(_config.ScoreThreshold, 0, 100);
        var filtered = result.Where(r => r.Score >= threshold).ToList();

        return filtered
            .OrderByDescending(r => r.Score)
            .ThenByDescending(r => r.Count)
            .ThenByDescending(r => r.LatestAssigned)
            .ToList();
    }

    public Task ArchiveAsync(DuplicateGroup group) =>
        _db.ArchiveCustomersAsync(group.ContactIds);

    public Task DeleteAsync(DuplicateGroup group) =>
        _db.DeleteCustomersAsync(group.ContactIds);

    public Task MergeAsync(int primaryContactId, IEnumerable<int> duplicateContactIds) =>
        _db.MergeCustomersAsync(primaryContactId, duplicateContactIds);

    private static string NormalizeDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return Regex.Replace(value, "\\D", string.Empty);
    }

    private static string Last4(string? value)
    {
        var digits = NormalizeDigits(value);
        return digits.Length >= 4 ? digits[^4..] : digits;
    }

    private static string NamePrefix2(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = new string(value.Where(ch => !char.IsWhiteSpace(ch)).ToArray());
        return trimmed.Length >= 2 ? trimmed[..2].ToLowerInvariant() : trimmed.ToLowerInvariant();
    }

    private double ComputeScore(List<DbCustomer> group)
    {
        if (group.Count <= 1)
        {
            return 0d;
        }

        var reference = group.OrderByDescending(x => x.AssignedDate).First();

        var fields = new List<(bool enabled, int weight, Func<DbCustomer, object?> selector, Func<object?, object?, bool> equal)>
        {
            (_config.UseGender, _config.WeightGender, c => c.Gender, StrEq),
            (_config.UseAddress, _config.WeightAddress, c => c.Address, StrEq),
            (_config.UseJobTitle, _config.WeightJobTitle, c => c.JobTitle, StrEq),
            (_config.UseMaritalStatus, _config.WeightMaritalStatus, c => c.MaritalStatus, StrEq),
            (_config.UseProofNumber, _config.WeightProofNumber, c => c.ProofNumber, StrEq),
            (_config.UseDbPrice, _config.WeightDbPrice, c => c.DbPrice, NumEq),
            (_config.UseHeadquarters, _config.WeightHeadquarters, c => c.Headquarters, StrEq),
            (_config.UseInsuranceName, _config.WeightInsuranceName, c => c.InsuranceName, StrEq),
            (_config.UseCarJoinDate, _config.WeightCarJoinDate, c => c.CarJoinDate, DateEq),
            (_config.UseNotes, _config.WeightNotes, c => c.Notes, StrEq)
        }.Where(tuple => tuple.enabled && tuple.weight > 0).ToList();

        var totalWeight = fields.Sum(tuple => tuple.weight);
        if (totalWeight == 0)
        {
            return 0d;
        }

        var score = 0d;
        foreach (var candidate in group)
        {
            if (ReferenceEquals(candidate, reference))
            {
                continue;
            }

            var matchWeight = 0;
            foreach (var (enabled, weight, selector, equal) in fields)
            {
                if (!enabled)
                {
                    continue;
                }

                if (equal(selector(reference), selector(candidate)))
                {
                    matchWeight += weight;
                }
            }

            var ratio = (double)matchWeight / totalWeight;
            score += ratio * 100d;
        }

        return Math.Round(score / (group.Count - 1), 2);
    }

    private static bool StrEq(object? a, object? b) => string.Equals(a?.ToString(), b?.ToString(), StringComparison.OrdinalIgnoreCase);

    private static bool NumEq(object? a, object? b)
    {
        if (a is null || b is null)
        {
            return false;
        }

        return decimal.TryParse(a.ToString(), out var da) && decimal.TryParse(b.ToString(), out var db) && da == db;
    }

    private static bool DateEq(object? a, object? b)
    {
        if (a is DateTime da && b is DateTime db)
        {
            return da.Date == db.Date;
        }

        if (DateTime.TryParse(a?.ToString(), out var parsedA) && DateTime.TryParse(b?.ToString(), out var parsedB))
        {
            return parsedA.Date == parsedB.Date;
        }

        return false;
    }
}
