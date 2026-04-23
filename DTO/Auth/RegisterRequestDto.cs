using DAL.Enums;

namespace DTO.Auth;

public class RegisterRequestDto
{
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Email { get; set; }
    public string Password { get; set; } = null!;
    public RoleType Role { get; set; }

    public long? PrimaryCityId { get; set; }
    public string? DisplayName { get; set; }
}