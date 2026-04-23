using DAL.Models;

namespace DAL.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<User?> GetByPhoneOrEmailAsync(string login);
    Task<bool> ExistsByPhoneNumberAsync(string phoneNumber);
    Task AddAsync(User user);
    void Update(User user);
    Task SaveChangesAsync();
}