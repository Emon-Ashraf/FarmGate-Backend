using System.Security.Claims;
using BLL.Interfaces;
using DTO.Rating;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmGate_Backend.Controllers;

/// <summary>
/// Provides rating endpoints for completed FarmGate orders.
/// </summary>
/// <remarks>
/// Ratings are only allowed after a completed real pickup transaction.
/// Each order can have at most one rating.
/// </remarks>
[ApiController]
[Route("api/ratings")]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    /// <summary>
    /// Creates a rating for a completed order.
    /// </summary>
    /// <param name="request">Rating data including order id, score, and optional comment.</param>
    /// <returns>The created rating.</returns>
    /// <response code="200">Rating created successfully.</response>
    /// <response code="400">Order is not completed, rating already exists, or score is invalid.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="404">Customer profile or order not found.</response>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<RatingDto>> Create([FromBody] CreateRatingDto request)
    {
        var userId = GetUserId();
        var result = await _ratingService.CreateAsync(userId, request);
        return Ok(result);
    }

    /// <summary>
    /// Returns ratings for a specific farmer.
    /// </summary>
    /// <param name="farmerId">Farmer user identifier.</param>
    /// <returns>List of ratings for the selected farmer.</returns>
    /// <response code="200">Ratings returned successfully.</response>
    [HttpGet("farmers/{farmerId:long}")]
    public async Task<ActionResult<List<RatingDto>>> GetForFarmer(long farmerId)
    {
        var result = await _ratingService.GetForFarmerAsync(farmerId);
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