using ProductCatalog.Common.Attributes;

namespace ProductCatalog.Application.DTOs
{
    public class ProductFilterDto
    {
        public string? Name { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        [NotEmptyGuid]
        public Guid? CategoryId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
