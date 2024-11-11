using CsvHelper.Configuration;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Common.Mappings
{
    public class ProductMap : ClassMap<Product>
    {
        public ProductMap()
        {
            Map(m => m.Name).Name(nameof(Product.Name));
            Map(m => m.Description).Name(nameof(Product.Description));
            Map(m => m.Price).Name(nameof(Product.Price));
            Map(m => m.InventoryLevel).Name(nameof(Product.InventoryLevel));
            Map(m => m.CategoryId).Name(nameof(Product.CategoryId));
            Map(m => m.ImageUrl).Name(nameof(Product.ImageUrl));
            Map(m => m.CreatedAt).Name(nameof(Product.CreatedAt));
        }
    }
}
