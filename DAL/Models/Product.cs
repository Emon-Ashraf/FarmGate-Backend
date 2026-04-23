using DAL.Enums;

namespace DAL.Models;

public class Product : BaseEntity
{
    public long FarmerId { get; set; }
    public long PickupLocationId { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public UnitType UnitType { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal AvailableQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public Farmer Farmer { get; set; } = null!;
    public PickupLocation PickupLocation { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}