namespace ProductCatalog.Application.DTOs
{
    public class CategoryWithProductsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public IEnumerable<ProductDto> Products { get; set; } = null!;
    }
}
