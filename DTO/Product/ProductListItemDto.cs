using DAL.Enums;

namespace DTO.Product;

public class ProductListItemDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public UnitType UnitType { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal AvailableQuantity { get; set; }
    public long PickupLocationId { get; set; }
    public string PickupArea { get; set; } = null!;
    public string CityName { get; set; } = null!;
    public string FarmerName { get; set; } = null!;
    public string? ImageUrl { get; set; }
}