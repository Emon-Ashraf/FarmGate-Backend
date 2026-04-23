using DAL.Data;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        return await _context.Users
            .Include(x => x.FarmerProfile)
            .Include(x => x.CustomerProfile)
            .Include(x => x.AdminProfile)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Users
            .Include(x => x.FarmerProfile)
            .Include(x => x.CustomerProfile)
            .Include(x => x.AdminProfile)
            .FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);
    }

    public async Task<User?> GetByPhoneOrEmailAsync(string login)
    {
        return await _context.Users
            .Include(x => x.FarmerProfile)
            .Include(x => x.CustomerProfile)
            .Include(x => x.AdminProfile)
            .FirstOrDefaultAsync(x => x.PhoneNumber == login || (x.Email != null && x.Email == login));
    }

    public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Users.AnyAsync(x => x.PhoneNumber == phoneNumber);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}