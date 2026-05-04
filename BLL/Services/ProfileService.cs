using BLL.Exceptions;
using BLL.Interfaces;
using DAL.Data;
using DTO.Profile;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;

    public ProfileService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<object> GetMyProfileAsync(long userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
            throw new NotFoundException("User not found.");

        if (user.Role == DAL.Enums.RoleType.Farmer)
        {
            var farmer = await _context.Farmers
                .Include(x => x.User)
                .Include(x => x.PrimaryCity)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (farmer is null)
                throw new NotFoundException("Farmer profile not found.");

            return new FarmerProfileDto
            {
                UserId = farmer.UserId,
                FullName = farmer.User.FullName,
                PhoneNumber = farmer.User.PhoneNumber,
                Email = farmer.User.Email,
                ProfileImageUrl = farmer.User.ProfileImageUrl,
                DisplayName = farmer.DisplayName,
                Description = farmer.Description,
                PrimaryCityId = farmer.PrimaryCityId,
                PrimaryCityName = farmer.PrimaryCity?.Name,
                IsProfileCompleted = farmer.IsProfileCompleted
            };
        }

        if (user.Role == DAL.Enums.RoleType.Customer)
        {
            var customer = await _context.Customers
                .Include(x => x.User)
                .Include(x => x.PrimaryCity)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (customer is null)
                throw new NotFoundException("Customer profile not found.");

            return new CustomerProfileDto
            {
                UserId = customer.UserId,
                FullName = customer.User.FullName,
                PhoneNumber = customer.User.PhoneNumber,
                Email = customer.User.Email,
                ProfileImageUrl = customer.User.ProfileImageUrl,
                PrimaryCityId = customer.PrimaryCityId,
                PrimaryCityName = customer.PrimaryCity?.Name
            };
        }

        throw new BadRequestException("Unsupported profile type.");
    }

    public async Task<FarmerProfileDto> UpdateFarmerProfileAsync(long userId, UpdateFarmerProfileDto request)
    {
        var farmer = await _context.Farmers
            .Include(x => x.User)
            .Include(x => x.PrimaryCity)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (farmer is null)
            throw new NotFoundException("Farmer profile not found.");

        if (string.IsNullOrWhiteSpace(request.DisplayName))
            throw new BadRequestException("Display name is required.");

        if (request.PrimaryCityId.HasValue)
        {
            var cityExists = await _context.Cities.AnyAsync(x =>
                x.Id == request.PrimaryCityId.Value && x.IsActive);

            if (!cityExists)
                throw new BadRequestException("Selected city is invalid.");
        }

        farmer.DisplayName = request.DisplayName.Trim();
        farmer.Description = request.Description?.Trim();
        farmer.PrimaryCityId = request.PrimaryCityId;
        farmer.User.ProfileImageUrl = NormalizeProfileImageUrl(request.ProfileImageUrl);
        farmer.IsProfileCompleted = true;

        await _context.SaveChangesAsync();

        farmer = await _context.Farmers
            .Include(x => x.User)
            .Include(x => x.PrimaryCity)
            .FirstAsync(x => x.UserId == userId);

        return new FarmerProfileDto
        {
            UserId = farmer.UserId,
            FullName = farmer.User.FullName,
            PhoneNumber = farmer.User.PhoneNumber,
            Email = farmer.User.Email,
            ProfileImageUrl = farmer.User.ProfileImageUrl,
            DisplayName = farmer.DisplayName,
            Description = farmer.Description,
            PrimaryCityId = farmer.PrimaryCityId,
            PrimaryCityName = farmer.PrimaryCity?.Name,
            IsProfileCompleted = farmer.IsProfileCompleted
        };
    }

    public async Task<CustomerProfileDto> UpdateCustomerProfileAsync(long userId, UpdateCustomerProfileDto request)
    {
        var customer = await _context.Customers
            .Include(x => x.User)
            .Include(x => x.PrimaryCity)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (customer is null)
            throw new NotFoundException("Customer profile not found.");

        if (request.PrimaryCityId.HasValue)
        {
            var cityExists = await _context.Cities.AnyAsync(x =>
                x.Id == request.PrimaryCityId.Value && x.IsActive);

            if (!cityExists)
                throw new BadRequestException("Selected city is invalid.");
        }

        customer.PrimaryCityId = request.PrimaryCityId;
        customer.User.ProfileImageUrl = NormalizeProfileImageUrl(request.ProfileImageUrl);

        await _context.SaveChangesAsync();

        customer = await _context.Customers
            .Include(x => x.User)
            .Include(x => x.PrimaryCity)
            .FirstAsync(x => x.UserId == userId);

        return new CustomerProfileDto
        {
            UserId = customer.UserId,
            FullName = customer.User.FullName,
            PhoneNumber = customer.User.PhoneNumber,
            Email = customer.User.Email,
            ProfileImageUrl = customer.User.ProfileImageUrl,
            PrimaryCityId = customer.PrimaryCityId,
            PrimaryCityName = customer.PrimaryCity?.Name
        };
    }

    private static string? NormalizeProfileImageUrl(string? profileImageUrl)
    {
        if (string.IsNullOrWhiteSpace(profileImageUrl))
            return null;

        var trimmed = profileImageUrl.Trim();

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            throw new BadRequestException("Profile image URL is invalid.");

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            throw new BadRequestException("Profile image URL must be an HTTP or HTTPS URL.");

        return trimmed;
    }
}