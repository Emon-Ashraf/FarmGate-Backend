namespace DTO.City;

public class CityListItemDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? RegionName { get; set; }
}