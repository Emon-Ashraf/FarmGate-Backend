using DTO.City;

namespace BLL.Interfaces;

public interface ICityService
{
    Task<List<CityListItemDto>> GetCitiesAsync();
}