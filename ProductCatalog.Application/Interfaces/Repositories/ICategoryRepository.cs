using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Interfaces.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category?> GetCategoryWithProductsAsync(Guid id);
    }
}
