using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Interfaces
{
    public interface IProductCatalogContext
    {
        DbSet<Category> Categories { get; set; }
        DbSet<CategoryHierarchy> CategoryHierarchies { get; set; }
        DbSet<Product> Products { get; set; }

        int SaveChanges();
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
