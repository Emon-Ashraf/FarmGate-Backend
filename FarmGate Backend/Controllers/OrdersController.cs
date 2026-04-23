using System.Security.Claims;
using BLL.Interfaces;
using DTO.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmGate_Backend.Controllers;

/// <summary>
/// Manages FarmGate order workflows.
/// </summary>
/// <remarks>
/// FarmGate orders are pickup-only.
/// Order lifecycle:
/// Pending -> AwaitingFee -> Confirmed -> Completed
/// with alternate transitions to Rejected or Cancelled.
/// </remarks>
[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <remarks>
    /// All items must belong to exactly one farmer and one pickup location.
    /// Stock is not reduced at creation; it is reduced only when the farmer accepts the order.
    /// </remarks>
    /// <param name="request">Pickup location and order items.</param>
    /// <returns>Created order details.</returns>
    /// <response code="200">Order created successfully.</response>
    /// <response code="400">Order validation failed.</response>
    /// <response code="401">User token is missing or invalid.</response>
    [HttpPost]
    public async Task<ActionResult<OrderDetailsDto>> Create([FromBody] CreateOrderDto request)
    {
        var userId = GetUserId();
        var result = await _orderService.CreateOrderAsync(userId, request);
        return Ok(result);
    }

    /// <summary>
    /// Returns orders belonging to the authenticated customer.
    /// </summary>
    /// <returns>List of customer order summaries.</returns>
    /// <response code="200">Orders returned successfully.</response>
    /// <response code="401">User token is missing or invalid.</response>
    [HttpGet("customer")]
    public async Task<ActionResult<List<OrderSummaryDto>>> GetCustomerOrders()
    {
        var userId = GetUserId();
        var result = await _orderService.GetCustomerOrdersAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Returns orders belonging to the authenticated farmer.
    /// </summary>
    /// <returns>List of farmer order summaries.</returns>
    /// <response code="200">Orders returned successfully.</response>
    /// <response code="401">User token is missing or invalid.</response>
    [HttpGet("farmer")]
    public async Task<ActionResult<List<OrderSummaryDto>>> GetFarmerOrders()
    {
        var userId = GetUserId();
        var result = await _orderService.GetFarmerOrdersAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Returns details for a single order visible to the authenticated user.
    /// </summary>
    /// <remarks>
    /// Customers can only access their own orders.
    /// Farmers can only access their own incoming orders.
    /// Admin can access any order.
    /// </remarks>
    /// <param name="id">Order identifier.</param>
    /// <returns>Detailed order information.</returns>
    /// <response code="200">Order returned successfully.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="404">Order not found.</response>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<OrderDetailsDto>> GetById(long id)
    {
        var userId = GetUserId();
        var result = await _orderService.GetOrderDetailsAsync(userId, id);
        return Ok(result);
    }

    /// <summary>
    /// Accepts a pending order as the farmer.
    /// </summary>
    /// <remarks>
    /// Acceptance rechecks stock and reserves it if available.
    /// Order moves from Pending to AwaitingFee.
    /// </remarks>
    /// <param name="id">Order identifier.</param>
    /// <returns>Updated order details.</returns>
    /// <response code="200">Order accepted successfully.</response>
    /// <response code="400">Order is not pending or stock is insufficient.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="403">Order does not belong to the authenticated farmer.</response>
    /// <response code="404">Order not found.</response>
    [HttpPatch("{id:long}/accept")]
    public async Task<ActionResult<OrderDetailsDto>> Accept(long id)
    {
        var userId = GetUserId();
        var result = await _orderService.AcceptOrderAsync(userId, id);
        return Ok(result);
    }

    /// <summary>
    /// Rejects a pending order as the farmer.
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <returns>Updated order details.</returns>
    /// <response code="200">Order rejected successfully.</response>
    /// <response code="400">Order is not pending.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="403">Order does not belong to the authenticated farmer.</response>
    /// <response code="404">Order not found.</response>
    [HttpPatch("{id:long}/reject")]
    public async Task<ActionResult<OrderDetailsDto>> Reject(long id)
    {
        var userId = GetUserId();
        var result = await _orderService.RejectOrderAsync(userId, id);
        return Ok(result);
    }

    /// <summary>
    /// Cancels an order when allowed by lifecycle and ownership rules.
    /// </summary>
    /// <remarks>
    /// Customer can cancel in Pending or AwaitingFee.
    /// Farmer can cancel in AwaitingFee due to unavailability.
    /// Admin can cancel in selected states.
    /// Reserved stock is restored when applicable.
    /// </remarks>
    /// <param name="id">Order identifier.</param>
    /// <param name="request">Optional cancellation note.</param>
    /// <returns>Updated order details.</returns>
    /// <response code="200">Order cancelled successfully.</response>
    /// <response code="400">Cancellation is not allowed from the current state.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="403">User is not allowed to cancel this order.</response>
    /// <response code="404">Order not found.</response>
    [HttpPatch("{id:long}/cancel")]
    public async Task<ActionResult<OrderDetailsDto>> Cancel(long id, [FromBody] CancelOrderDto? request)
    {
        var userId = GetUserId();
        var result = await _orderService.CancelOrderAsync(userId, id, request);
        return Ok(result);
    }

    /// <summary>
    /// Confirms service fee payment for an order as the customer.
    /// </summary>
    /// <remarks>
    /// This only confirms the platform fee, not the product payment.
    /// Product payment remains offline at pickup.
    /// Order moves from AwaitingFee to Confirmed.
    /// </remarks>
    /// <param name="id">Order identifier.</param>
    /// <param name="request">Payment reference or related fee confirmation data.</param>
    /// <returns>Updated order details.</returns>
    /// <response code="200">Fee confirmed successfully.</response>
    /// <response code="400">Order is not in AwaitingFee state.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="404">Order or payment not found.</response>
    [HttpPatch("{id:long}/fee/confirm")]
    public async Task<ActionResult<OrderDetailsDto>> ConfirmFee(long id, [FromBody] ConfirmServiceFeeDto request)
    {
        var userId = GetUserId();
        var result = await _orderService.ConfirmServiceFeeAsync(userId, id, request);
        return Ok(result);
    }

    /// <summary>
    /// Completes an order after pickup verification by OTP code.
    /// </summary>
    /// <remarks>
    /// Completion is only allowed from Confirmed status.
    /// Farmer must provide the correct pickup code and fulfilled quantities for all items.
    /// </remarks>
    /// <param name="id">Order identifier.</param>
    /// <param name="request">Pickup code and fulfilled quantities.</param>
    /// <returns>Updated order details.</returns>
    /// <response code="200">Order completed successfully.</response>
    /// <response code="400">Order state, OTP, or fulfilled quantity validation failed.</response>
    /// <response code="401">User token is missing or invalid.</response>
    /// <response code="403">Order does not belong to the authenticated farmer.</response>
    /// <response code="404">Order not found.</response>
    [HttpPatch("{id:long}/complete")]
    public async Task<ActionResult<OrderDetailsDto>> Complete(long id, [FromBody] CompleteOrderDto request)
    {
        var userId = GetUserId();
        var result = await _orderService.CompleteOrderAsync(userId, id, request);
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