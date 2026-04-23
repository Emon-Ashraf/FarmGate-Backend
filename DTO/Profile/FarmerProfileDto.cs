namespace DTO.Profile;

public class FarmerProfileDto
{
    public long UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Email { get; set; }

    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public long? PrimaryCityId { get; set; }
    public string? PrimaryCityName { get; set; }
    public bool IsProfileCompleted { get; set; }
}