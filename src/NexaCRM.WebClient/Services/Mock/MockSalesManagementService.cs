using NexaCRM.UI.Models;
using NexaCRM.UI.Models.Enums;
using NexaCRM.UI.Services.Interfaces;

namespace NexaCRM.WebClient.Services.Mock
{
    public class MockSalesManagementService : ISalesManagementService
    {
        private readonly List<SalesAppointment> _appointments;
        private readonly List<ConsultationNote> _consultationNotes;
        private int _appointmentIdCounter = 1;
        private int _noteIdCounter = 1;

        public MockSalesManagementService()
            : this(null, null)
        {
        }

        public MockSalesManagementService(List<SalesAppointment>? appointments, List<ConsultationNote>? consultationNotes = null)
        {
            _appointments = appointments ?? GenerateSampleAppointments();
            _consultationNotes = consultationNotes ?? GenerateSampleConsultationNotes();
            _appointmentIdCounter = _appointments.Count + 1;
            _noteIdCounter = _consultationNotes.Count + 1;
        }

        // Appointment Management
        public System.Threading.Tasks.Task<List<SalesAppointment>> GetAppointmentsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _appointments.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(a => a.UserId == userId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.StartDateTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.StartDateTime <= endDate.Value);
            }

            return System.Threading.Tasks.Task.FromResult(query.OrderBy(a => a.StartDateTime).ToList());
        }

        public System.Threading.Tasks.Task<SalesAppointment?> GetAppointmentByIdAsync(int id)
        {
            var appointment = _appointments.FirstOrDefault(a => a.Id == id);
            return System.Threading.Tasks.Task.FromResult(appointment);
        }

        public System.Threading.Tasks.Task<SalesAppointment> CreateAppointmentAsync(SalesAppointment appointment)
        {
            appointment.Id = _appointmentIdCounter++;
            appointment.CreatedAt = DateTime.Now;
            appointment.UpdatedAt = DateTime.Now;
            _appointments.Add(appointment);
            return System.Threading.Tasks.Task.FromResult(appointment);
        }

        public System.Threading.Tasks.Task<SalesAppointment> UpdateAppointmentAsync(SalesAppointment appointment)
        {
            var existingAppointment = _appointments.FirstOrDefault(a => a.Id == appointment.Id);
            if (existingAppointment != null)
            {
                var index = _appointments.IndexOf(existingAppointment);
                appointment.UpdatedAt = DateTime.Now;
                _appointments[index] = appointment;
            }
            return System.Threading.Tasks.Task.FromResult(appointment);
        }

        public System.Threading.Tasks.Task<bool> DeleteAppointmentAsync(int id)
        {
            var appointment = _appointments.FirstOrDefault(a => a.Id == id);
            if (appointment != null)
            {
                _appointments.Remove(appointment);
                return System.Threading.Tasks.Task.FromResult(true);
            }
            return System.Threading.Tasks.Task.FromResult(false);
        }

        public System.Threading.Tasks.Task<List<SalesAppointment>> GetAppointmentsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var appointments = _appointments
                .Where(a => a.UserId == userId && a.StartDateTime >= startDate && a.StartDateTime <= endDate)
                .OrderBy(a => a.StartDateTime)
                .ToList();
            return System.Threading.Tasks.Task.FromResult(appointments);
        }

        // Conflict Detection
        public System.Threading.Tasks.Task<List<SalesAppointment>> CheckAppointmentConflictsAsync(string userId, DateTime startDateTime, DateTime endDateTime, int? excludeAppointmentId = null)
        {
            var conflictingAppointments = _appointments
                .Where(a => a.UserId == userId && 
                           (excludeAppointmentId == null || a.Id != excludeAppointmentId) &&
                           ((a.StartDateTime < endDateTime && a.EndDateTime > startDateTime) ||  // Overlapping
                            (startDateTime < a.EndDateTime && endDateTime > a.StartDateTime)))   // Alternative overlap check
                .OrderBy(a => a.StartDateTime)
                .ToList();
            
            return System.Threading.Tasks.Task.FromResult(conflictingAppointments);
        }

        // Consultation Notes Management
        public System.Threading.Tasks.Task<List<ConsultationNote>> GetConsultationNotesAsync(string userId, int? contactId = null)
        {
            var query = _consultationNotes.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(n => n.UserId == userId);
            }

            if (contactId.HasValue)
            {
                query = query.Where(n => n.ContactId == contactId.Value);
            }

            return System.Threading.Tasks.Task.FromResult(query.OrderByDescending(n => n.CreatedAt).ToList());
        }

        public System.Threading.Tasks.Task<ConsultationNote?> GetConsultationNoteByIdAsync(int id)
        {
            var note = _consultationNotes.FirstOrDefault(n => n.Id == id);
            return System.Threading.Tasks.Task.FromResult(note);
        }

        public System.Threading.Tasks.Task<ConsultationNote> CreateConsultationNoteAsync(ConsultationNote note)
        {
            note.Id = _noteIdCounter++;
            note.CreatedAt = DateTime.Now;
            note.UpdatedAt = DateTime.Now;
            _consultationNotes.Add(note);
            return System.Threading.Tasks.Task.FromResult(note);
        }

        public System.Threading.Tasks.Task<ConsultationNote> UpdateConsultationNoteAsync(ConsultationNote note)
        {
            var existingNote = _consultationNotes.FirstOrDefault(n => n.Id == note.Id);
            if (existingNote != null)
            {
                var index = _consultationNotes.IndexOf(existingNote);
                note.UpdatedAt = DateTime.Now;
                _consultationNotes[index] = note;
            }
            return System.Threading.Tasks.Task.FromResult(note);
        }

        public System.Threading.Tasks.Task<bool> DeleteConsultationNoteAsync(int id)
        {
            var note = _consultationNotes.FirstOrDefault(n => n.Id == id);
            if (note != null)
            {
                _consultationNotes.Remove(note);
                return System.Threading.Tasks.Task.FromResult(true);
            }
            return System.Threading.Tasks.Task.FromResult(false);
        }

        public System.Threading.Tasks.Task<List<ConsultationNote>> GetConsultationNotesByContactAsync(int contactId)
        {
            var notes = _consultationNotes
                .Where(n => n.ContactId == contactId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
            return System.Threading.Tasks.Task.FromResult(notes);
        }

        public System.Threading.Tasks.Task<List<ConsultationNote>> SearchConsultationNotesAsync(string userId, string searchTerm)
        {
            var notes = _consultationNotes
                .Where(n => n.UserId == userId && 
                           (n.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            n.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            (n.ContactName != null && n.ContactName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))))
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
            return System.Threading.Tasks.Task.FromResult(notes);
        }

        // Sample data generation
        private List<SalesAppointment> GenerateSampleAppointments()
        {
            var appointments = new List<SalesAppointment>();
            var appointmentTypes = Enum.GetValues<AppointmentType>();
            var statuses = Enum.GetValues<AppointmentStatus>();

            for (int i = 1; i <= 10; i++)
            {
                var startDate = DateTime.Today.AddDays(i - 7).AddHours(9 + (i % 8));
                appointments.Add(new SalesAppointment
                {
                    Id = i,
                    Title = $"Meeting with Client {i}",
                    Description = $"Sales discussion about product requirements and pricing for client {i}",
                    StartDateTime = startDate,
                    EndDateTime = startDate.AddHours(1),
                    ContactId = i,
                    ContactName = $"John Client {i}",
                    ContactCompany = $"Company {i} Ltd",
                    Type = appointmentTypes[i % appointmentTypes.Length],
                    Status = statuses[i % statuses.Length],
                    UserId = "user1",
                    Location = i % 2 == 0 ? "Office Conference Room" : "Client Site",
                    Notes = $"Preparation notes for meeting {i}",
                    CreatedAt = DateTime.Now.AddDays(-i),
                    UpdatedAt = DateTime.Now.AddDays(-(i % 5))
                });
            }

            return appointments;
        }

        private List<ConsultationNote> GenerateSampleConsultationNotes()
        {
            var notes = new List<ConsultationNote>();
            var priorities = Enum.GetValues<ConsultationPriority>();

            for (int i = 1; i <= 15; i++)
            {
                notes.Add(new ConsultationNote
                {
                    Id = i,
                    ContactId = (i % 10) + 1,
                    ContactName = $"John Client {(i % 10) + 1}",
                    Title = $"Consultation Note {i}",
                    Content = $"Detailed consultation notes for client meeting {i}. Discussed product requirements, budget constraints, and timeline expectations. Client showed interest in premium features.",
                    CreatedAt = DateTime.Now.AddDays(-i),
                    UpdatedAt = DateTime.Now.AddDays(-(i % 30)),
                    UserId = "user1",
                    Tags = i % 3 == 0 ? "important,follow-up" : i % 2 == 0 ? "pricing,negotiation" : "requirements,demo",
                    Priority = priorities[i % priorities.Length],
                    IsFollowUpRequired = i % 4 == 0,
                    FollowUpDate = i % 4 == 0 ? DateTime.Today.AddDays(i) : null
                });
            }

            return notes;
        }
    }
}