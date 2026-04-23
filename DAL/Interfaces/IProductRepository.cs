using DAL.Models;

namespace DAL.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(long id);
    Task<Product?> GetByIdWithRelationsAsync(long id);
    Task<List<Product>> GetByFarmerIdAsync(long farmerId);
    IQueryable<Product> Query();
    Task AddAsync(Product product);
    void Update(Product product);
    Task SaveChangesAsync();
}