using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Db;
using NexaCRM.UI.Models.Supabase;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;

namespace NexaCRM.Service.Supabase;

public sealed class SupabaseDbDataService : IDbDataService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseDbDataService> _logger;

    public SupabaseDbDataService(SupabaseClientProvider clientProvider, ILogger<SupabaseDbDataService> logger)
    {
        _clientProvider = clientProvider ?? throw new ArgumentNullException(nameof(clientProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<DbCustomer>> GetAllDbListAsync()
    {
        var customers = await LoadAllCustomersAsync().ConfigureAwait(false);
        return customers.Where(customer => !customer.IsArchived).ToList();
    }

    public async Task<IEnumerable<DbCustomer>> GetTeamDbStatusAsync()
    {
        var customers = await LoadAllCustomersAsync().ConfigureAwait(false);
        return customers
            .Where(customer => !customer.IsArchived && !string.IsNullOrWhiteSpace(customer.AssignedTo))
            .ToList();
    }

    public async Task<IEnumerable<DbCustomer>> GetUnassignedDbListAsync()
    {
        var customers = await LoadAllCustomersAsync().ConfigureAwait(false);
        return customers
            .Where(customer => !customer.IsArchived && string.IsNullOrWhiteSpace(customer.AssignedTo))
            .ToList();
    }

    public async Task<IEnumerable<DbCustomer>> GetTodaysAssignedDbAsync()
    {
        var customers = await LoadAllCustomersAsync().ConfigureAwait(false);
        var today = DateTime.Today;
        return customers
            .Where(customer => !customer.IsArchived && customer.AssignedDate.Date == today)
            .ToList();
    }

    public async Task<IEnumerable<DbCustomer>> GetDbDistributionStatusAsync()
    {
        var customers = await LoadAllCustomersAsync().ConfigureAwait(false);
        return customers
            .Where(customer => !customer.IsArchived && !string.IsNullOrWhiteSpace(customer.AssignedTo))
            .ToList();
    }

    public async Task AssignDbToAgentAsync(int contactId, string agentName)
    {
        if (contactId <= 0 || string.IsNullOrWhiteSpace(agentName))
        {
            return;
        }

        var client = await _clientProvider.GetClientAsync().ConfigureAwait(false);
        var record = await GetRecordByContactIdAsync(client, contactId).ConfigureAwait(false);
        if (record is null)
        {
            _logger.LogWarning("Unable to assign DB customer {ContactId} because it does not exist.", contactId);
            return;
        }

        record.AssignedTo = agentName;
        record.AssignedDate = DateTime.UtcNow;
        record.IsArchived = false;
        await UpdateRecordAsync(client, record).ConfigureAwait(false);
    }

    public Task ReassignDbAsync(int contactId, string agentName) => AssignDbToAgentAsync(contactId, agentName);

    public async Task RecallDbAsync(int contactId)
    {
        if (contactId <= 0)
        {
            return;
        }

        var client = await _clientProvider.GetClientAsync().ConfigureAwait(false);
        var record = await GetRecordByContactIdAsync(client, contactId).ConfigureAwait(false);
        if (record is null)
        {
            _logger.LogWarning("Unable to recall DB customer {ContactId} because it does not exist.", contactId);
            return;
        }

        record.AssignedTo = null;
        record.AssignedDate = null;
        await UpdateRecordAsync(client, record).ConfigureAwait(false);
    }

    public async Task<IEnumerable<DbCustomer>> GetNewDbListAsync(string salesAgentName)
    {
        if (string.IsNullOrWhiteSpace(salesAgentName))
        {
            return Array.Empty<DbCustomer>();
        }

        var customers = await LoadAllCustomersAsync().ConfigureAwait(false);
        return customers
            .Where(customer => !customer.IsArchived
                && string.Equals(customer.AssignedTo, salesAgentName, StringComparison.Ordinal)
                && customer.Status == DbStatus.New)
            .ToList();
    }

    public async Task<IEnumerable<DbCustomer>> GetStarredDbListAsync(string salesAgentName)
    {
        if (string.IsNullOrWhiteSpace(salesAgentName))
        {
            return Array.Empty<DbCustomer>();
        }

        var customers = await LoadAllCustomersAsync().ConfigureAwait(false);
        return customers
            .Where(customer => !customer.IsArchived
                && string.Equals(customer.AssignedTo, salesAgentName, StringComparison.Ordinal)
                && customer.IsStarred)
            .ToList();
    }

    public async Task<IEnumerable<DbCustomer>> GetNewlyAssignedDbAsync(string salesAgentName)
    {
        if (string.IsNullOrWhiteSpace(salesAgentName))
        {
            return Array.Empty<DbCustomer>();
        }

        var customers = await LoadAllCustomersAsync().ConfigureAwait(false);
        var today = DateTime.Today;
        return customers
            .Where(customer => !customer.IsArchived
                && string.Equals(customer.AssignedTo, salesAgentName, StringComparison.Ordinal)
                && customer.AssignedDate.Date == today)
            .ToList();
    }

    public async Task<IEnumerable<DbCustomer>> GetMyAssignmentHistoryAsync(string salesAgentName)
    {
        if (string.IsNullOrWhiteSpace(salesAgentName))
        {
            return Array.Empty<DbCustomer>();
        }

        var customers = await LoadAllCustomersAsync().ConfigureAwait(false);
        return customers
            .Where(customer => !customer.IsArchived
                && string.Equals(customer.AssignedTo, salesAgentName, StringComparison.Ordinal))
            .ToList();
    }

    public async Task<IEnumerable<DbCustomer>> GetMyDbListAsync(string agentName)
    {
        if (string.IsNullOrWhiteSpace(agentName))
        {
            return Array.Empty<DbCustomer>();
        }

        var customers = await LoadAllCustomersAsync().ConfigureAwait(false);
        return customers
            .Where(customer => !customer.IsArchived
                && string.Equals(customer.AssignedTo, agentName, StringComparison.Ordinal))
            .ToList();
    }

    public async Task ArchiveCustomersAsync(IEnumerable<int> contactIds)
    {
        ArgumentNullException.ThrowIfNull(contactIds);

        var ids = contactIds.Distinct().Where(id => id > 0).ToArray();
        if (ids.Length == 0)
        {
            return;
        }

        var client = await _clientProvider.GetClientAsync().ConfigureAwait(false);
        var records = await GetRecordsByContactIdsAsync(client, ids).ConfigureAwait(false);
        if (records.Count == 0)
        {
            return;
        }

        foreach (var record in records)
        {
            record.IsArchived = true;
        }

        await Task.WhenAll(records.Select(record => UpdateRecordAsync(client, record))).ConfigureAwait(false);
    }

    public async Task DeleteCustomersAsync(IEnumerable<int> contactIds)
    {
        ArgumentNullException.ThrowIfNull(contactIds);

        var ids = contactIds.Distinct().Where(id => id > 0).ToArray();
        if (ids.Length == 0)
        {
            return;
        }

        var client = await _clientProvider.GetClientAsync().ConfigureAwait(false);
        await client
            .From<DbCustomerRecord>()
            .Filter(record => record.ContactId, PostgrestOperator.In, ids)
            .Delete()
            .ConfigureAwait(false);
    }

    public async Task MergeCustomersAsync(int primaryContactId, IEnumerable<int> duplicateContactIds)
    {
        ArgumentNullException.ThrowIfNull(duplicateContactIds);

        if (primaryContactId <= 0)
        {
            return;
        }

        var duplicateIds = duplicateContactIds.Distinct().Where(id => id > 0 && id != primaryContactId).ToArray();
        if (duplicateIds.Length == 0)
        {
            return;
        }

        var client = await _clientProvider.GetClientAsync().ConfigureAwait(false);
        var relevantIds = duplicateIds.Concat(new[] { primaryContactId }).ToArray();
        var records = await GetRecordsByContactIdsAsync(client, relevantIds).ConfigureAwait(false);
        if (records.Count == 0)
        {
            return;
        }

        var primary = records.FirstOrDefault(record => record.ContactId == primaryContactId);
        if (primary is null)
        {
            await DeleteCustomersAsync(duplicateIds).ConfigureAwait(false);
            return;
        }

        var duplicates = records.Where(record => duplicateIds.Contains(record.ContactId)).ToList();
        if (duplicates.Count == 0)
        {
            return;
        }

        MergeIntoPrimary(primary, duplicates);
        await UpdateRecordAsync(client, primary).ConfigureAwait(false);
        await DeleteCustomersAsync(duplicateIds).ConfigureAwait(false);
    }

    public async Task UpdateCustomerPartialAsync(int contactId, DbCustomer patch, bool overwriteEmptyOnly = false)
    {
        if (contactId <= 0)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(patch);

        var client = await _clientProvider.GetClientAsync().ConfigureAwait(false);
        var record = await GetRecordByContactIdAsync(client, contactId).ConfigureAwait(false);
        if (record is null)
        {
            return;
        }

        ApplyPatch(record, patch, overwriteEmptyOnly);
        await UpdateRecordAsync(client, record).ConfigureAwait(false);
    }

    private async Task<List<DbCustomer>> LoadAllCustomersAsync()
    {
        try
        {
            _logger.LogInformation("Loading DB customers from Supabase...");
            var client = await _clientProvider.GetClientAsync().ConfigureAwait(false);
            _logger.LogInformation("Supabase client obtained successfully");
            
            _logger.LogInformation("Executing query on db_customers table...");
            var response = await client.From<DbCustomerRecord>().Get().ConfigureAwait(false);
            
            _logger.LogInformation("Query executed. Response received");
            _logger.LogInformation("Response Model Count: {Count}", response.Models?.Count ?? 0);
            _logger.LogInformation("Response Content: {Content}", response.Content ?? "null");
            
            var records = response.Models ?? new List<DbCustomerRecord>();
            _logger.LogInformation("Loaded {Count} DB customer records from Supabase", records.Count);
            
            if (records.Count == 0)
            {
                _logger.LogWarning("No DB customer records found in database. Check if data exists and RLS policies are correct.");
                _logger.LogWarning("Response was empty. This could mean: 1) No data in table, 2) RLS blocking access, 3) Wrong table name");
            }
            else
            {
                _logger.LogInformation("First record sample - ContactId: {ContactId}, Name: {Name}, Status: {Status}", 
                    records[0].ContactId, records[0].CustomerName, records[0].Status);
            }
            
            var customers = records.Select(MapToCustomer).ToList();
            _logger.LogInformation("Mapped {Count} DB customers successfully", customers.Count);
            
            return customers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load DB customers from Supabase. Error: {Message}", ex.Message);
            throw;
        }
    }

    private static async Task<DbCustomerRecord?> GetRecordByContactIdAsync(global::Supabase.Client client, int contactId)
    {
        var response = await client
            .From<DbCustomerRecord>()
            .Filter(record => record.ContactId, PostgrestOperator.Equals, contactId)
            .Get()
            .ConfigureAwait(false);

        return response.Models.FirstOrDefault();
    }

    private static async Task<List<DbCustomerRecord>> GetRecordsByContactIdsAsync(global::Supabase.Client client, IReadOnlyCollection<int> contactIds)
    {
        if (contactIds.Count == 0)
        {
            return new List<DbCustomerRecord>();
        }

        var response = await client
            .From<DbCustomerRecord>()
            .Filter(record => record.ContactId, PostgrestOperator.In, contactIds.ToArray())
            .Get()
            .ConfigureAwait(false);

        return response.Models ?? new List<DbCustomerRecord>();
    }

    private static async Task UpdateRecordAsync(global::Supabase.Client client, DbCustomerRecord record)
    {
        await client
            .From<DbCustomerRecord>()
            .Where(r => r.Id == record.Id)
            .Update(record);
    }

    private static DbCustomer MapToCustomer(DbCustomerRecord record)
    {
        var assignedDate = NormalizeTimestamp(record.AssignedDate, record.CreatedAt);
        var lastContactDate = NormalizeTimestamp(record.LastContactDate, record.UpdatedAt ?? record.CreatedAt);

        return new DbCustomer
        {
            Id = record.Id.ToString(CultureInfo.InvariantCulture),
            ContactId = record.ContactId,
            CustomerName = record.CustomerName,
            ContactNumber = record.ContactNumber,
            Group = record.CustomerGroup,
            AssignedTo = record.AssignedTo,
            Assigner = record.Assigner,
            AssignedDate = assignedDate,
            Status = ParseStatus(record.Status),
            LastContactDate = lastContactDate,
            IsStarred = record.IsStarred ?? false,
            IsArchived = record.IsArchived ?? false,
            Gender = record.Gender,
            Address = record.Address,
            JobTitle = record.JobTitle,
            MaritalStatus = record.MaritalStatus,
            ProofNumber = record.ProofNumber,
            DbPrice = record.DbPrice,
            Headquarters = record.Headquarters,
            InsuranceName = record.InsuranceName,
            CarJoinDate = record.CarJoinDate,
            Notes = record.Notes
        };
    }

    private static DateTime NormalizeTimestamp(DateTime? value, DateTime? fallback)
    {
        if (value.HasValue)
        {
            return Normalize(value.Value);
        }

        return fallback.HasValue ? Normalize(fallback.Value) : Normalize(DateTime.UtcNow);

        static DateTime Normalize(DateTime input)
        {
            if (input.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(input, DateTimeKind.Utc).ToLocalTime();
            }

            return input.Kind == DateTimeKind.Utc ? input.ToLocalTime() : input;
        }
    }

    private static DbStatus ParseStatus(string? status)
    {
        if (Enum.TryParse(status, true, out DbStatus parsed))
        {
            return parsed;
        }

        return DbStatus.New;
    }

    private static void MergeIntoPrimary(DbCustomerRecord primary, IReadOnlyCollection<DbCustomerRecord> duplicates)
    {
        if (duplicates.Count == 0)
        {
            return;
        }

        primary.Gender = ResolveString(primary.Gender, duplicates, d => d.Gender);
        primary.Address = ResolveString(primary.Address, duplicates, d => d.Address);
        primary.JobTitle = ResolveString(primary.JobTitle, duplicates, d => d.JobTitle);
        primary.MaritalStatus = ResolveString(primary.MaritalStatus, duplicates, d => d.MaritalStatus);
        primary.ProofNumber = ResolveString(primary.ProofNumber, duplicates, d => d.ProofNumber);
        primary.Headquarters = ResolveString(primary.Headquarters, duplicates, d => d.Headquarters);
        primary.InsuranceName = ResolveString(primary.InsuranceName, duplicates, d => d.InsuranceName);

        if (primary.DbPrice is null)
        {
            primary.DbPrice = ResolveValue(duplicates, d => d.DbPrice);
        }

        if (primary.CarJoinDate is null)
        {
            primary.CarJoinDate = ResolveDate(duplicates, d => d.CarJoinDate);
        }

        var noteSegments = new List<string>();
        if (!string.IsNullOrWhiteSpace(primary.Notes))
        {
            noteSegments.Add(primary.Notes.Trim());
        }

        foreach (var note in duplicates.Select(d => d.Notes).Where(s => !string.IsNullOrWhiteSpace(s)))
        {
            noteSegments.Add(note!.Trim());
        }

        if (noteSegments.Count > 0)
        {
            primary.Notes = string.Join(" | ", noteSegments.Distinct(StringComparer.OrdinalIgnoreCase));
        }

        static string? ResolveString(string? current, IEnumerable<DbCustomerRecord> items, Func<DbCustomerRecord, string?> selector)
        {
            if (!string.IsNullOrWhiteSpace(current))
            {
                return current;
            }

            foreach (var record in items.OrderByDescending(OrderByDate))
            {
                var value = selector(record);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return current;
        }

        static decimal? ResolveValue(IEnumerable<DbCustomerRecord> items, Func<DbCustomerRecord, decimal?> selector)
        {
            foreach (var record in items.OrderByDescending(OrderByDate))
            {
                var value = selector(record);
                if (value.HasValue)
                {
                    return value;
                }
            }

            return null;
        }

        static DateTime? ResolveDate(IEnumerable<DbCustomerRecord> items, Func<DbCustomerRecord, DateTime?> selector)
        {
            foreach (var record in items.OrderByDescending(OrderByDate))
            {
                var value = selector(record);
                if (value.HasValue)
                {
                    return value;
                }
            }

            return null;
        }

        static DateTime OrderByDate(DbCustomerRecord record)
        {
            return record.AssignedDate ?? record.UpdatedAt ?? record.CreatedAt ?? DateTime.MinValue;
        }
    }

    private static void ApplyPatch(DbCustomerRecord target, DbCustomer patch, bool overwriteEmptyOnly)
    {
        if (ShouldAssign(patch.Gender, target.Gender, overwriteEmptyOnly))
        {
            target.Gender = patch.Gender;
        }

        if (ShouldAssign(patch.Address, target.Address, overwriteEmptyOnly))
        {
            target.Address = patch.Address;
        }

        if (ShouldAssign(patch.JobTitle, target.JobTitle, overwriteEmptyOnly))
        {
            target.JobTitle = patch.JobTitle;
        }

        if (ShouldAssign(patch.MaritalStatus, target.MaritalStatus, overwriteEmptyOnly))
        {
            target.MaritalStatus = patch.MaritalStatus;
        }

        if (ShouldAssign(patch.ProofNumber, target.ProofNumber, overwriteEmptyOnly))
        {
            target.ProofNumber = patch.ProofNumber;
        }

        if (patch.DbPrice.HasValue && (!target.DbPrice.HasValue || !overwriteEmptyOnly))
        {
            target.DbPrice = patch.DbPrice;
        }

        if (ShouldAssign(patch.Headquarters, target.Headquarters, overwriteEmptyOnly))
        {
            target.Headquarters = patch.Headquarters;
        }

        if (ShouldAssign(patch.InsuranceName, target.InsuranceName, overwriteEmptyOnly))
        {
            target.InsuranceName = patch.InsuranceName;
        }

        if (patch.CarJoinDate.HasValue && (!target.CarJoinDate.HasValue || !overwriteEmptyOnly))
        {
            target.CarJoinDate = patch.CarJoinDate;
        }

        if (ShouldAssign(patch.Notes, target.Notes, overwriteEmptyOnly))
        {
            target.Notes = patch.Notes;
        }

        if (ShouldAssign(patch.CustomerName, target.CustomerName, overwriteEmptyOnly))
        {
            target.CustomerName = patch.CustomerName;
        }

        if (ShouldAssign(patch.ContactNumber, target.ContactNumber, overwriteEmptyOnly))
        {
            target.ContactNumber = patch.ContactNumber;
        }

        if (ShouldAssign(patch.Group, target.CustomerGroup, overwriteEmptyOnly))
        {
            target.CustomerGroup = patch.Group;
        }

        if (ShouldAssign(patch.AssignedTo, target.AssignedTo, overwriteEmptyOnly))
        {
            target.AssignedTo = patch.AssignedTo;
        }

        if (!overwriteEmptyOnly || target.AssignedDate == default)
        {
            target.AssignedDate = patch.AssignedDate == default ? target.AssignedDate : patch.AssignedDate;
        }

        if (!overwriteEmptyOnly || target.LastContactDate == default)
        {
            target.LastContactDate = patch.LastContactDate == default ? target.LastContactDate : patch.LastContactDate;
        }

        if (!overwriteEmptyOnly || target.Status is null)
        {
            target.Status = patch.Status.ToString();
        }

        target.IsStarred = patch.IsStarred;
    }

    private static bool ShouldAssign(string? source, string? destination, bool overwriteEmptyOnly)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return false;
        }

        return !overwriteEmptyOnly || string.IsNullOrWhiteSpace(destination);
    }
}
