using System.Security.Claims;
using BLL.Interfaces;
using DTO.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmGate_Backend.Controllers;

/// <summary>
/// Creates a new pickup location for the authenticated farmer.
/// </summary>
/// <param name="request">Pickup location data including city, area, address, coordinates, and instructions.</param>
/// <returns>The created pickup location.</returns>
/// <response code="200">Pickup location created successfully.</response>
/// <response code="400">Validation failed or city is invalid.</response>
/// <response code="401">User token is missing or invalid.</response>
/// <response code="404">Farmer profile not found.</response>
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Provides product listing, search, and farmer product management endpoints.
    /// </summary>
    /// <remarks>
    /// FarmGate product discovery is city-scoped.
    /// Products belong to exactly one pickup location and one farmer.
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<List<ProductListItemDto>>> Search([FromQuery] ProductSearchRequestDto request)
    {
        var result = await _productService.SearchAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Returns detailed information for a single product.
    /// </summary>
    /// <param name="id">Product identifier.</param>
    /// <returns>Detailed product information including pickup and farmer contact data.</returns>
    /// <response code="200">Product returned successfully.</response>
    /// <response code="404">Product not found.</response>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductDetailsDto>> GetById(long id)
    {
        var result = await _productService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Returns products owned by the authenticated farmer.
    /// </summary>
    /// <returns>List of the current farmer's products.</returns>
    /// <response code="200">Products returned successfully.</response>
    /// <response code="401">User token is missing or invalid.</response>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<List<ProductListItemDto>>> GetMyProducts()
    {
        var userId = GetUserId();
        var result = await _productService.GetMyProductsAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new product for the authenticated farmer.
    /// </summary>
    /// <remarks>
    /// Farmer profile must be completed first.
    /// The pickup location must belong to the authenticated farmer and must be active.
    /// </remarks>
    /// <param name="request">Product creation data.</param>
    /// <returns>The created product details.</returns>
    /// <response code="200">Product created successfully.</response>
    /// <response code="400">Validation failed or farmer profile is incomplete.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="403">Pickup location does not belong to the authenticated farmer.</response>
    /// <response code="404">Farmer profile or pickup location not found.</response>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProductDetailsDto>> Create([FromBody] CreateProductDto request)
    {
        var userId = GetUserId();
        var result = await _productService.CreateAsync(userId, request);
        return Ok(result);
    }

    /// <summary>
    /// Updates a product owned by the authenticated farmer.
    /// </summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="request">Updated product data.</param>
    /// <returns>The updated product details.</returns>
    /// <response code="200">Product updated successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="403">Product or pickup location does not belong to the authenticated farmer.</response>
    /// <response code="404">Product or pickup location not found.</response>
    [HttpPut("{id:long}")]
    [Authorize]
    public async Task<ActionResult<ProductDetailsDto>> Update(long id, [FromBody] UpdateProductDto request)
    {
        var userId = GetUserId();
        var result = await _productService.UpdateAsync(userId, id, request);
        return Ok(result);
    }

    /// <summary>
    /// Deactivates a product owned by the authenticated farmer.
    /// </summary>
    /// <param name="id">Product identifier.</param>
    /// <response code="204">Product deactivated successfully.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="403">Product does not belong to the authenticated farmer.</response>
    /// <response code="404">Product not found.</response>
    [HttpDelete("{id:long}")]
    [Authorize]
    public async Task<IActionResult> Deactivate(long id)
    {
        var userId = GetUserId();
        await _productService.DeactivateAsync(userId, id);
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