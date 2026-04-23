using DAL.Data;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class PickupLocationRepository : IPickupLocationRepository
{
    private readonly ApplicationDbContext _context;

    public PickupLocationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PickupLocation?> GetByIdAsync(long id)
    {
        return await _context.PickupLocations
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<PickupLocation?> GetByIdWithCityAsync(long id)
    {
        return await _context.PickupLocations
            .Include(x => x.City)
            .Include(x => x.Farmer)
                .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<PickupLocation>> GetByFarmerIdAsync(long farmerId)
    {
        return await _context.PickupLocations
            .Include(x => x.City)
            .Where(x => x.FarmerId == farmerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(PickupLocation pickupLocation)
    {
        await _context.PickupLocations.AddAsync(pickupLocation);
    }

    public void Update(PickupLocation pickupLocation)
    {
        _context.PickupLocations.Update(pickupLocation);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}