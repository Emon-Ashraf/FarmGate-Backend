using DAL.Data;
using DAL.Interfaces;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(long id)
    {
        return await _context.Orders
            .Include(x => x.Payment)
            .Include(x => x.Rating)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Order?> GetByIdWithItemsAsync(long id)
    {
        return await _context.Orders
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                    .ThenInclude(x => x.PickupLocation)
                        .ThenInclude(x => x.City)
            .Include(x => x.Customer)
                .ThenInclude(x => x.User)
            .Include(x => x.Farmer)
                .ThenInclude(x => x.User)
            .Include(x => x.Payment)
            .Include(x => x.Rating)
            .Include(x => x.IssueReports)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Order?> GetByIdForCustomerAsync(long orderId, long customerId)
    {
        return await _context.Orders
            .Include(x => x.Items)
            .Include(x => x.Payment)
            .Include(x => x.Rating)
            .Include(x => x.IssueReports)
            .FirstOrDefaultAsync(x => x.Id == orderId && x.CustomerId == customerId);
    }

    public async Task<Order?> GetByIdForFarmerAsync(long orderId, long farmerId)
    {
        return await _context.Orders
            .Include(x => x.Items)
            .Include(x => x.Payment)
            .Include(x => x.Rating)
            .Include(x => x.IssueReports)
            .FirstOrDefaultAsync(x => x.Id == orderId && x.FarmerId == farmerId);
    }

    public async Task<List<Order>> GetCustomerOrdersAsync(long customerId)
    {
        return await _context.Orders
            .Include(x => x.Farmer)
                .ThenInclude(x => x.User)
            .Include(x => x.Payment)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetFarmerOrdersAsync(long farmerId)
    {
        return await _context.Orders
            .Include(x => x.Customer)
                .ThenInclude(x => x.User)
            .Include(x => x.Payment)
            .Where(x => x.FarmerId == farmerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
    }

    public void Update(Order order)
    {
        _context.Orders.Update(order);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}