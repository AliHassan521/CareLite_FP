using CareLite.Models.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareLite.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task<List<Appointment>> GetProviderAppointmentsAsync(int providerId, DateTime weekStart, DateTime weekEnd);
    }
}
