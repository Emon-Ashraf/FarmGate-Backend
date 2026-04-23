using DAL.Models;

namespace DAL.Interfaces;

public interface IRatingRepository
{
    Task<Rating?> GetByIdAsync(long id);
    Task<Rating?> GetByOrderIdAsync(long orderId);
    Task<List<Rating>> GetByFarmerIdAsync(long farmerId);
    Task AddAsync(Rating rating);
    Task SaveChangesAsync();
}