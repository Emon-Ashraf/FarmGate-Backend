using DAL.Enums;

namespace DTO.Admin;

public class AdminIssueListItemDto
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public string Title { get; set; } = null!;
    public IssueStatus Status { get; set; }
    public string CustomerName { get; set; } = null!;
    public string FarmerName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}