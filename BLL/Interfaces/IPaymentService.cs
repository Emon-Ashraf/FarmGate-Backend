using DTO.Payment;

namespace BLL.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto?> GetByOrderIdAsync(long orderId);
}