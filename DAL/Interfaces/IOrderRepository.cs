using DAL.Models;

namespace DAL.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(long id);
    Task<Order?> GetByIdWithItemsAsync(long id);
    Task<Order?> GetByIdForCustomerAsync(long orderId, long customerId);
    Task<Order?> GetByIdForFarmerAsync(long orderId, long farmerId);
    Task<List<Order>> GetCustomerOrdersAsync(long customerId);
    Task<List<Order>> GetFarmerOrdersAsync(long farmerId);
    Task AddAsync(Order order);
    void Update(Order order);
    Task SaveChangesAsync();
}