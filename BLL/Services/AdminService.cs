using BLL.Exceptions;
using BLL.Interfaces;
using DAL.Data;
using DTO.Admin;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly IIssueReportService _issueReportService;

    public AdminService(ApplicationDbContext context, IIssueReportService issueReportService)
    {
        _context = context;
        _issueReportService = issueReportService;
    }

    public async Task<List<AdminIssueListItemDto>> GetOpenIssuesAsync()
    {
        var issues = await _context.IssueReports
            .Include(x => x.Customer)
                .ThenInclude(x => x.User)
            .Include(x => x.Farmer)
                .ThenInclude(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return issues
            .Where(x => x.Status == DAL.Enums.IssueStatus.Open || x.Status == DAL.Enums.IssueStatus.UnderReview)
            .Select(x => new AdminIssueListItemDto
            {
                Id = x.Id,
                OrderId = x.OrderId,
                Title = x.Title,
                Status = x.Status,
                CustomerName = x.Customer.User.FullName,
                FarmerName = x.Farmer.User.FullName,
                CreatedAt = x.CreatedAt
            })
            .ToList();
    }

    public async Task DeactivateUserAsync(long userId, long targetUserId, AdminModerateUserDto request)
    {
        var admin = await _context.Admins.FirstOrDefaultAsync(x => x.UserId == userId);
        if (admin is null)
            throw new ForbiddenException("Only admin can moderate users.");

        var targetUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == targetUserId);
        if (targetUser is null)
            throw new NotFoundException("Target user not found.");

        targetUser.IsActive = request.IsActive;
        await _context.SaveChangesAsync();
    }

    public async Task DeactivateProductAsync(long userId, long productId, AdminModerateProductDto request)
    {
        var admin = await _context.Admins.FirstOrDefaultAsync(x => x.UserId == userId);
        if (admin is null)
            throw new ForbiddenException("Only admin can moderate products.");

        var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == productId);
        if (product is null)
            throw new NotFoundException("Product not found.");

        product.IsActive = request.IsActive;
        await _context.SaveChangesAsync();
    }
}