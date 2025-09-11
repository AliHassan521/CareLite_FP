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

        public async Task<Appointment> ScheduleAppointmentAsync(Appointment appointment)
        {
            // Additional business logic can be added here if needed
            return await _repo.CreateAppointmentAsync(appointment);
        }

        public async Task<List<Appointment>> GetProviderAppointmentsAsync(int providerId, DateTime weekStart, DateTime weekEnd)
        {
            return await _repo.GetProviderAppointmentsAsync(providerId, weekStart, weekEnd);
        }
    }
}
