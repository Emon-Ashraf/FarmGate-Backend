using DAL.Enums;
using DTO.Payment;

namespace DTO.Order;

public class OrderDetailsDto
{
    public long Id { get; set; }
    public OrderStatus Status { get; set; }
    public CancellationReason? CancellationReason { get; set; }

    public string CustomerName { get; set; } = null!;
    public string FarmerName { get; set; } = null!;

    public decimal EstimatedProductTotal { get; set; }
    public decimal? ActualProductTotal { get; set; }
    public decimal ServiceFeeAmount { get; set; }

    public DateTime? FeePaidAt { get; set; }
    public DateTime PickupDueAt { get; set; }

    public string PickupCity { get; set; } = null!;
    public string PickupArea { get; set; } = null!;
    public string PickupAddress { get; set; } = null!;
    public string? PickupInstructions { get; set; }
    public string? FarmerPhone { get; set; }

    public string? PickupCode { get; set; }

    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public PaymentDto? Payment { get; set; }
}