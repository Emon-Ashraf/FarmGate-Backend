using BLL.Interfaces;
using DAL.Interfaces;
using DTO.City;

namespace BLL.Services;

public class CityService : ICityService
{
    private readonly ICityRepository _cityRepository;

    public CityService(ICityRepository cityRepository)
    {
        _cityRepository = cityRepository;
    }

    public async Task<List<CityListItemDto>> GetCitiesAsync()
    {
        var cities = await _cityRepository.GetAllActiveAsync();

        return cities.Select(x => new CityListItemDto
        {
            Id = x.Id,
            Name = x.Name,
            RegionName = x.RegionName
        }).ToList();
    }
}