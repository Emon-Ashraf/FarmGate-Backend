using BLL.Exceptions;
using BLL.Interfaces;
using DAL.Data;
using DAL.Enums;
using DAL.Interfaces;
using DAL.Models;
using DTO.Product;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IPickupLocationRepository _pickupLocationRepository;
    private readonly ApplicationDbContext _context;

    public ProductService(
        IProductRepository productRepository,
        IPickupLocationRepository pickupLocationRepository,
        ApplicationDbContext context)
    {
        _productRepository = productRepository;
        _pickupLocationRepository = pickupLocationRepository;
        _context = context;
    }

    public async Task<ProductDetailsDto> CreateAsync(long userId, CreateProductDto request)
    {
        var farmer = await _context.Farmers
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (farmer is null)
            throw new NotFoundException("Farmer profile not found.");

        if (!farmer.IsProfileCompleted)
            throw new BadRequestException("Complete farmer profile before creating products.");

        var pickupLocation = await _pickupLocationRepository.GetByIdWithCityAsync(request.PickupLocationId);
        if (pickupLocation is null)
            throw new NotFoundException("Pickup location not found.");

        if (pickupLocation.FarmerId != userId)
            throw new ForbiddenException("This pickup location does not belong to you.");

        if (!pickupLocation.IsActive)
            throw new BadRequestException("Cannot create product on an inactive pickup location.");

        ValidateProductInput(request.Name, request.PricePerUnit, request.AvailableQuantity, request.UnitType);

        var product = new Product
        {
            FarmerId = userId,
            PickupLocationId = request.PickupLocationId,
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            UnitType = request.UnitType,
            PricePerUnit = request.PricePerUnit,
            AvailableQuantity = request.AvailableQuantity,
            ImageUrl = request.ImageUrl,
            IsActive = true
        };

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        return await MapToDetailsDtoAsync(product.Id);
    }

    public async Task<ProductDetailsDto> UpdateAsync(long userId, long productId, UpdateProductDto request)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null)
            throw new NotFoundException("Product not found.");

        if (product.FarmerId != userId)
            throw new ForbiddenException("You cannot update this product.");

        var pickupLocation = await _pickupLocationRepository.GetByIdWithCityAsync(request.PickupLocationId);
        if (pickupLocation is null)
            throw new NotFoundException("Pickup location not found.");

        if (pickupLocation.FarmerId != userId)
            throw new ForbiddenException("This pickup location does not belong to you.");

        if (!pickupLocation.IsActive)
            throw new BadRequestException("Cannot move product to an inactive pickup location.");

        ValidateProductInput(request.Name, request.PricePerUnit, request.AvailableQuantity, request.UnitType);

        product.PickupLocationId = request.PickupLocationId;
        product.Name = request.Name;
        product.Description = request.Description;
        product.Category = request.Category;
        product.UnitType = request.UnitType;
        product.PricePerUnit = request.PricePerUnit;
        product.AvailableQuantity = request.AvailableQuantity;
        product.ImageUrl = request.ImageUrl;

        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync();

        return await MapToDetailsDtoAsync(product.Id);
    }

    public async Task DeactivateAsync(long userId, long productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null)
            throw new NotFoundException("Product not found.");

        if (product.FarmerId != userId)
            throw new ForbiddenException("You cannot deactivate this product.");

        product.IsActive = false;

        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync();
    }

    public async Task<List<ProductListItemDto>> GetMyProductsAsync(long userId)
    {
        var products = await _productRepository.GetByFarmerIdAsync(userId);

        return products.Select(x => new ProductListItemDto
        {
            Id = x.Id,
            Name = x.Name,
            UnitType = x.UnitType,
            PricePerUnit = x.PricePerUnit,
            AvailableQuantity = x.AvailableQuantity,
            PickupLocationId = x.PickupLocationId,
            PickupArea = x.PickupLocation.AreaName,
            CityName = x.PickupLocation.City.Name,
            FarmerName = x.Farmer?.User?.FullName ?? string.Empty,
            ImageUrl = x.ImageUrl
        }).ToList();
    }

    public async Task<ProductDetailsDto> GetByIdAsync(long productId)
    {
        return await MapToDetailsDtoAsync(productId);
    }

    public async Task<List<ProductListItemDto>> SearchAsync(ProductSearchRequestDto request)
    {
        if (request.CityId <= 0)
            throw new BadRequestException("City is required.");

        var query = _productRepository.Query()
            .Where(x => x.IsActive)
            .Where(x => x.PickupLocation.IsActive)
            .Where(x => x.PickupLocation.City.IsActive)
            .Where(x => x.PickupLocation.CityId == request.CityId);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var normalized = request.Query.Trim().ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(normalized) ||
                (x.Description != null && x.Description.ToLower().Contains(normalized)) ||
                (x.Category != null && x.Category.ToLower().Contains(normalized)));
        }

        if (!string.IsNullOrWhiteSpace(request.Area))
        {
            var area = request.Area.Trim().ToLower();
            query = query.Where(x => x.PickupLocation.AreaName.ToLower().Contains(area));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var category = request.Category.Trim().ToLower();
            query = query.Where(x => x.Category != null && x.Category.ToLower() == category);
        }

        query = request.SortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(x => x.PricePerUnit),
            "price_desc" => query.OrderByDescending(x => x.PricePerUnit),
            "name" => query.OrderBy(x => x.Name),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return products.Select(x => new ProductListItemDto
        {
            Id = x.Id,
            Name = x.Name,
            UnitType = x.UnitType,
            PricePerUnit = x.PricePerUnit,
            AvailableQuantity = x.AvailableQuantity,
            PickupLocationId = x.PickupLocationId,
            PickupArea = x.PickupLocation.AreaName,
            CityName = x.PickupLocation.City.Name,
            FarmerName = x.Farmer.User.FullName,
            ImageUrl = x.ImageUrl
        }).ToList();
    }

    private async Task<ProductDetailsDto> MapToDetailsDtoAsync(long productId)
    {
        var product = await _productRepository.GetByIdWithRelationsAsync(productId);
        if (product is null)
            throw new NotFoundException("Product not found.");

        return new ProductDetailsDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            UnitType = product.UnitType,
            PricePerUnit = product.PricePerUnit,
            AvailableQuantity = product.AvailableQuantity,
            PickupLocationId = product.PickupLocationId,
            PickupArea = product.PickupLocation.AreaName,
            CityName = product.PickupLocation.City.Name,
            PickupAddress = product.PickupLocation.AddressLine,
            Instructions = product.PickupLocation.Instructions,
            FarmerId = product.FarmerId,
            FarmerName = product.Farmer.User.FullName,
            FarmerPhone = product.Farmer.User.PhoneNumber,
            FarmerProfileImageUrl = product.Farmer.User.ProfileImageUrl,
            ImageUrl = product.ImageUrl
        };
    }

    private static void ValidateProductInput(string name, decimal price, decimal quantity, UnitType unitType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Product name is required.");

        if (price <= 0)
            throw new BadRequestException("Price must be greater than zero.");

        if (quantity <= 0)
            throw new BadRequestException("Quantity must be greater than zero.");

        var hasMoreThanTwoDecimals = decimal.Round(quantity, 2) != quantity;
        if (hasMoreThanTwoDecimals)
            throw new BadRequestException("Quantity cannot have more than 2 decimal places.");

        if (unitType is UnitType.Piece or UnitType.Dozen or UnitType.Bundle)
        {
            if (quantity != decimal.Truncate(quantity))
                throw new BadRequestException("Selected unit type requires an integer quantity.");
        }
    }
}