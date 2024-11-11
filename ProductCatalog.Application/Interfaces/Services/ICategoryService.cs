using ProductCatalog.Application.DTOs;

namespace ProductCatalog.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> GetCategoryByIdAsync(Guid id);
        Task<CategoryDto?> CreateCategoryAsync(CategoryDto categoryDto);
        Task UpdateCategoryAsync(CategoryDto categoryDto);
        Task DeleteCategoryAsync(Guid id);
        Task<CategoryWithProductsDto> GetCategoryWithProductsAsync(Guid id);
    }
}
