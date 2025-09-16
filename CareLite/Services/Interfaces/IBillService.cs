using CareLite.Models.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareLite.Services.Interfaces
{
    public interface IBillService
    {
        Task<(Bill, List<BillLineItem>)> GenerateOrGetBillAsync(int visitId);
    }
}
