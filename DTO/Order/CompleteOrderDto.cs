namespace DTO.Order;

public class CompleteOrderDto
{
    public string PickupCode { get; set; } = null!;
    public List<CompleteOrderItemDto> Items { get; set; } = new();
}