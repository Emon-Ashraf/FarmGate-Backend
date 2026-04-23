using DTO.Order;

namespace BLL.Interfaces;

public interface IOrderService
{
    Task<OrderDetailsDto> CreateOrderAsync(long userId, CreateOrderDto request);
    Task<List<OrderSummaryDto>> GetCustomerOrdersAsync(long userId);
    Task<List<OrderSummaryDto>> GetFarmerOrdersAsync(long userId);
    Task<OrderDetailsDto> GetOrderDetailsAsync(long userId, long orderId);
    Task<OrderDetailsDto> AcceptOrderAsync(long userId, long orderId);
    Task<OrderDetailsDto> RejectOrderAsync(long userId, long orderId);
    Task<OrderDetailsDto> CancelOrderAsync(long userId, long orderId, CancelOrderDto? request);
    Task<OrderDetailsDto> ConfirmServiceFeeAsync(long userId, long orderId, ConfirmServiceFeeDto request);
    Task<OrderDetailsDto> CompleteOrderAsync(long userId, long orderId, CompleteOrderDto request);
}