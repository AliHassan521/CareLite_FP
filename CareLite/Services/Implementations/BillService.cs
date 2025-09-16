using CareLite.Models.Domain;
using CareLite.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareLite.Services.Interfaces
{
    public class BillService : IBillService
    {
        private readonly IBillRepository _repo;
        public BillService(IBillRepository repo)
        {
            _repo = repo;
        }

        public async Task<(Bill, List<BillLineItem>)> GenerateOrGetBillAsync(int visitId)
        {
            return await _repo.GenerateOrGetBillAsync(visitId);
        }
    }
}
