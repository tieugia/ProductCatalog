using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Interfaces.Repositories
{
    public interface IProductRepository: IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetFilteredProductsAsync(string? name, decimal? minPrice, decimal? maxPrice, Guid? categoryId, int pageNumber, int pageSize);
    }
}
