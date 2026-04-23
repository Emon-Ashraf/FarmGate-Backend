using DAL.Enums;

namespace DTO.Order;

public class OrderSummaryDto
{
    public long Id { get; set; }
    public OrderStatus Status { get; set; }
    public string CounterpartyName { get; set; } = null!;
    public decimal EstimatedProductTotal { get; set; }
    public decimal? ActualProductTotal { get; set; }
    public decimal ServiceFeeAmount { get; set; }
    public DateTime PickupDueAt { get; set; }
    public DateTime CreatedAt { get; set; }
}