namespace DAL.Models;

public class Customer
{
    public long UserId { get; set; }
    public long? PrimaryCityId { get; set; }

    public User User { get; set; } = null!;
    public City? PrimaryCity { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<IssueReport> IssueReports { get; set; } = new List<IssueReport>();
}