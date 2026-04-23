using DTO.Rating;

namespace BLL.Interfaces;

public interface IRatingService
{
    Task<RatingDto> CreateAsync(long userId, CreateRatingDto request);
    Task<List<RatingDto>> GetForFarmerAsync(long farmerId);
}