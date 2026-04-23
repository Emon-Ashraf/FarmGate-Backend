namespace DAL.Models;

public class PickupLocation : BaseEntity
{
    public long FarmerId { get; set; }
    public long CityId { get; set; }
    public string AreaName { get; set; } = null!;
    public string AddressLine { get; set; } = null!;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Instructions { get; set; }
    public bool IsActive { get; set; } = true;

    public Farmer Farmer { get; set; } = null!;
    public City City { get; set; } = null!;

    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}