using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexaCRM.WebClient.Models;
using NexaCRM.WebClient.Models.Enums;
using NexaCRM.WebClient.Models.Supabase;
using NexaCRM.WebClient.Services.Interfaces;
using PostgrestOperator = Supabase.Postgrest.Constants.Operator;
using PostgrestOrdering = Supabase.Postgrest.Constants.Ordering;

namespace NexaCRM.WebClient.Services;

public sealed class SupabaseSalesManagementService : ISalesManagementService
{
    private readonly SupabaseClientProvider _clientProvider;
    private readonly ILogger<SupabaseSalesManagementService> _logger;

    public SupabaseSalesManagementService(
        SupabaseClientProvider clientProvider,
        ILogger<SupabaseSalesManagementService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<List<SalesAppointment>> GetAppointmentsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var userGuid = ParseUserId(userId);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var query = client.From<SalesAppointmentRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userGuid)
                .Order(x => x.StartDateTime, PostgrestOrdering.Ascending);

            if (startDate.HasValue)
            {
                query = query.Filter(x => x.StartDateTime, PostgrestOperator.GreaterThanOrEqual, ToTimestamp(startDate.Value));
            }

            if (endDate.HasValue)
            {
                query = query.Filter(x => x.StartDateTime, PostgrestOperator.LessThanOrEqual, ToTimestamp(endDate.Value));
            }

