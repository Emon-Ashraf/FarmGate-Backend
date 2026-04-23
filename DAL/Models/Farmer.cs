namespace DAL.Models;

public class Farmer
{
    public long UserId { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public long? PrimaryCityId { get; set; }
    public bool IsProfileCompleted { get; set; }

    public User User { get; set; } = null!;
    public City? PrimaryCity { get; set; }

    public ICollection<PickupLocation> PickupLocations { get; set; } = new List<PickupLocation>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<IssueReport> IssueReports { get; set; } = new List<IssueReport>();
}