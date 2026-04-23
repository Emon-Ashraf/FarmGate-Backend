namespace DAL.Models;

public class City : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? RegionName { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<PickupLocation> PickupLocations { get; set; } = new List<PickupLocation>();
    public ICollection<Farmer> Farmers { get; set; } = new List<Farmer>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}