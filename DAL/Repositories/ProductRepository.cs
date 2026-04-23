using DAL.Data;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(long id)
    {
        return await _context.Products
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Product?> GetByIdWithRelationsAsync(long id)
    {
        return await _context.Products
            .Include(x => x.PickupLocation)
                .ThenInclude(x => x.City)
            .Include(x => x.Farmer)
                .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Product>> GetByFarmerIdAsync(long farmerId)
    {
        return await _context.Products
            .Include(x => x.PickupLocation)
                .ThenInclude(x => x.City)
            .Where(x => x.FarmerId == farmerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public IQueryable<Product> Query()
    {
        return _context.Products
            .Include(x => x.PickupLocation)
                .ThenInclude(x => x.City)
            .Include(x => x.Farmer)
                .ThenInclude(x => x.User)
            .AsQueryable();
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}