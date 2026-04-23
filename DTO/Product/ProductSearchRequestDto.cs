namespace DTO.Product;

public class ProductSearchRequestDto
{
    public long CityId { get; set; }
    public string? Query { get; set; }
    public string? Area { get; set; }
    public string? Category { get; set; }
    public string? SortBy { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}