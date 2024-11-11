using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Interfaces.Repositories;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Infrastructure.Data;
using System.Data;

namespace ProductCatalog.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ProductCatalogContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetFilteredProductsAsync(
            string? name,
            decimal? minPrice,
            decimal? maxPrice,
            Guid? categoryId,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Products.AsNoTracking();

            // Apply filters
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Name.Contains(name));
            }
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Apply pagination
            query = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return await query.ToListAsync();
        }

        public async Task ImportProductsAsync(DataTable dataTable)
        {
            using var connection = _context.Database.GetDbConnection();

            await connection.OpenAsync();

            using var bulkCopy = new SqlBulkCopy((SqlConnection)connection);

            bulkCopy.DestinationTableName = "Products";
            bulkCopy.BatchSize = 1000;

            bulkCopy.ColumnMappings.Add(nameof(Product.Name), nameof(Product.Name));
            bulkCopy.ColumnMappings.Add(nameof(Product.Description), nameof(Product.Description));
            bulkCopy.ColumnMappings.Add(nameof(Product.Price), nameof(Product.Price));
            bulkCopy.ColumnMappings.Add(nameof(Product.InventoryLevel), nameof(Product.InventoryLevel));
            bulkCopy.ColumnMappings.Add(nameof(Product.CategoryId), nameof(Product.CategoryId));
            bulkCopy.ColumnMappings.Add(nameof(Product.ImageUrl), nameof(Product.ImageUrl));
            bulkCopy.ColumnMappings.Add(nameof(Product.CreatedAt), nameof(Product.CreatedAt));

            await bulkCopy.WriteToServerAsync(dataTable);
        }
    }
}
