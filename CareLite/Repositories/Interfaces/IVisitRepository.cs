using CareLite.Models.Domain;
using System.Threading.Tasks;

namespace CareLite.Repositories.Interfaces
{
    public interface IVisitRepository
    {
        Task<Visit> CreateVisitAsync(Visit visit);
        Task<Visit> GetVisitByAppointmentAsync(int appointmentId);
        Task<Visit> UpdateVisitAsync(Visit visit);
    }
}
