using DTO.PickupLocation;

namespace BLL.Interfaces;

public interface IPickupLocationService
{
    Task<PickupLocationDto> CreateAsync(long userId, CreatePickupLocationDto request);
    Task<PickupLocationDto> UpdateAsync(long userId, long pickupLocationId, UpdatePickupLocationDto request);
    Task<List<PickupLocationDto>> GetMyLocationsAsync(long userId);
    Task DeactivateAsync(long userId, long pickupLocationId);
}