using DAL.Enums;

namespace DTO.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = null!;
    public long UserId { get; set; }
    public RoleType Role { get; set; }
    public string FullName { get; set; } = null!;
}