namespace DAL.Models;

public class Admin
{
    public long UserId { get; set; }

    public User User { get; set; } = null!;
}