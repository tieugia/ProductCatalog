using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Interfaces.Repositories;
using ProductCatalog.Application.Interfaces.Services;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Services
{
    public class CategoryHierarchyService : ICategoryHierarchyService
    {
        private readonly ICategoryHierarchyRepository _categoryHierarchyRepository;

        public CategoryHierarchyService(ICategoryHierarchyRepository categoryHierarchyRepository)
        {
            _categoryHierarchyRepository = categoryHierarchyRepository;
        }

        public async Task<CategoryHierarchyDto> GetByIdAsync(Guid id)
        {
            var createdCategoryHierarchy = await _categoryHierarchyRepository.GetByIdAsync(id);

            if (createdCategoryHierarchy == null)
                throw new KeyNotFoundException("Category hierarchy not found");

            return new CategoryHierarchyDto
            {
                Id = createdCategoryHierarchy.Id,
                ParentId = createdCategoryHierarchy.ParentId,
                ChildId = createdCategoryHierarchy.ChildId,
                RowVersion = createdCategoryHierarchy.RowVersion
            };
        }
        
        public async Task<CategoryHierarchyDto> AddCategoryHierarchyAsync(CategoryHierarchyDto dto)
        {
            var categoryHierarchy = new CategoryHierarchy
            {
                ParentId = dto.ParentId,
                ChildId = dto.ChildId
            };

            var createdCategoryHierarchy = await _categoryHierarchyRepository.AddAsync(categoryHierarchy);

            return new CategoryHierarchyDto
            {
                Id = createdCategoryHierarchy.Id,
                ParentId = createdCategoryHierarchy.ParentId,
                ChildId = createdCategoryHierarchy.ChildId,
                RowVersion = createdCategoryHierarchy.RowVersion
            };
        }
        
        public async Task UpdateCategoryHierarchyAsync(Guid id, CategoryHierarchyDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (id != dto.Id) throw new ArgumentException("Id mismatch");

            var existingCategoryHierarchy = await _categoryHierarchyRepository.GetByIdAsync(id);

            if (existingCategoryHierarchy == null)
                throw new KeyNotFoundException("Category hierarchy not found");

            existingCategoryHierarchy.ParentId = dto.ParentId;
            existingCategoryHierarchy.ChildId = dto.ChildId;

            existingCategoryHierarchy.RowVersion = dto.RowVersion;

            await _categoryHierarchyRepository.UpdateAsync(existingCategoryHierarchy);
        }

        public async Task RemoveCategoryHierarchyAsync(Guid id)
        {
            var categoryHierarchy = await _categoryHierarchyRepository.GetByIdAsync(id);
            if (categoryHierarchy == null)
                throw new KeyNotFoundException("Category hierarchy not found");

            await _categoryHierarchyRepository.DeleteAsync(categoryHierarchy);
        }

        public async Task<IEnumerable<CategoryDto>> GetChildrenAsync(Guid parentId)
        {
            var hierarchies = await _categoryHierarchyRepository.GetChildrenAsync(parentId);
            return hierarchies.Select(h => h.Child).Select(c => new CategoryDto
            {
                Id = c.Id,
                Description = c.Description,
                Name = c.Name,
                RowVersion = c.RowVersion
            });
        }

        public async Task<IEnumerable<CategoryDto>> GetParentsAsync(Guid childId)
        {
            var hierarchies = await _categoryHierarchyRepository.GetParentsAsync(childId);
            return hierarchies.Select(h => h.Parent).Select(c => new CategoryDto
            {
                Id = c.Id,
                Description = c.Description,
                Name = c.Name,
                RowVersion = c.RowVersion
            });
        }
    }
}
