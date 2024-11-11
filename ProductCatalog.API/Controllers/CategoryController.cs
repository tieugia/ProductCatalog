using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Interfaces.Services;
using ProductCatalog.Common.Attributes;

namespace ProductCatalog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            return Ok(await _categoryService.GetAllCategoriesAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory([NotEmptyGuid] Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(category);
        }

        [HttpGet("{id}/products")]
        public async Task<IActionResult> GetCategoryWithProducts([NotEmptyGuid] Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _categoryService.GetCategoryWithProductsAsync(id);
            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryDto categoryDto)
        {
            var createdCategoryDto = await _categoryService.CreateCategoryAsync(categoryDto);
            if (createdCategoryDto == null)
                return BadRequest("Please provide a valid product category.");

            return CreatedAtAction(nameof(GetCategory), new { id = createdCategoryDto.Id }, createdCategoryDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory([NotEmptyGuid] Guid id, CategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != categoryDto.Id)
                return BadRequest("Category Id mismatch");

            await _categoryService.UpdateCategoryAsync(categoryDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory([NotEmptyGuid] Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _categoryService.DeleteCategoryAsync(id);
            return NoContent();
        }
    }
}
