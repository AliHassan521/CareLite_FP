using CareLite.Models.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareLite.Services.Interfaces
{
    public interface IAppointmentService
    {
    Task<Appointment> ScheduleAppointmentAsync(Appointment appointment, int createdByUserId);
    Task<Appointment> UpdateAppointmentAsync(Appointment appointment,string newSatus, int changedByUserId);
    Task<List<Appointment>> GetProviderAppointmentsAsync(int providerId, DateTime weekStart, DateTime weekEnd);
    Task<Appointment> GetOverlappingAppointmentAsync(int providerId, DateTime startTime, int durationMinutes, int? excludeAppointmentId = null);
    Task<List<Appointment>> GetAllAppointmentsAsync();
    }
}
