namespace ProductCatalog.Application.DTOs
{
    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
