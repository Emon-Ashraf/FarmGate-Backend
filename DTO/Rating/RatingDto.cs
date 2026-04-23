namespace DTO.Rating;

public class RatingDto
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long CustomerId { get; set; }
    public long FarmerId { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
    public string? CustomerName { get; set; }
    public DateTime CreatedAt { get; set; }
}