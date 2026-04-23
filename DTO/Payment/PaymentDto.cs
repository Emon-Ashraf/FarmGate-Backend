using DAL.Enums;

namespace DTO.Payment;

public class PaymentDto
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionReference { get; set; }
    public DateTime? PaidAt { get; set; }
}