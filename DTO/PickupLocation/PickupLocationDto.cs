namespace DTO.PickupLocation;

public class PickupLocationDto
{
    public long Id { get; set; }
    public long CityId { get; set; }
    public string CityName { get; set; } = null!;
    public string AreaName { get; set; } = null!;
    public string AddressLine { get; set; } = null!;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Instructions { get; set; }
    public bool IsActive { get; set; }
}