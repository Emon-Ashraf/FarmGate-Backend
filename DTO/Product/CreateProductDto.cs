using DAL.Enums;

namespace DTO.Product;

public class CreateProductDto
{
    public long PickupLocationId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public UnitType UnitType { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal AvailableQuantity { get; set; }
    public string? ImageUrl { get; set; }
}