using CareLite.Models.Domain;
using CareLite.Repositories.Implementations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareLite.Services.Implementations
{
    public class BillService
    {
        private readonly BillRepository _repo;
        public BillService(BillRepository repo)
        {
            _repo = repo;
        }

        public async Task<(Bill, List<BillLineItem>)> GenerateOrGetBillAsync(int visitId)
        {
            return await _repo.GenerateOrGetBillAsync(visitId);
        }
    }
}
