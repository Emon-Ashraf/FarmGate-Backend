using System.Security.Claims;
using BLL.Interfaces;
using DTO.PickupLocation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmGate_Backend.Controllers;

/// <summary>
/// Manages farmer-owned pickup locations.
/// </summary>
/// <remarks>
/// Pickup locations are essential in FarmGate because the system is pickup-only.
/// Each product belongs to one pickup location, and each order uses exactly one pickup location.
/// </remarks>
[ApiController]
[Route("api/pickup-locations")]
[Authorize]
public class PickupLocationsController : ControllerBase
{
    private readonly IPickupLocationService _pickupLocationService;

    public PickupLocationsController(IPickupLocationService pickupLocationService)
    {
        _pickupLocationService = pickupLocationService;
    }

    /// <summary>
    /// Returns pickup locations owned by the authenticated farmer.
    /// </summary>
    /// <returns>List of the current farmer's pickup locations.</returns>
    /// <response code="200">Pickup locations returned successfully.</response>
    /// <response code="401">User token is missing or invalid.</response>
    [HttpGet("me")]
    public async Task<ActionResult<List<PickupLocationDto>>> GetMyLocations()
    {
        var userId = GetUserId();
        var result = await _pickupLocationService.GetMyLocationsAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new pickup location for the authenticated farmer.
    /// </summary>
    /// <param name="request">Pickup location data including city, area, address, coordinates, and instructions.</param>
    /// <returns>The created pickup location.</returns>
    /// <response code="200">Pickup location created successfully.</response>
    /// <response code="400">Validation failed or city is invalid.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="404">Farmer profile not found.</response>
    [HttpPost]
    public async Task<ActionResult<PickupLocationDto>> Create([FromBody] CreatePickupLocationDto request)
    {
        var userId = GetUserId();
        var result = await _pickupLocationService.CreateAsync(userId, request);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing pickup location owned by the authenticated farmer.
    /// </summary>
    /// <param name="id">Pickup location identifier.</param>
    /// <param name="request">Updated pickup location data.</param>
    /// <returns>The updated pickup location.</returns>
    /// <response code="200">Pickup location updated successfully.</response>
    /// <response code="400">Validation failed or city is invalid.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="403">Pickup location does not belong to the authenticated farmer.</response>
    /// <response code="404">Pickup location not found.</response>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<PickupLocationDto>> Update(long id, [FromBody] UpdatePickupLocationDto request)
    {
        var userId = GetUserId();
        var result = await _pickupLocationService.UpdateAsync(userId, id, request);
        return Ok(result);
    }

    /// <summary>
    /// Deactivates a pickup location owned by the authenticated farmer.
    /// </summary>
    /// <remarks>
    /// Deactivation is blocked when active AwaitingFee or Confirmed orders depend on the location.
    /// </remarks>
    /// <param name="id">Pickup location identifier.</param>
    /// <response code="204">Pickup location deactivated successfully.</response>
    /// <response code="400">Pickup location has active dependent orders.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="403">Pickup location does not belong to the authenticated farmer.</response>
    /// <response code="404">Pickup location not found.</response>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Deactivate(long id)
    {
        var userId = GetUserId();
        await _pickupLocationService.DeactivateAsync(userId, id);
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