using DAL.Models;

namespace DAL.Interfaces;

public interface IPickupLocationRepository
{
    Task<PickupLocation?> GetByIdAsync(long id);
    Task<PickupLocation?> GetByIdWithCityAsync(long id);
    Task<List<PickupLocation>> GetByFarmerIdAsync(long farmerId);
    Task AddAsync(PickupLocation pickupLocation);
    void Update(PickupLocation pickupLocation);
    Task SaveChangesAsync();
}