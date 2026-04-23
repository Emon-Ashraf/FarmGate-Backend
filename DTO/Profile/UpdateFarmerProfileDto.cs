namespace DTO.Profile;

public class UpdateFarmerProfileDto
{
    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public long? PrimaryCityId { get; set; }
}