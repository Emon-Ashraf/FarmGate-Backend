using DTO.Product;

namespace BLL.Interfaces;

public interface IProductService
{
    Task<ProductDetailsDto> CreateAsync(long userId, CreateProductDto request);
    Task<ProductDetailsDto> UpdateAsync(long userId, long productId, UpdateProductDto request);
    Task DeactivateAsync(long userId, long productId);
    Task<List<ProductListItemDto>> GetMyProductsAsync(long userId);
    Task<ProductDetailsDto> GetByIdAsync(long productId);
    Task<List<ProductListItemDto>> SearchAsync(ProductSearchRequestDto request);
}