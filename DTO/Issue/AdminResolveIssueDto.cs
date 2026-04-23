using DAL.Enums;

namespace DTO.Issue;

public class AdminResolveIssueDto
{
    public IssueStatus Status { get; set; }
    public string? AdminNote { get; set; }
}