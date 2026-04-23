namespace DTO.Rating;

public class CreateRatingDto
{
    public long OrderId { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
}