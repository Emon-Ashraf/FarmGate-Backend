namespace DTO.Order;

public class OrderItemRequestDto
{
    public long ProductId { get; set; }
    public decimal Quantity { get; set; }
}