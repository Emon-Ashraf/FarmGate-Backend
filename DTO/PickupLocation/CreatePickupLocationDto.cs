namespace DTO.PickupLocation;

public class CreatePickupLocationDto
{
    public long CityId { get; set; }
    public string AreaName { get; set; } = null!;
    public string AddressLine { get; set; } = null!;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Instructions { get; set; }
}