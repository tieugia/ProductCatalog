using ProductCatalog.Domain.Entities;
using System.Data;

namespace ProductCatalog.Common.Helpers
{
    public static class DataTableHelper
    {
        public static DataTable ToDataTable(IEnumerable<Product> products)
        {
            var table = new DataTable();
            table.Columns.Add(nameof(Product.Id), typeof(Guid));
            table.Columns.Add(nameof(Product.Name), typeof(string));
            table.Columns.Add(nameof(Product.Description), typeof(string));
            table.Columns.Add(nameof(Product.Price), typeof(decimal));
            table.Columns.Add(nameof(Product.InventoryLevel), typeof(int));
            table.Columns.Add(nameof(Product.CategoryId), typeof(Guid));
            table.Columns.Add(nameof(Product.ImageUrl), typeof(string));
            table.Columns.Add(nameof(Product.CreatedAt), typeof(DateTime));
            table.Columns.Add(nameof(Product.UpdatedAt), typeof(DateTime));

            foreach (var product in products)
            {
                table.Rows.Add(
                    product.Id,
                    product.Name,
                    product.Description,
                    product.Price,
                    product.InventoryLevel,
                    product.CategoryId,
                    product.ImageUrl,
                    product.CreatedAt,
                    product.UpdatedAt
                );
            }

            return table;
        }
    }
}
