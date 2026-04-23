using DAL.Enums;

namespace DAL.Models;

public class IssueReport : BaseEntity
{
    public long OrderId { get; set; }
    public long CustomerId { get; set; }
    public long FarmerId { get; set; }

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public IssueStatus Status { get; set; }
    public string? AdminNote { get; set; }

    public Order Order { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Farmer Farmer { get; set; } = null!;
}