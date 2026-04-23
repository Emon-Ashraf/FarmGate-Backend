using BLL.Exceptions;
using BLL.Interfaces;
using DAL.Data;
using DAL.Interfaces;
using DAL.Models;
using DTO.PickupLocation;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class PickupLocationService : IPickupLocationService
{
    private readonly IPickupLocationRepository _pickupLocationRepository;
    private readonly ApplicationDbContext _context;

    public PickupLocationService(
        IPickupLocationRepository pickupLocationRepository,
        ApplicationDbContext context)
    {
        _pickupLocationRepository = pickupLocationRepository;
        _context = context;
    }

    public async Task<PickupLocationDto> CreateAsync(long userId, CreatePickupLocationDto request)
    {
        var farmer = await _context.Farmers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (farmer is null)
            throw new NotFoundException("Farmer profile not found.");

        var city = await _context.Cities.FirstOrDefaultAsync(x => x.Id == request.CityId && x.IsActive);
        if (city is null)
            throw new BadRequestException("City is invalid or inactive.");

        var pickupLocation = new PickupLocation
        {
            FarmerId = farmer.UserId,
            CityId = request.CityId,
            AreaName = request.AreaName,
            AddressLine = request.AddressLine,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Instructions = request.Instructions,
            IsActive = true
        };

        await _pickupLocationRepository.AddAsync(pickupLocation);
        await _pickupLocationRepository.SaveChangesAsync();

        return new PickupLocationDto
        {
            Id = pickupLocation.Id,
            CityId = city.Id,
            CityName = city.Name,
            AreaName = pickupLocation.AreaName,
            AddressLine = pickupLocation.AddressLine,
            Latitude = pickupLocation.Latitude,
            Longitude = pickupLocation.Longitude,
            Instructions = pickupLocation.Instructions,
            IsActive = pickupLocation.IsActive
        };
    }

    public async Task<PickupLocationDto> UpdateAsync(long userId, long pickupLocationId, UpdatePickupLocationDto request)
    {
        var pickupLocation = await _pickupLocationRepository.GetByIdWithCityAsync(pickupLocationId);
        if (pickupLocation is null)
            throw new NotFoundException("Pickup location not found.");

        if (pickupLocation.FarmerId != userId)
            throw new ForbiddenException("You cannot update this pickup location.");

        var city = await _context.Cities.FirstOrDefaultAsync(x => x.Id == request.CityId && x.IsActive);
        if (city is null)
            throw new BadRequestException("City is invalid or inactive.");

        pickupLocation.CityId = request.CityId;
        pickupLocation.AreaName = request.AreaName;
        pickupLocation.AddressLine = request.AddressLine;
        pickupLocation.Latitude = request.Latitude;
        pickupLocation.Longitude = request.Longitude;
        pickupLocation.Instructions = request.Instructions;

        _pickupLocationRepository.Update(pickupLocation);
        await _pickupLocationRepository.SaveChangesAsync();

        return new PickupLocationDto
        {
            Id = pickupLocation.Id,
            CityId = city.Id,
            CityName = city.Name,
            AreaName = pickupLocation.AreaName,
            AddressLine = pickupLocation.AddressLine,
            Latitude = pickupLocation.Latitude,
            Longitude = pickupLocation.Longitude,
            Instructions = pickupLocation.Instructions,
            IsActive = pickupLocation.IsActive
        };
    }

    public async Task<List<PickupLocationDto>> GetMyLocationsAsync(long userId)
    {
        var locations = await _pickupLocationRepository.GetByFarmerIdAsync(userId);

        return locations.Select(x => new PickupLocationDto
        {
            Id = x.Id,
            CityId = x.CityId,
            CityName = x.City.Name,
            AreaName = x.AreaName,
            AddressLine = x.AddressLine,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
            Instructions = x.Instructions,
            IsActive = x.IsActive
        }).ToList();
    }

    public async Task DeactivateAsync(long userId, long pickupLocationId)
    {
        var pickupLocation = await _pickupLocationRepository.GetByIdAsync(pickupLocationId);
        if (pickupLocation is null)
            throw new NotFoundException("Pickup location not found.");

        if (pickupLocation.FarmerId != userId)
            throw new ForbiddenException("You cannot deactivate this pickup location.");

        var hasActiveOrders = await _context.Orders.AnyAsync(x =>
            x.PickupLocationId == pickupLocationId &&
            (x.Status == DAL.Enums.OrderStatus.AwaitingFee || x.Status == DAL.Enums.OrderStatus.Confirmed));

        if (hasActiveOrders)
            throw new BadRequestException("Cannot deactivate pickup location with active orders.");

        pickupLocation.IsActive = false;

        _pickupLocationRepository.Update(pickupLocation);
        await _pickupLocationRepository.SaveChangesAsync();
    }
}