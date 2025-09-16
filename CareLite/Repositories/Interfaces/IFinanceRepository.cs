using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareLite.Repositories.Interfaces
{
    public interface IFinanceRepository
    {
        Task<List<object>> GetOutstandingBalancesAsync(string patientName = null);
        Task<List<object>> GetDailyCollectionsAsync(DateTime date);
    }
}
