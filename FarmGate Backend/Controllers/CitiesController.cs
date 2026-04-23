using BLL.Interfaces;
using DTO.City;
using Microsoft.AspNetCore.Mvc;

namespace FarmGate_Backend.Controllers;

/// <summary>
/// Provides city data used for locality-based FarmGate features.
/// </summary>
/// <remarks>
/// FarmGate product discovery is city-scoped, so active cities are used in search,
/// profile preferences, and pickup location creation.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class CitiesController : ControllerBase
{
    private readonly ICityService _cityService;

    public CitiesController(ICityService cityService)
    {
        _cityService = cityService;
    }

    /// <summary>
    /// Returns all active cities.
    /// </summary>
    /// <returns>List of active cities available for FarmGate usage.</returns>
    /// <response code="200">Cities returned successfully.</response>
    [HttpGet]
    public async Task<ActionResult<List<CityListItemDto>>> GetCities()
    {
        var result = await _cityService.GetCitiesAsync();
        return Ok(result);
    }
}