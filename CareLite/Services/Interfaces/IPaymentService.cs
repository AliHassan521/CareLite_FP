using System.Threading.Tasks;

namespace CareLite.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<(decimal totalAmount, decimal remainingBalance)> RecordPaymentAsync(int billId, decimal amount, string method, int userId);
    }
}
