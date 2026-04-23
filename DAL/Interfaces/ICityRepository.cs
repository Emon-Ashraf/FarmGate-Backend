using DAL.Models;

namespace DAL.Interfaces;

public interface ICityRepository
{
    Task<List<City>> GetAllActiveAsync();
    Task<City?> GetByIdAsync(long id);
    Task<City?> GetActiveByIdAsync(long id);
}