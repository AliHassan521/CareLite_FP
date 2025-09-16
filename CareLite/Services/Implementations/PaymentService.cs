using CareLite.Repositories.Interfaces;
using System.Threading.Tasks;

namespace CareLite.Services.Implementations
{
    public class PaymentService : CareLite.Services.Interfaces.IPaymentService
    {
        private readonly IPaymentRepository _repo;
        public PaymentService(IPaymentRepository repo)
        {
            _repo = repo;
        }

        public async Task<(decimal totalAmount, decimal remainingBalance)> RecordPaymentAsync(int billId, decimal amount, string method, int userId)
        {
            return await _repo.RecordPaymentAsync(billId, amount, method, userId);
        }
    }
}
