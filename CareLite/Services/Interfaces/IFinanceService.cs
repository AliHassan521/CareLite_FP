using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace CareLite.Services.Interfaces
{
    public interface IFinanceService
    {
        Task<List<object>> GetOutstandingBalancesAsync(string patientName = null);
        Task<List<object>> GetDailyCollectionsAsync(DateTime date);
    }
}
