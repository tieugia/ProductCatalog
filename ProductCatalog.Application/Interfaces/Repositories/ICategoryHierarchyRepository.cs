using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Interfaces.Repositories
{
    public interface ICategoryHierarchyRepository : IGenericRepository<CategoryHierarchy>
    {
        Task<IEnumerable<CategoryHierarchy>> GetChildrenAsync(Guid parentId);
        Task<IEnumerable<CategoryHierarchy>> GetParentsAsync(Guid childId);
    }
}
