using DTO.Issue;

namespace BLL.Interfaces;

public interface IIssueReportService
{
    Task<IssueReportDto> CreateAsync(long userId, CreateIssueReportDto request);
    Task<List<IssueReportDto>> GetMyReportsAsync(long userId);
    Task<List<IssueReportDto>> GetOpenReportsAsync();
    Task<IssueReportDto> ResolveAsync(long issueId, AdminResolveIssueDto request);
}