using CareLite.Models.Domain;
using CareLite.Repositories.Interfaces;
using CareLite.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareLite.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repo;
        public AppointmentService(IAppointmentRepository repo)
        {
            _repo = repo;
        }


        public async Task<Appointment> ScheduleAppointmentAsync(Appointment appointment, int createdByUserId)
        {
            // Pass userId to repository
            return await _repo.CreateAppointmentAsync(appointment, createdByUserId);
        }

        public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment, int changedByUserId)
        {
            // Pass userId to repository
            return await _repo.UpdateAppointmentAsync(appointment, changedByUserId);
        }

        public async Task<List<Appointment>> GetProviderAppointmentsAsync(int providerId, DateTime weekStart, DateTime weekEnd)
        {
            return await _repo.GetProviderAppointmentsAsync(providerId, weekStart, weekEnd);
        }

        public async Task<List<AppointmentStatusHistory>> GetAppointmentStatusHistoryAsync(int appointmentId)
        {
            return await _repo.GetAppointmentStatusHistoryAsync(appointmentId);
        }
    }
}
