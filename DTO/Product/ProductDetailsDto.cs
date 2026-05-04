using DAL.Enums;

namespace DTO.Product;

public class ProductDetailsDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public UnitType UnitType { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal AvailableQuantity { get; set; }
    public long PickupLocationId { get; set; }
    public string PickupArea { get; set; } = null!;
    public string CityName { get; set; } = null!;
    public string PickupAddress { get; set; } = null!;
    public string? Instructions { get; set; }
    public long FarmerId { get; set; }
    public string? FarmerProfileImageUrl { get; set; }
    public string FarmerName { get; set; } = null!;
    public string? FarmerPhone { get; set; }
    public string? ImageUrl { get; set; }
}