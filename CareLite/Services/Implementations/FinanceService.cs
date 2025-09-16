using CareLite.Services.Interfaces;
using CareLite.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareLite.Services.Implementations
{
    public class FinanceService : IFinanceService
    {
        private readonly IFinanceRepository _repo;
        public FinanceService(IFinanceRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<object>> GetOutstandingBalancesAsync(string patientName = null)
        {
            return await _repo.GetOutstandingBalancesAsync(patientName);
        }

        public async Task<List<object>> GetDailyCollectionsAsync(DateTime date)
        {
            return await _repo.GetDailyCollectionsAsync(date);
        }
    }
}
