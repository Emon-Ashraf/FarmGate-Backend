using DAL.Data;
using DAL.Enums;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class IssueReportRepository : IIssueReportRepository
{
    private readonly ApplicationDbContext _context;

    public IssueReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IssueReport?> GetByIdAsync(long id)
    {
        return await _context.IssueReports
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<IssueReport>> GetByCustomerIdAsync(long customerId)
    {
        return await _context.IssueReports
            .Include(x => x.Order)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<IssueReport>> GetOpenIssuesAsync()
    {
        return await _context.IssueReports
            .Include(x => x.Order)
            .Include(x => x.Customer)
                .ThenInclude(x => x.User)
            .Include(x => x.Farmer)
                .ThenInclude(x => x.User)
            .Where(x => x.Status == IssueStatus.Open || x.Status == IssueStatus.UnderReview)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(IssueReport issueReport)
    {
        await _context.IssueReports.AddAsync(issueReport);
    }

    public void Update(IssueReport issueReport)
    {
        _context.IssueReports.Update(issueReport);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}