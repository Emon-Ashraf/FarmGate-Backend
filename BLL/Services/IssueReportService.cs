using BLL.Exceptions;
using BLL.Interfaces;
using DAL.Data;
using DAL.Enums;
using DAL.Interfaces;
using DAL.Models;
using DTO.Issue;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class IssueReportService : IIssueReportService
{
    private readonly IIssueReportRepository _issueReportRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ApplicationDbContext _context;

    public IssueReportService(
        IIssueReportRepository issueReportRepository,
        IOrderRepository orderRepository,
        ApplicationDbContext context)
    {
        _issueReportRepository = issueReportRepository;
        _orderRepository = orderRepository;
        _context = context;
    }

    public async Task<IssueReportDto> CreateAsync(long userId, CreateIssueReportDto request)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.UserId == userId);
        if (customer is null)
            throw new NotFoundException("Customer profile not found.");

        var order = await _orderRepository.GetByIdForCustomerAsync(request.OrderId, userId);
        if (order is null)
            throw new NotFoundException("Order not found.");

        if (order.Status != OrderStatus.Completed)
            throw new BadRequestException("Issue reporting is allowed only after order completion.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new BadRequestException("Issue title is required.");

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new BadRequestException("Issue description is required.");

        var issue = new IssueReport
        {
            OrderId = order.Id,
            CustomerId = userId,
            FarmerId = order.FarmerId,
            Title = request.Title,
            Description = request.Description,
            Status = IssueStatus.Open
        };

        await _issueReportRepository.AddAsync(issue);
        await _issueReportRepository.SaveChangesAsync();

        return new IssueReportDto
        {
            Id = issue.Id,
            OrderId = issue.OrderId,
            CustomerId = issue.CustomerId,
            FarmerId = issue.FarmerId,
            Title = issue.Title,
            Description = issue.Description,
            Status = issue.Status,
            AdminNote = issue.AdminNote,
            CreatedAt = issue.CreatedAt
        };
    }

    public async Task<List<IssueReportDto>> GetMyReportsAsync(long userId)
    {
        var issues = await _issueReportRepository.GetByCustomerIdAsync(userId);

        return issues.Select(x => new IssueReportDto
        {
            Id = x.Id,
            OrderId = x.OrderId,
            CustomerId = x.CustomerId,
            FarmerId = x.FarmerId,
            Title = x.Title,
            Description = x.Description,
            Status = x.Status,
            AdminNote = x.AdminNote,
            CreatedAt = x.CreatedAt
        }).ToList();
    }

    public async Task<List<IssueReportDto>> GetOpenReportsAsync()
    {
        var issues = await _issueReportRepository.GetOpenIssuesAsync();

        return issues.Select(x => new IssueReportDto
        {
            Id = x.Id,
            OrderId = x.OrderId,
            CustomerId = x.CustomerId,
            FarmerId = x.FarmerId,
            Title = x.Title,
            Description = x.Description,
            Status = x.Status,
            AdminNote = x.AdminNote,
            CreatedAt = x.CreatedAt
        }).ToList();
    }

    public async Task<IssueReportDto> ResolveAsync(long issueId, AdminResolveIssueDto request)
    {
        var issue = await _issueReportRepository.GetByIdAsync(issueId);
        if (issue is null)
            throw new NotFoundException("Issue report not found.");

        issue.Status = request.Status;
        issue.AdminNote = request.AdminNote;

        _issueReportRepository.Update(issue);
        await _issueReportRepository.SaveChangesAsync();

        return new IssueReportDto
        {
            Id = issue.Id,
            OrderId = issue.OrderId,
            CustomerId = issue.CustomerId,
            FarmerId = issue.FarmerId,
            Title = issue.Title,
            Description = issue.Description,
            Status = issue.Status,
            AdminNote = issue.AdminNote,
            CreatedAt = issue.CreatedAt
        };
    }
}