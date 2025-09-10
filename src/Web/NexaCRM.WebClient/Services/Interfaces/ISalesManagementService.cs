using NexaCRM.WebClient.Models;

namespace NexaCRM.WebClient.Services.Interfaces
{
    public interface ISalesManagementService
    {
        // Appointment Management
        System.Threading.Tasks.Task<List<SalesAppointment>> GetAppointmentsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        System.Threading.Tasks.Task<SalesAppointment?> GetAppointmentByIdAsync(int id);
        System.Threading.Tasks.Task<SalesAppointment> CreateAppointmentAsync(SalesAppointment appointment);
        System.Threading.Tasks.Task<SalesAppointment> UpdateAppointmentAsync(SalesAppointment appointment);
        System.Threading.Tasks.Task<bool> DeleteAppointmentAsync(int id);
        System.Threading.Tasks.Task<List<SalesAppointment>> GetAppointmentsByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);

        // Consultation Notes Management
        System.Threading.Tasks.Task<List<ConsultationNote>> GetConsultationNotesAsync(string userId, int? contactId = null);
        System.Threading.Tasks.Task<ConsultationNote?> GetConsultationNoteByIdAsync(int id);
        System.Threading.Tasks.Task<ConsultationNote> CreateConsultationNoteAsync(ConsultationNote note);
        System.Threading.Tasks.Task<ConsultationNote> UpdateConsultationNoteAsync(ConsultationNote note);
        System.Threading.Tasks.Task<bool> DeleteConsultationNoteAsync(int id);
        System.Threading.Tasks.Task<List<ConsultationNote>> GetConsultationNotesByContactAsync(int contactId);
        System.Threading.Tasks.Task<List<ConsultationNote>> SearchConsultationNotesAsync(string userId, string searchTerm);
    }
}