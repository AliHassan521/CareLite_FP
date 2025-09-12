using CareLite.Models.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareLite.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
    Task<Appointment> CreateAppointmentAsync(Appointment appointment, int createdByUserId);
    Task<Appointment> UpdateAppointmentAsync(Appointment appointment, string newStatus, int changedByUserId);
    Task<List<Appointment>> GetProviderAppointmentsAsync(int providerId, DateTime weekStart, DateTime weekEnd);
    Task<List<AppointmentStatusHistory>> GetAppointmentStatusHistoryAsync(int appointmentId);
    Task<Appointment> GetOverlappingAppointmentAsync(int providerId, DateTime startTime, int durationMinutes, int? excludeAppointmentId = null);
        //Task<Appointment> UpdateAppointmentAsync(Appointment appointment, int changedByUserId);
    }
}
