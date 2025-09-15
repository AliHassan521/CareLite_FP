using CareLite.Models.Domain;
using CareLite.Repositories.Interfaces;
using CareLite.Services.Interfaces;
using System.Threading.Tasks;

namespace CareLite.Services.Implementations
{
    public class VisitService : IVisitService
    {
        private readonly IVisitRepository _repo;
        public VisitService(IVisitRepository repo)
        {
            _repo = repo;
        }

        public async Task<Visit> CreateVisitAsync(Visit visit)
        {
            return await _repo.CreateVisitAsync(visit);
        }

        public async Task<Visit> GetVisitByAppointmentAsync(int appointmentId)
        {
            return await _repo.GetVisitByAppointmentAsync(appointmentId);
        }

        public async Task<Visit> UpdateVisitAsync(Visit visit)
        {
            return await _repo.UpdateVisitAsync(visit);
        }
    }
}
