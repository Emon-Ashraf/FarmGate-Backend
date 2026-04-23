using System.Security.Claims;
using BLL.Interfaces;
using DTO.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmGate_Backend.Controllers;

/// <summary>
/// Searches active products within a selected city.
/// </summary>
/// <remarks>
/// Product discovery in FarmGate is city-scoped. Results may be filtered by text, area, category, and sort order.
/// </remarks>
/// <param name="request">Search filters including city, query text, area, category, page, and page size.</param>
/// <returns>List of matching products.</returns>
/// <response code="200">Products returned successfully.</response>
/// <response code="400">City is missing or invalid.</response>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>
    /// Returns the current authenticated user's profile.
    /// </summary>
    /// <remarks>
    /// The returned profile shape depends on the authenticated user's role.
    /// Farmer and Customer profiles return different DTOs.
    /// </remarks>
    /// <returns>The authenticated user's role-specific profile.</returns>
    /// <response code="200">Profile returned successfully.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="404">Profile not found.</response>
    [HttpGet("me")]
    public async Task<ActionResult<object>> GetMyProfile()
    {
        var userId = GetUserId();
        var result = await _profileService.GetMyProfileAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Updates the authenticated farmer's profile.
    /// </summary>
    /// <remarks>
    /// Completing this profile enables product creation workflows for the farmer.
    /// </remarks>
    /// <param name="request">Farmer profile data including display name, description, and optional primary city.</param>
    /// <returns>Updated farmer profile.</returns>
    /// <response code="200">Farmer profile updated successfully.</response>
    /// <response code="400">Validation failed or selected city is invalid.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="404">Farmer profile not found.</response>
    [HttpPut("farmer")]
    public async Task<ActionResult<FarmerProfileDto>> UpdateFarmerProfile([FromBody] UpdateFarmerProfileDto request)
    {
        var userId = GetUserId();
        var result = await _profileService.UpdateFarmerProfileAsync(userId, request);
        return Ok(result);
    }

    /// <summary>
    /// Updates the authenticated customer's profile.
    /// </summary>
    /// <param name="request">Customer profile data including optional primary city.</param>
    /// <returns>Updated customer profile.</returns>
    /// <response code="200">Customer profile updated successfully.</response>
    /// <response code="400">Validation failed or selected city is invalid.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="404">Customer profile not found.</response>
    [HttpPut("customer")]
    public async Task<ActionResult<CustomerProfileDto>> UpdateCustomerProfile([FromBody] UpdateCustomerProfileDto request)
    {
        var userId = GetUserId();
        var result = await _profileService.UpdateCustomerProfileAsync(userId, request);
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