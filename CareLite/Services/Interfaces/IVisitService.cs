using CareLite.Models.Domain;
using System.Threading.Tasks;

namespace CareLite.Services.Interfaces
{
    public interface IVisitService
    {
        Task<Visit> CreateVisitAsync(Visit visit);
        Task<Visit> GetVisitByAppointmentAsync(int appointmentId);
        Task<Visit> UpdateVisitAsync(Visit visit);
    }
}
