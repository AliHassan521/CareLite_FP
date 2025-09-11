using CareLite.Models.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareLite.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<Appointment> ScheduleAppointmentAsync(Appointment appointment);
        Task<List<Appointment>> GetProviderAppointmentsAsync(int providerId, DateTime weekStart, DateTime weekEnd);
    }
}
