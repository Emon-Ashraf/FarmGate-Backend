using DAL.Models;

namespace DAL.Interfaces;

public interface IIssueReportRepository
{
    Task<IssueReport?> GetByIdAsync(long id);
    Task<List<IssueReport>> GetByCustomerIdAsync(long customerId);
    Task<List<IssueReport>> GetOpenIssuesAsync();
    Task AddAsync(IssueReport issueReport);
    void Update(IssueReport issueReport);
    Task SaveChangesAsync();
}