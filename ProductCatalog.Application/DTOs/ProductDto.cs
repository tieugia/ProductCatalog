using ProductCatalog.Common.Attributes;

namespace ProductCatalog.Application.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int InventoryLevel { get; set; }
        [NotEmptyGuid]
        public Guid CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
