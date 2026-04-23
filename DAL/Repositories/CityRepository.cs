using DAL.Data;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class CityRepository : ICityRepository
{
    private readonly ApplicationDbContext _context;

    public CityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<City>> GetAllActiveAsync()
    {
        return await _context.Cities
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<City?> GetByIdAsync(long id)
    {
        return await _context.Cities.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<City?> GetActiveByIdAsync(long id)
    {
        return await _context.Cities
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
    }
}