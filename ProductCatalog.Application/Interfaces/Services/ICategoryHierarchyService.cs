using ProductCatalog.Application.DTOs;

namespace ProductCatalog.Application.Interfaces.Services
{
    public interface ICategoryHierarchyService
    {
        Task<CategoryHierarchyDto> GetByIdAsync(Guid id);
        Task<CategoryHierarchyDto> AddCategoryHierarchyAsync(CategoryHierarchyDto dto);
        Task UpdateCategoryHierarchyAsync(Guid id, CategoryHierarchyDto dto);
        Task RemoveCategoryHierarchyAsync(Guid id);
        Task<IEnumerable<CategoryDto>> GetChildrenAsync(Guid parentId);
        Task<IEnumerable<CategoryDto>> GetParentsAsync(Guid childId);
    }
}
