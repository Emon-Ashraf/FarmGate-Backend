namespace DTO.Profile;

public class CustomerProfileDto
{
    public long UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Email { get; set; }

    public long? PrimaryCityId { get; set; }
    public string? PrimaryCityName { get; set; }
}