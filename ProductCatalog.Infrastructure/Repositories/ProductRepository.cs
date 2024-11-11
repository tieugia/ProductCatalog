using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Interfaces.Repositories;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Infrastructure.Data;
using System.Data;
using System.Data.Common;

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

        public async Task ImportUpsertProductsAsync(DataTable dataTable)
        {
            using var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            // Create a temporary table
            await CreateTemporaryTableAsync(connection);

            // Insert data into the temporary table
            await BulkInsertToTemporaryTableAsync(connection, dataTable);

            // Merge data from the temporary table into the Products table
            await MergeTemporaryTableIntoProductsAsync(connection);

            // Drop the temporary table to clean up
            await DropTemporaryTableAsync(connection);
        }

        private async Task CreateTemporaryTableAsync(DbConnection connection)
        {
            var createTempTableSql = @"
            CREATE TABLE #TempProducts (
                Id UNIQUEIDENTIFIER,
                Name NVARCHAR(255),
                Description NVARCHAR(1000),
                Price DECIMAL(18, 2),
                InventoryLevel INT,
                CategoryId UNIQUEIDENTIFIER,
                ImageUrl NVARCHAR(1000),
                CreatedAt DATETIME,
                UpdatedAt DATETIME
            )";

            using var command = connection.CreateCommand();
            command.CommandText = createTempTableSql;
            await command.ExecuteNonQueryAsync();
        }

        private async Task BulkInsertToTemporaryTableAsync(DbConnection connection, DataTable productUpdates)
        {
            using var bulkCopy = new SqlBulkCopy((SqlConnection)connection)
            {
                DestinationTableName = "#TempProducts",
                BatchSize = 1000
            };

            // Map columns
            bulkCopy.ColumnMappings.Add(nameof(Product.Id), nameof(Product.Id));
            bulkCopy.ColumnMappings.Add(nameof(Product.Name), nameof(Product.Name));
            bulkCopy.ColumnMappings.Add(nameof(Product.Description), nameof(Product.Description));
            bulkCopy.ColumnMappings.Add(nameof(Product.Price), nameof(Product.Price));
            bulkCopy.ColumnMappings.Add(nameof(Product.InventoryLevel), nameof(Product.InventoryLevel));
            bulkCopy.ColumnMappings.Add(nameof(Product.CategoryId), nameof(Product.CategoryId));
            bulkCopy.ColumnMappings.Add(nameof(Product.ImageUrl), nameof(Product.ImageUrl));
            bulkCopy.ColumnMappings.Add(nameof(Product.CreatedAt), nameof(Product.CreatedAt));
            bulkCopy.ColumnMappings.Add(nameof(Product.UpdatedAt), nameof(Product.UpdatedAt));

            await bulkCopy.WriteToServerAsync(productUpdates);
        }

        private async Task MergeTemporaryTableIntoProductsAsync(DbConnection connection)
        {
            var mergeSql = @"
            MERGE INTO Products AS target
            USING #TempProducts AS source
            ON target.Id = source.Id
            WHEN MATCHED THEN 
                UPDATE SET 
                    target.Name = source.Name,
                    target.Description = source.Description,
                    target.Price = source.Price,
                    target.InventoryLevel = source.InventoryLevel,
                    target.CategoryId = source.CategoryId,
                    target.ImageUrl = source.ImageUrl,
                    target.UpdatedAt = source.UpdatedAt
            WHEN NOT MATCHED BY TARGET THEN 
                INSERT (Id, Name, Description, Price, InventoryLevel, CategoryId, ImageUrl, CreatedAt, UpdatedAt)
                VALUES (source.Id, source.Name, source.Description, source.Price, source.InventoryLevel, source.CategoryId, source.ImageUrl, source.CreatedAt, source.UpdatedAt);";

            using var command = connection.CreateCommand();
            command.CommandText = mergeSql;
            await command.ExecuteNonQueryAsync();
        }

        private async Task DropTemporaryTableAsync(DbConnection connection)
        {
            var dropTempTableSql = "DROP TABLE #TempProducts;";
            using var command = connection.CreateCommand();
            command.CommandText = dropTempTableSql;
            await command.ExecuteNonQueryAsync();
        }
    }
}
