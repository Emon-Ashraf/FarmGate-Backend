using DAL.Models;

namespace DAL.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(long id);
    Task<Payment?> GetByOrderIdAsync(long orderId);
    Task AddAsync(Payment payment);
    void Update(Payment payment);
    Task SaveChangesAsync();
}