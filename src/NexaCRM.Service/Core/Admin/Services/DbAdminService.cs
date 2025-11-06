using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexaCRM.Services.Admin.Interfaces;
using NexaCRM.Services.Admin.Models.Db;

namespace NexaCRM.Services.Admin;

public sealed class DbAdminService : IDbAdminService
{
    private static readonly string[] DefaultExportFields =
    {
        nameof(DbCustomer.CustomerName),
        nameof(DbCustomer.ContactNumber),
        nameof(DbCustomer.Group),
        nameof(DbCustomer.AssignedTo),
        nameof(DbCustomer.AssignedDate),
        nameof(DbCustomer.Status),
        nameof(DbCustomer.LastContactDate)
    };

    private readonly IDbDataService _dbDataService;

    public DbAdminService(IDbDataService dbDataService)
    {
        _dbDataService = dbDataService ?? throw new ArgumentNullException(nameof(dbDataService));
    }

    public Task DeleteEntryAsync(int id)
    {
        if (id <= 0)
        {
            return Task.CompletedTask;
        }

        return _dbDataService.DeleteCustomersAsync(new[] { id });
    }

    public async Task<byte[]> ExportToExcelAsync(DbExportSettings settings, DbSearchCriteria? criteria = null)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var customers = await LoadCustomersAsync(criteria);
        if (customers.Count == 0)
        {
            return Array.Empty<byte>();
        }

        var fields = (settings.Fields?.Count > 0 ? settings.Fields : DefaultExportFields)
            .Select(field => field?.Trim())
            .Where(field => !string.IsNullOrWhiteSpace(field))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (fields.Count == 0)
        {
            fields = DefaultExportFields.ToList();
        }

        var builder = new StringBuilder();
        builder.AppendLine(string.Join(',', fields.Select(EscapeCsv)));

        foreach (var customer in customers.OrderBy(c => c.CustomerName, StringComparer.OrdinalIgnoreCase))
        {
            var values = fields.Select(field => ResolveFieldValue(field, customer));
            builder.AppendLine(string.Join(',', values.Select(EscapeCsv)));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    public async Task<IEnumerable<DbCustomer>> SearchAsync(DbSearchCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var customers = await LoadCustomersAsync(criteria);
        return customers;
    }

    private async Task<List<DbCustomer>> LoadCustomersAsync(DbSearchCriteria? criteria)
    {
        var customers = await _dbDataService.GetAllDbListAsync();
        var list = customers?.ToList() ?? new List<DbCustomer>();

        IEnumerable<DbCustomer> filtered = criteria?.IncludeArchived == true
            ? list
            : list.Where(c => !c.IsArchived);

        if (criteria?.From is DateTime from)
        {
            filtered = filtered.Where(c => c.AssignedDate.Date >= from.Date);
        }

        if (criteria?.To is DateTime to)
        {
            filtered = filtered.Where(c => c.AssignedDate.Date <= to.Date);
        }

        var searchTerm = criteria?.SearchTerm?.Trim();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedDigits = NormalizeDigits(searchTerm);

            filtered = filtered.Where(customer =>
            {
                if (customer.CustomerName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return true;
                }

                if (customer.ContactNumber?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return true;
                }

                if (normalizedDigits.Length == 0)
                {
                    return false;
                }

                var contactDigits = NormalizeDigits(customer.ContactNumber);
                return contactDigits.Length > 0 && contactDigits.Contains(normalizedDigits, StringComparison.Ordinal);
            });
        }

        if (criteria?.Status is DbStatus status)
        {
            filtered = filtered.Where(c => c.Status == status);
        }

        var materialized = filtered.ToList();

        if (criteria?.CheckDuplicates == true)
        {
            var duplicateIds = materialized
                .GroupBy(c => NormalizeDigits(c.ContactNumber))
                .Where(group => !string.IsNullOrEmpty(group.Key) && group.Count() > 1)
                .SelectMany(group => group.Select(customer => customer.ContactId))
                .ToHashSet();

            materialized = materialized
                .Where(c => duplicateIds.Contains(c.ContactId))
                .ToList();
        }

        return materialized
            .OrderByDescending(c => c.AssignedDate)
            .ThenBy(c => c.CustomerName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string ResolveFieldValue(string field, DbCustomer customer)
    {
        return field switch
        {
            nameof(DbCustomer.CustomerName) => customer.CustomerName ?? string.Empty,
            nameof(DbCustomer.ContactNumber) => customer.ContactNumber ?? string.Empty,
            nameof(DbCustomer.Group) => customer.Group ?? string.Empty,
            nameof(DbCustomer.AssignedTo) => customer.AssignedTo ?? string.Empty,
            nameof(DbCustomer.Assigner) => customer.Assigner ?? string.Empty,
            nameof(DbCustomer.Gender) => customer.Gender ?? string.Empty,
            nameof(DbCustomer.Address) => customer.Address ?? string.Empty,
            nameof(DbCustomer.JobTitle) => customer.JobTitle ?? string.Empty,
            nameof(DbCustomer.MaritalStatus) => customer.MaritalStatus ?? string.Empty,
            nameof(DbCustomer.ProofNumber) => customer.ProofNumber ?? string.Empty,
            nameof(DbCustomer.Headquarters) => customer.Headquarters ?? string.Empty,
            nameof(DbCustomer.InsuranceName) => customer.InsuranceName ?? string.Empty,
            nameof(DbCustomer.Notes) => customer.Notes ?? string.Empty,
            nameof(DbCustomer.AssignedDate) => customer.AssignedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            nameof(DbCustomer.LastContactDate) => customer.LastContactDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            nameof(DbCustomer.CarJoinDate) => customer.CarJoinDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
            nameof(DbCustomer.DbPrice) => customer.DbPrice?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            nameof(DbCustomer.Status) => customer.Status.ToString(),
            nameof(DbCustomer.IsStarred) => customer.IsStarred ? "true" : "false",
            nameof(DbCustomer.IsArchived) => customer.IsArchived ? "true" : "false",
            nameof(DbCustomer.ContactId) => customer.ContactId.ToString(CultureInfo.InvariantCulture),
            nameof(DbCustomer.Id) => customer.Id,
            _ => string.Empty
        };
    }

    private static string EscapeCsv(string? value)
    {
        var content = value ?? string.Empty;
        if (!content.Contains('"') && !content.Contains(',') && !content.Contains('\n') && !content.Contains('\r'))
        {
            return content;
        }

        content = content.Replace("\"", "\"\"");
        return $"\"{content}\"";
    }

    private static string NormalizeDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        Span<char> buffer = stackalloc char[value.Length];
        var index = 0;
        foreach (var ch in value)
        {
            if (char.IsDigit(ch))
            {
                buffer[index++] = ch;
            }
        }

        return index == 0 ? string.Empty : new string(buffer[..index]);
    }
}
