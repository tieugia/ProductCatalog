using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Interfaces.Repositories;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Infrastructure.Data;

namespace ProductCatalog.Infrastructure.Repositories
{
    public class CategoryHierarchyRepository : GenericRepository<CategoryHierarchy>, ICategoryHierarchyRepository
    {
        public CategoryHierarchyRepository(ProductCatalogContext context) : base(context) { }

        public async Task<IEnumerable<CategoryHierarchy>> GetChildrenAsync(Guid parentId)
        {
            var hierarchies = Include(h => h.Child);
            return await hierarchies.Where(h => h.ParentId == parentId).ToListAsync();
        }

        public async Task<IEnumerable<CategoryHierarchy>> GetParentsAsync(Guid childId)
        {
            var hierarchies = Include(h => h.Parent);
            return await hierarchies.Where(h => h.ChildId == childId).ToListAsync();
        }
    }
}
