using CareLite.Models.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareLite.Repositories.Interfaces
{
    public interface IBillRepository
    {
        Task<(Bill, List<BillLineItem>)> GenerateOrGetBillAsync(int visitId);
    }
}
