using BLL.Interfaces;
using DTO.Auth;
using Microsoft.AspNetCore.Mvc;

namespace FarmGate_Backend.Controllers;

/// <summary>
/// Authentication endpoints for registering and logging into FarmGate accounts.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new FarmGate account as Customer, Farmer, or Admin.
    /// </summary>
    /// <remarks>
    /// Customer and Farmer accounts may optionally include a primary city.
    /// Farmer registration requires a display name.
    /// </remarks>
    /// <param name="request">Registration data including full name, phone, password, role, and optional profile data.</param>
    /// <returns>JWT token and basic account information.</returns>
    /// <response code="200">Registration successful.</response>
    /// <response code="400">Validation failed or phone number already exists.</response>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Logs in using phone number or email and returns a JWT access token.
    /// </summary>
    /// <remarks>
    /// The login field accepts either the user's phone number or email.
    /// </remarks>
    /// <param name="request">Login credentials.</param>
    /// <returns>JWT token and basic account information.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="400">Invalid login or password.</response>
    /// <response code="403">Account is deactivated.</response>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }
}