using System.Security.Claims;
using BLL.Interfaces;
using DTO.Admin;
using DTO.Issue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmGate_Backend.Controllers;

/// <summary>
/// Administrative moderation endpoints for FarmGate.
/// </summary>
/// <remarks>
/// Admin scope in MVP is intentionally limited to:
/// issue review,
/// user deactivation,
/// and product deactivation.
/// </remarks>
[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IIssueReportService _issueReportService;

    public AdminController(IAdminService adminService, IIssueReportService issueReportService)
    {
        _adminService = adminService;
        _issueReportService = issueReportService;
    }

    /// <summary>
    /// Returns open and under-review issue reports for admin review.
    /// </summary>
    /// <returns>List of issue summaries for admin moderation.</returns>
    /// <response code="200">Open issues returned successfully.</response>
    /// <response code="401">User token is missing or invalid.</response>
    [HttpGet("issues/open")]
    public async Task<ActionResult<List<AdminIssueListItemDto>>> GetOpenIssues()
    {
        var result = await _adminService.GetOpenIssuesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Resolves or updates the status of an issue report as admin.
    /// </summary>
    /// <param name="id">Issue report identifier.</param>
    /// <param name="request">New issue status and optional admin note.</param>
    /// <returns>Updated issue report.</returns>
    /// <response code="200">Issue updated successfully.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="404">Issue report not found.</response>
    [HttpPatch("issues/{id:long}/resolve")]
    public async Task<ActionResult<IssueReportDto>> ResolveIssue(long id, [FromBody] AdminResolveIssueDto request)
    {
        var result = await _issueReportService.ResolveAsync(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Activates or deactivates a user account as admin.
    /// </summary>
    /// <param name="id">Target user identifier.</param>
    /// <param name="request">Activation state to apply.</param>
    /// <response code="204">User moderation action completed successfully.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="403">Authenticated user is not an admin.</response>
    /// <response code="404">Target user not found.</response>
    [HttpPatch("users/{id:long}/deactivate")]
    public async Task<IActionResult> DeactivateUser(long id, [FromBody] AdminModerateUserDto request)
    {
        var adminUserId = GetUserId();
        await _adminService.DeactivateUserAsync(adminUserId, id, request);
        return NoContent();
    }

    /// <summary>
    /// Activates or deactivates a product as admin.
    /// </summary>
    /// <param name="id">Target product identifier.</param>
    /// <param name="request">Activation state to apply.</param>
    /// <response code="204">Product moderation action completed successfully.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="403">Authenticated user is not an admin.</response>
    /// <response code="404">Product not found.</response>
    [HttpPatch("products/{id:long}/deactivate")]
    public async Task<IActionResult> DeactivateProduct(long id, [FromBody] AdminModerateProductDto request)
    {
        var adminUserId = GetUserId();
        await _adminService.DeactivateProductAsync(adminUserId, id, request);
        return NoContent();
    }

    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user token.");

        return userId;
    }
}