using DAL.Data;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly ApplicationDbContext _context;

    public RatingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Rating?> GetByIdAsync(long id)
    {
        return await _context.Ratings.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Rating?> GetByOrderIdAsync(long orderId)
    {
        return await _context.Ratings.FirstOrDefaultAsync(x => x.OrderId == orderId);
    }

    public async Task<List<Rating>> GetByFarmerIdAsync(long farmerId)
    {
        return await _context.Ratings
            .Include(x => x.Customer)
                .ThenInclude(x => x.User)
            .Where(x => x.FarmerId == farmerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Rating rating)
    {
        await _context.Ratings.AddAsync(rating);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}