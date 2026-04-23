namespace DTO.Order;

public class CreateOrderDto
{
    public long PickupLocationId { get; set; }
    public List<OrderItemRequestDto> Items { get; set; } = new();
}