using BLL.Exceptions;
using BLL.Interfaces;
using DAL.Data;
using DAL.Enums;
using DAL.Interfaces;
using DAL.Models;
using DTO.Rating;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ApplicationDbContext _context;

    public RatingService(
        IRatingRepository ratingRepository,
        IOrderRepository orderRepository,
        ApplicationDbContext context)
    {
        _ratingRepository = ratingRepository;
        _orderRepository = orderRepository;
        _context = context;
    }

    public async Task<RatingDto> CreateAsync(long userId, CreateRatingDto request)
    {
        if (request.Score < 1 || request.Score > 5)
            throw new BadRequestException("Score must be between 1 and 5.");

        var customer = await _context.Customers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (customer is null)
            throw new NotFoundException("Customer profile not found.");

        var order = await _orderRepository.GetByIdForCustomerAsync(request.OrderId, userId);
        if (order is null)
            throw new NotFoundException("Order not found.");

        if (order.Status != OrderStatus.Completed)
            throw new BadRequestException("Rating is allowed only after order completion.");

        var existingRating = await _ratingRepository.GetByOrderIdAsync(request.OrderId);
        if (existingRating is not null)
            throw new BadRequestException("A rating already exists for this order.");

        var rating = new Rating
        {
            OrderId = order.Id,
            CustomerId = userId,
            FarmerId = order.FarmerId,
            Score = request.Score,
            Comment = request.Comment
        };

        await _ratingRepository.AddAsync(rating);
        await _ratingRepository.SaveChangesAsync();

        return new RatingDto
        {
            Id = rating.Id,
            OrderId = rating.OrderId,
            CustomerId = rating.CustomerId,
            FarmerId = rating.FarmerId,
            Score = rating.Score,
            Comment = rating.Comment,
            CustomerName = customer.User.FullName,
            CreatedAt = rating.CreatedAt
        };
    }

    public async Task<List<RatingDto>> GetForFarmerAsync(long farmerId)
    {
        var ratings = await _ratingRepository.GetByFarmerIdAsync(farmerId);

        return ratings.Select(x => new RatingDto
        {
            Id = x.Id,
            OrderId = x.OrderId,
            CustomerId = x.CustomerId,
            FarmerId = x.FarmerId,
            Score = x.Score,
            Comment = x.Comment,
            CustomerName = x.Customer.User.FullName,
            CreatedAt = x.CreatedAt
        }).ToList();
    }
}