            var response = await query.Get();
            var records = response.Models ?? new List<SalesAppointmentRecord>();
            return records.Select(MapToAppointment).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load appointments for user {UserId} from Supabase.", userId);
            throw;
        }
    }

    public async Task<SalesAppointment?> GetAppointmentByIdAsync(int id)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<SalesAppointmentRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Get();

            var record = response.Models.FirstOrDefault();
            return record is null ? null : MapToAppointment(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load appointment {AppointmentId} from Supabase.", id);
            throw;
        }
    }

    public async Task<SalesAppointment> CreateAppointmentAsync(SalesAppointment appointment)
    {
        ArgumentNullException.ThrowIfNull(appointment);
        var userGuid = ParseUserId(appointment.UserId);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var record = ToRecord(appointment, userGuid);
            record.CreatedAt = DateTime.UtcNow;
            record.UpdatedAt = record.CreatedAt;

            var response = await client.From<SalesAppointmentRecord>()
                .Insert(record);

            return MapToAppointment(response.Models.FirstOrDefault() ?? record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create appointment for user {UserId} in Supabase.", appointment.UserId);
            throw;
        }
    }

    public async Task<SalesAppointment> UpdateAppointmentAsync(SalesAppointment appointment)
    {
        ArgumentNullException.ThrowIfNull(appointment);
        var userGuid = ParseUserId(appointment.UserId);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var record = ToRecord(appointment, userGuid);
            record.UpdatedAt = DateTime.UtcNow;

            var response = await client.From<SalesAppointmentRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, record.Id)
                .Update(record);

            return MapToAppointment(response.Models.FirstOrDefault() ?? record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update appointment {AppointmentId} in Supabase.", appointment.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAppointmentAsync(int id)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var existing = await client.From<SalesAppointmentRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Get();

            if (existing.Models.FirstOrDefault() is null)
            {
                return false;
            }

            await client.From<SalesAppointmentRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Delete();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete appointment {AppointmentId} from Supabase.", id);
            throw;
        }
    }

    public Task<List<SalesAppointment>> GetAppointmentsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return GetAppointmentsAsync(userId, startDate, endDate);
    }

    public async Task<List<SalesAppointment>> CheckAppointmentConflictsAsync(string userId, DateTime startDateTime, DateTime endDateTime, int? excludeAppointmentId = null)
    {
        var userGuid = ParseUserId(userId);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var query = client.From<SalesAppointmentRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userGuid)
                .Filter(x => x.StartDateTime, PostgrestOperator.LessThan, ToTimestamp(endDateTime))
                .Filter(x => x.EndDateTime, PostgrestOperator.GreaterThan, ToTimestamp(startDateTime));

            if (excludeAppointmentId.HasValue)
            {
                query = query.Filter(x => x.Id, PostgrestOperator.NotEqual, excludeAppointmentId.Value);
            }

            var response = await query.Get();
            var records = response.Models ?? new List<SalesAppointmentRecord>();
            return records.Select(MapToAppointment).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check appointment conflicts for user {UserId} in Supabase.", userId);
            throw;
        }
    }

    public async Task<List<ConsultationNote>> GetConsultationNotesAsync(string userId, int? contactId = null)
    {
        var userGuid = ParseUserId(userId);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var query = client.From<ConsultationNoteRecord>()
                .Filter(x => x.UserId, PostgrestOperator.Equals, userGuid)
                .Order(x => x.CreatedAt, PostgrestOrdering.Descending);

            if (contactId.HasValue)
            {
                query = query.Filter(x => x.ContactId, PostgrestOperator.Equals, contactId.Value);
            }

            var response = await query.Get();
            var records = response.Models ?? new List<ConsultationNoteRecord>();
            return records.Select(MapToConsultationNote).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load consultation notes for user {UserId} from Supabase.", userId);
            throw;
        }
    }

    public async Task<ConsultationNote?> GetConsultationNoteByIdAsync(int id)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<ConsultationNoteRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Get();

            var record = response.Models.FirstOrDefault();
            return record is null ? null : MapToConsultationNote(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load consultation note {NoteId} from Supabase.", id);
            throw;
        }
    }

    public async Task<ConsultationNote> CreateConsultationNoteAsync(ConsultationNote note)
    {
        ArgumentNullException.ThrowIfNull(note);
        var userGuid = ParseUserId(note.UserId);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var record = ToRecord(note, userGuid);
            record.CreatedAt = DateTime.UtcNow;
            record.UpdatedAt = record.CreatedAt;

            var response = await client.From<ConsultationNoteRecord>()
                .Insert(record);

            return MapToConsultationNote(response.Models.FirstOrDefault() ?? record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create consultation note for user {UserId} in Supabase.", note.UserId);
            throw;
        }
    }

    public async Task<ConsultationNote> UpdateConsultationNoteAsync(ConsultationNote note)
    {
        ArgumentNullException.ThrowIfNull(note);
        var userGuid = ParseUserId(note.UserId);

        try
        {
            var client = await _clientProvider.GetClientAsync();
            var record = ToRecord(note, userGuid);
            record.UpdatedAt = DateTime.UtcNow;

            var response = await client.From<ConsultationNoteRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, record.Id)
                .Update(record);

            return MapToConsultationNote(response.Models.FirstOrDefault() ?? record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update consultation note {NoteId} in Supabase.", note.Id);
            throw;
        }
    }

    public async Task<bool> DeleteConsultationNoteAsync(int id)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var existing = await client.From<ConsultationNoteRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Get();

            if (existing.Models.FirstOrDefault() is null)
            {
                return false;
            }

            await client.From<ConsultationNoteRecord>()
                .Filter(x => x.Id, PostgrestOperator.Equals, id)
                .Delete();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete consultation note {NoteId} from Supabase.", id);
            throw;
        }
    }

    public async Task<List<ConsultationNote>> GetConsultationNotesByContactAsync(int contactId)
    {
        try
        {
            var client = await _clientProvider.GetClientAsync();
            var response = await client.From<ConsultationNoteRecord>()
                .Filter(x => x.ContactId, PostgrestOperator.Equals, contactId)
                .Order(x => x.CreatedAt, PostgrestOrdering.Descending)
                .Get();

            var records = response.Models ?? new List<ConsultationNoteRecord>();
            return records.Select(MapToConsultationNote).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load consultation notes for contact {ContactId} from Supabase.", contactId);
            throw;
        }
    }

    public async Task<List<ConsultationNote>> SearchConsultationNotesAsync(string userId, string searchTerm)
    {
        try
        {
            var notes = await GetConsultationNotesAsync(userId);
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return notes;
            }

            var comparer = StringComparison.OrdinalIgnoreCase;
            return notes.Where(note =>
                    (!string.IsNullOrEmpty(note.Title) && note.Title.Contains(searchTerm, comparer)) ||
                    (!string.IsNullOrEmpty(note.Content) && note.Content.Contains(searchTerm, comparer)) ||
                    (!string.IsNullOrEmpty(note.Tags) && note.Tags.Contains(searchTerm, comparer)))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search consultation notes for user {UserId} in Supabase.", userId);
            throw;
        }
    }

    private static SalesAppointment MapToAppointment(SalesAppointmentRecord record)
    {
        return new SalesAppointment
        {
            Id = record.Id,
            Title = record.Title,
            Description = record.Description ?? string.Empty,
            StartDateTime = record.StartDateTime,
            EndDateTime = record.EndDateTime,
            ContactId = record.ContactId,
            ContactName = record.ContactName,
            ContactCompany = record.ContactCompany,
            Type = ParseEnum(record.Type, AppointmentType.Meeting),
            Status = ParseEnum(record.Status, AppointmentStatus.Scheduled),
            UserId = record.UserId.ToString(),
            Location = record.Location ?? string.Empty,
            Notes = record.Notes ?? string.Empty,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt
        };
    }

    private static ConsultationNote MapToConsultationNote(ConsultationNoteRecord record)
    {
        return new ConsultationNote
        {
            Id = record.Id,
            ContactId = record.ContactId,
            ContactName = record.ContactName,
            Title = record.Title,
            Content = record.Content,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt,
            UserId = record.UserId.ToString(),
            Tags = record.Tags,
            Priority = ParseEnum(record.Priority, ConsultationPriority.Medium),
            IsFollowUpRequired = record.IsFollowUpRequired,
            FollowUpDate = record.FollowUpDate
        };
    }

    private static SalesAppointmentRecord ToRecord(SalesAppointment appointment, Guid userId)
    {
        return new SalesAppointmentRecord
        {
            Id = appointment.Id,
            Title = appointment.Title,
            Description = appointment.Description,
            StartDateTime = appointment.StartDateTime,
            EndDateTime = appointment.EndDateTime,
            ContactId = appointment.ContactId,
            ContactName = appointment.ContactName,
            ContactCompany = appointment.ContactCompany,
            Type = appointment.Type.ToString(),
            Status = appointment.Status.ToString(),
            UserId = userId,
            Location = appointment.Location,
            Notes = appointment.Notes
        };
    }

    private static ConsultationNoteRecord ToRecord(ConsultationNote note, Guid userId)
    {
        return new ConsultationNoteRecord
        {
            Id = note.Id,
            ContactId = note.ContactId,
            ContactName = note.ContactName,
            Title = note.Title,
            Content = note.Content,
            UserId = userId,
            Tags = note.Tags,
            Priority = note.Priority.ToString(),
            IsFollowUpRequired = note.IsFollowUpRequired,
            FollowUpDate = note.FollowUpDate
        };
    }

    private static TEnum ParseEnum<TEnum>(string? value, TEnum fallback) where TEnum : struct
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        return Enum.TryParse(value, ignoreCase: true, out TEnum parsed)
            ? parsed
            : fallback;
    }

    private static Guid ParseUserId(string userId)
    {
        if (!Guid.TryParse(userId, out var guid))
        {
            throw new ArgumentException("The user identifier must be a valid GUID.", nameof(userId));
        }

        return guid;
    }

    private static string ToTimestamp(DateTime value)
    {
        return value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
    }
}
