using System.Security.Claims;
using BLL.Interfaces;
using DTO.Issue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmGate_Backend.Controllers;

/// <summary>
/// Provides issue reporting endpoints for customers.
/// </summary>
/// <remarks>
/// Issue reports are only allowed after a completed order.
/// Each report is linked to a specific existing order and can later be reviewed by an admin.
/// </remarks>
[ApiController]
[Route("api/issues")]
[Authorize]
public class IssuesController : ControllerBase
{
    private readonly IIssueReportService _issueReportService;

    public IssuesController(IIssueReportService issueReportService)
    {
        _issueReportService = issueReportService;
    }

    /// <summary>
    /// Creates an issue report for a completed order.
    /// </summary>
    /// <param name="request">Issue data including order id, title, and description.</param>
    /// <returns>The created issue report.</returns>
    /// <response code="200">Issue report created successfully.</response>
    /// <response code="400">Order is not completed or request validation failed.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="404">Customer profile or order not found.</response>
    [HttpPost]
    public async Task<ActionResult<IssueReportDto>> Create([FromBody] CreateIssueReportDto request)
    {
        var userId = GetUserId();
        var result = await _issueReportService.CreateAsync(userId, request);
        return Ok(result);
    }

    /// <summary>
    /// Returns issue reports created by the authenticated customer.
    /// </summary>
    /// <returns>List of the current customer's issue reports.</returns>
    /// <response code="200">Issue reports returned successfully.</response>
    /// <response code="401">User token is missing or invalid.</response>
    [HttpGet("me")]
    public async Task<ActionResult<List<IssueReportDto>>> GetMyReports()
    {
        var userId = GetUserId();
        var result = await _issueReportService.GetMyReportsAsync(userId);
        return Ok(result);
    }

    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user token.");

        return userId;
    }
}