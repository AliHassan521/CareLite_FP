using System.Threading.Tasks;

namespace CareLite.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<(decimal totalAmount, decimal remainingBalance)> RecordPaymentAsync(int billId, decimal amount, string method, int userId);
    }
}
