using BLL.Interfaces;
using DAL.Interfaces;
using DTO.Payment;

namespace BLL.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentDto?> GetByOrderIdAsync(long orderId)
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(orderId);
        if (payment is null)
            return null;

        return new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Status = payment.Status,
            TransactionReference = payment.TransactionReference,
            PaidAt = payment.PaidAt
        };
    }
}