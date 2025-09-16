using CareLite.Repositories.Implementations;
using System.Threading.Tasks;

namespace CareLite.Services.Implementations
{
    public class PaymentService
    {
        private readonly PaymentRepository _repo;
        public PaymentService(PaymentRepository repo)
        {
            _repo = repo;
        }

        public async Task<(decimal totalAmount, decimal remainingBalance)> RecordPaymentAsync(int billId, decimal amount, string method, int userId)
        {
            return await _repo.RecordPaymentAsync(billId, amount, method, userId);
        }
    }
}
