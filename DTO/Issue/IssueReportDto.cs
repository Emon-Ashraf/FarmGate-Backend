using DAL.Enums;

namespace DTO.Issue;

public class IssueReportDto
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long CustomerId { get; set; }
    public long FarmerId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public IssueStatus Status { get; set; }
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; }
}