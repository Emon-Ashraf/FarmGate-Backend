using DAL.Enums;

namespace DTO.Order;

public class OrderItemDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public UnitType UnitType { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal? FulfilledQuantity { get; set; }
}