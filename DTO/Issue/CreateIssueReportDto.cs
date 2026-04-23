namespace DTO.Issue;

public class CreateIssueReportDto
{
    public long OrderId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
}