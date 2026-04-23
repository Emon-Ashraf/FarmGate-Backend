using DAL.Enums;

namespace DAL.Models;

public class User : BaseEntity
{
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = null!;
    public RoleType Role { get; set; }
    public bool IsActive { get; set; } = true;

    public Farmer? FarmerProfile { get; set; }
    public Customer? CustomerProfile { get; set; }
    public Admin? AdminProfile { get; set; }
}