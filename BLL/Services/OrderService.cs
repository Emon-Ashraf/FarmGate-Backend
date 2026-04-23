using BLL.Exceptions;
using BLL.Interfaces;
using DAL.Data;
using DAL.Enums;
using DAL.Interfaces;
using DAL.Models;
using DTO.Order;
using DTO.Payment;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class OrderService : IOrderService
{
    private const decimal ServiceFeeRate = 0.05m;

    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ApplicationDbContext _context;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IPaymentRepository paymentRepository,
        ApplicationDbContext context)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _paymentRepository = paymentRepository;
        _context = context;
    }

    public async Task<OrderDetailsDto> CreateOrderAsync(long userId, CreateOrderDto request)
    {
        var customer = await _context.Customers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (customer is null)
            throw new NotFoundException("Customer profile not found.");

        if (request.Items is null || request.Items.Count == 0)
            throw new BadRequestException("Order must contain at least one item.");

        var duplicateProductIds = request.Items
            .GroupBy(x => x.ProductId)
            .Any(g => g.Count() > 1);

        if (duplicateProductIds)
            throw new BadRequestException("Duplicate product ids are not allowed in the same order.");

        var productIds = request.Items.Select(x => x.ProductId).ToList();

        var products = await _productRepository.Query()
            .Where(x => productIds.Contains(x.Id))
            .ToListAsync();

        if (products.Count != productIds.Count)
            throw new BadRequestException("One or more selected products do not exist.");

        if (products.Any(x => !x.IsActive))
            throw new BadRequestException("Inactive products cannot be ordered.");

        if (products.Any(x => !x.PickupLocation.IsActive))
            throw new BadRequestException("Products from inactive pickup locations cannot be ordered.");

        var farmerIds = products.Select(x => x.FarmerId).Distinct().ToList();
        if (farmerIds.Count != 1)
            throw new BadRequestException("All order items must belong to the same farmer.");

        var pickupLocationIds = products.Select(x => x.PickupLocationId).Distinct().ToList();
        if (pickupLocationIds.Count != 1)
            throw new BadRequestException("All order items must belong to the same pickup location.");

        var actualPickupLocationId = pickupLocationIds.Single();
        if (actualPickupLocationId != request.PickupLocationId)
            throw new BadRequestException("Request pickup location does not match product pickup location.");

        var pickupLocation = products.First().PickupLocation;
        var farmer = products.First().Farmer;

        decimal estimatedProductTotal = 0m;
        var orderItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            var product = products.First(x => x.Id == item.ProductId);

            ValidateQuantity(item.Quantity, product.UnitType);

            if (item.Quantity > product.AvailableQuantity)
                throw new BadRequestException($"Requested quantity exceeds available stock for product '{product.Name}'.");

            estimatedProductTotal += product.PricePerUnit * item.Quantity;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductNameSnapshot = product.Name,
                UnitTypeSnapshot = product.UnitType,
                UnitPriceSnapshot = product.PricePerUnit,
                OrderedQuantity = item.Quantity
            });
        }

        var serviceFeeAmount = decimal.Round(estimatedProductTotal * ServiceFeeRate, 2, MidpointRounding.AwayFromZero);

        var order = new Order
        {
            CustomerId = customer.UserId,
            FarmerId = farmer.UserId,
            PickupLocationId = pickupLocation.Id,
            Status = OrderStatus.Pending,
            EstimatedProductTotal = estimatedProductTotal,
            ServiceFeeAmount = serviceFeeAmount,
            PickupCode = GeneratePickupCode(),
            PickupCodeExpiresAt = DateTime.UtcNow.AddDays(1),
            PickupDueAt = DateTime.UtcNow.AddDays(2),
            PickupCitySnapshot = pickupLocation.City.Name,
            PickupAreaSnapshot = pickupLocation.AreaName,
            PickupAddressSnapshot = pickupLocation.AddressLine,
            PickupInstructionsSnapshot = pickupLocation.Instructions,
            FarmerPhoneSnapshot = farmer.User.PhoneNumber,
            Items = orderItems
        };

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        return await MapToOrderDetailsDtoAsync(order.Id);
    }

    public async Task<List<OrderSummaryDto>> GetCustomerOrdersAsync(long userId)
    {
        var orders = await _orderRepository.GetCustomerOrdersAsync(userId);

        return orders.Select(x => new OrderSummaryDto
        {
            Id = x.Id,
            Status = x.Status,
            CounterpartyName = x.Farmer.User.FullName,
            EstimatedProductTotal = x.EstimatedProductTotal,
            ActualProductTotal = x.ActualProductTotal,
            ServiceFeeAmount = x.ServiceFeeAmount,
            PickupDueAt = x.PickupDueAt,
            CreatedAt = x.CreatedAt
        }).ToList();
    }

    public async Task<List<OrderSummaryDto>> GetFarmerOrdersAsync(long userId)
    {
        var orders = await _orderRepository.GetFarmerOrdersAsync(userId);

        return orders.Select(x => new OrderSummaryDto
        {
            Id = x.Id,
            Status = x.Status,
            CounterpartyName = x.Customer.User.FullName,
            EstimatedProductTotal = x.EstimatedProductTotal,
            ActualProductTotal = x.ActualProductTotal,
            ServiceFeeAmount = x.ServiceFeeAmount,
            PickupDueAt = x.PickupDueAt,
            CreatedAt = x.CreatedAt
        }).ToList();
    }

    public async Task<OrderDetailsDto> GetOrderDetailsAsync(long userId, long orderId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
            throw new NotFoundException("User not found.");

        Order? order = null;

        if (user.Role == RoleType.Customer)
            order = await _orderRepository.GetByIdForCustomerAsync(orderId, userId);
        else if (user.Role == RoleType.Farmer)
            order = await _orderRepository.GetByIdForFarmerAsync(orderId, userId);
        else if (user.Role == RoleType.Admin)
            order = await _orderRepository.GetByIdWithItemsAsync(orderId);

        if (order is null)
            throw new NotFoundException("Order not found.");

        return await MapToOrderDetailsDtoAsync(order.Id);
    }

    public async Task<OrderDetailsDto> AcceptOrderAsync(long userId, long orderId)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(orderId);
        if (order is null)
            throw new NotFoundException("Order not found.");

        if (order.FarmerId != userId)
            throw new ForbiddenException("You cannot accept this order.");

        EnsureStatus(order, OrderStatus.Pending, "Only pending orders can be accepted.");

        foreach (var item in order.Items)
        {
            var product = item.Product;

            if (!product.IsActive)
                throw new BadRequestException($"Product '{product.Name}' is inactive.");

            if (item.OrderedQuantity > product.AvailableQuantity)
                throw new BadRequestException($"Insufficient stock for product '{product.Name}'.");
        }

        foreach (var item in order.Items)
        {
            item.Product.AvailableQuantity -= item.OrderedQuantity;
        }

        order.Status = OrderStatus.AwaitingFee;
        order.StockReservedAt = DateTime.UtcNow;
        order.PickupDueAt = DateTime.UtcNow.AddDays(2);

        var payment = await _paymentRepository.GetByOrderIdAsync(order.Id);
        if (payment is null)
        {
            payment = new Payment
            {
                OrderId = order.Id,
                Amount = order.ServiceFeeAmount,
                Status = PaymentStatus.Pending
            };

            await _paymentRepository.AddAsync(payment);
        }
        else
        {
            payment.Amount = order.ServiceFeeAmount;
            payment.Status = PaymentStatus.Pending;
            payment.TransactionReference = null;
            payment.PaidAt = null;
            _paymentRepository.Update(payment);
        }

        _orderRepository.Update(order);
        await _context.SaveChangesAsync();

        return await MapToOrderDetailsDtoAsync(order.Id);
    }

    public async Task<OrderDetailsDto> RejectOrderAsync(long userId, long orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
            throw new NotFoundException("Order not found.");

        if (order.FarmerId != userId)
            throw new ForbiddenException("You cannot reject this order.");

        EnsureStatus(order, OrderStatus.Pending, "Only pending orders can be rejected.");

        order.Status = OrderStatus.Rejected;
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        return await MapToOrderDetailsDtoAsync(order.Id);
    }

    public async Task<OrderDetailsDto> CancelOrderAsync(long userId, long orderId, CancelOrderDto? request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
            throw new NotFoundException("User not found.");

        Order? order = null;

        if (user.Role == RoleType.Customer)
            order = await _orderRepository.GetByIdForCustomerAsync(orderId, userId);
        else if (user.Role == RoleType.Farmer)
            order = await _orderRepository.GetByIdForFarmerAsync(orderId, userId);
        else if (user.Role == RoleType.Admin)
            order = await _orderRepository.GetByIdWithItemsAsync(orderId);

        if (order is null)
            throw new NotFoundException("Order not found.");

        if (order.Status != OrderStatus.Pending &&
            order.Status != OrderStatus.AwaitingFee &&
            order.Status != OrderStatus.Confirmed)
        {
            throw new BadRequestException("This order cannot be cancelled from its current state.");
        }

        CancellationReason reason = user.Role switch
        {
            RoleType.Customer when order.Status == OrderStatus.Pending || order.Status == OrderStatus.AwaitingFee
                => CancellationReason.CustomerCancelled,

            RoleType.Farmer when order.Status == OrderStatus.AwaitingFee
                => CancellationReason.FarmerUnavailable,

            RoleType.Admin when order.Status == OrderStatus.Confirmed
                => CancellationReason.AdminCancelled,

            RoleType.Admin when order.Status == OrderStatus.Pending || order.Status == OrderStatus.AwaitingFee
                => CancellationReason.AdminCancelled,

            _ => throw new ForbiddenException("You cannot cancel this order in the current state.")
        };

        if (order.StockReservedAt.HasValue && order.Status != OrderStatus.Completed)
        {
            var fullOrder = await _orderRepository.GetByIdWithItemsAsync(order.Id);
            if (fullOrder is null)
                throw new NotFoundException("Order not found.");

            foreach (var item in fullOrder.Items)
            {
                item.Product.AvailableQuantity += item.OrderedQuantity;
            }
        }

        order.Status = OrderStatus.Cancelled;
        order.CancellationReason = reason;

        _orderRepository.Update(order);
        await _context.SaveChangesAsync();

        return await MapToOrderDetailsDtoAsync(order.Id);
    }

    public async Task<OrderDetailsDto> ConfirmServiceFeeAsync(long userId, long orderId, ConfirmServiceFeeDto request)
    {
        var order = await _orderRepository.GetByIdForCustomerAsync(orderId, userId);
        if (order is null)
            throw new NotFoundException("Order not found.");

        EnsureStatus(order, OrderStatus.AwaitingFee, "Service fee can only be confirmed from AwaitingFee status.");

        var payment = await _paymentRepository.GetByOrderIdAsync(order.Id);
        if (payment is null)
            throw new NotFoundException("Payment record not found.");

        if (payment.Status == PaymentStatus.Paid)
            return await MapToOrderDetailsDtoAsync(order.Id);

        payment.Status = PaymentStatus.Paid;
        payment.TransactionReference = request.PaymentReference;
        payment.PaidAt = DateTime.UtcNow;

        order.FeePaidAt = payment.PaidAt;
        order.Status = OrderStatus.Confirmed;

        _paymentRepository.Update(payment);
        _orderRepository.Update(order);

        await _context.SaveChangesAsync();

        return await MapToOrderDetailsDtoAsync(order.Id);
    }

    public async Task<OrderDetailsDto> CompleteOrderAsync(long userId, long orderId, CompleteOrderDto request)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(orderId);
        if (order is null)
            throw new NotFoundException("Order not found.");

        if (order.FarmerId != userId)
            throw new ForbiddenException("You cannot complete this order.");

        EnsureStatus(order, OrderStatus.Confirmed, "Only confirmed orders can be completed.");

        if (order.PickupCodeUsedAt.HasValue)
            throw new BadRequestException("Pickup code has already been used.");

        if (order.PickupCodeExpiresAt < DateTime.UtcNow)
            throw new BadRequestException("Pickup code has expired.");

        if (request.PickupCode != order.PickupCode)
            throw new BadRequestException("Invalid pickup code.");

        if (request.Items is null || request.Items.Count == 0)
            throw new BadRequestException("Completion items are required.");

        if (request.Items.Count != order.Items.Count)
            throw new BadRequestException("All order items must be provided for completion.");

        decimal actualProductTotal = 0m;

        foreach (var item in order.Items)
        {
            var completionItem = request.Items.FirstOrDefault(x => x.OrderItemId == item.Id);
            if (completionItem is null)
                throw new BadRequestException($"Missing fulfilled quantity for order item {item.Id}.");

            ValidateQuantity(completionItem.FulfilledQuantity, item.UnitTypeSnapshot);

            if (completionItem.FulfilledQuantity < 0)
                throw new BadRequestException("Fulfilled quantity cannot be negative.");

            if (completionItem.FulfilledQuantity > item.OrderedQuantity)
                throw new BadRequestException("Fulfilled quantity cannot exceed ordered quantity.");

            item.FulfilledQuantity = completionItem.FulfilledQuantity;
            actualProductTotal += item.UnitPriceSnapshot * completionItem.FulfilledQuantity;
        }

        order.ActualProductTotal = actualProductTotal;
        order.PickupCodeUsedAt = DateTime.UtcNow;
        order.Status = OrderStatus.Completed;

        _orderRepository.Update(order);
        await _context.SaveChangesAsync();

        return await MapToOrderDetailsDtoAsync(order.Id);
    }

    private async Task<OrderDetailsDto> MapToOrderDetailsDtoAsync(long orderId)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(orderId);
        if (order is null)
            throw new NotFoundException("Order not found.");

        return new OrderDetailsDto
        {
            Id = order.Id,
            Status = order.Status,
            CancellationReason = order.CancellationReason,
            CustomerName = order.Customer.User.FullName,
            FarmerName = order.Farmer.User.FullName,
            EstimatedProductTotal = order.EstimatedProductTotal,
            ActualProductTotal = order.ActualProductTotal,
            ServiceFeeAmount = order.ServiceFeeAmount,
            FeePaidAt = order.FeePaidAt,
            PickupDueAt = order.PickupDueAt,
            PickupCity = order.PickupCitySnapshot,
            PickupArea = order.PickupAreaSnapshot,
            PickupAddress = order.PickupAddressSnapshot,
            PickupInstructions = order.PickupInstructionsSnapshot,
            FarmerPhone = order.FarmerPhoneSnapshot,
            PickupCode = order.Status == OrderStatus.Confirmed ? order.PickupCode : null,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(x => new OrderItemDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.ProductNameSnapshot,
                UnitType = x.UnitTypeSnapshot,
                UnitPrice = x.UnitPriceSnapshot,
                OrderedQuantity = x.OrderedQuantity,
                FulfilledQuantity = x.FulfilledQuantity
            }).ToList(),
            Payment = order.Payment is null
                ? null
                : new PaymentDto
                {
                    Id = order.Payment.Id,
                    OrderId = order.Payment.OrderId,
                    Amount = order.Payment.Amount,
                    Status = order.Payment.Status,
                    TransactionReference = order.Payment.TransactionReference,
                    PaidAt = order.Payment.PaidAt
                }
        };
    }

    private static void ValidateQuantity(decimal quantity, UnitType unitType)
    {
        if (quantity <= 0)
            throw new BadRequestException("Quantity must be greater than zero.");

        if (decimal.Round(quantity, 2) != quantity)
            throw new BadRequestException("Quantity cannot have more than 2 decimal places.");

        if (unitType is UnitType.Piece or UnitType.Dozen or UnitType.Bundle)
        {
            if (quantity != decimal.Truncate(quantity))
                throw new BadRequestException("Selected unit type requires an integer quantity.");
        }
    }

    private static void EnsureStatus(Order order, OrderStatus expectedStatus, string message)
    {
        if (order.Status == OrderStatus.Completed ||
            order.Status == OrderStatus.Rejected ||
            order.Status == OrderStatus.Cancelled)
        {
            throw new BadRequestException("No further state transition is allowed from a terminal state.");
        }

        if (order.Status != expectedStatus)
            throw new BadRequestException(message);
    }

    private static string GeneratePickupCode()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }
}