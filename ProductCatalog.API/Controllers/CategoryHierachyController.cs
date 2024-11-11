using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Interfaces.Services;
using ProductCatalog.Common.Attributes;

namespace ProductCatalog.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryHierarchyController : ControllerBase
    {
        private readonly ICategoryHierarchyService _categoryHierarchyService;

        public CategoryHierarchyController(ICategoryHierarchyService categoryHierarchyService)
        {
            _categoryHierarchyService = categoryHierarchyService;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHierarchy([NotEmptyGuid] Guid id, [FromBody] CategoryHierarchyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _categoryHierarchyService.UpdateCategoryHierarchyAsync(id, dto);
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> AddHierarchy([FromBody] CategoryHierarchyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdCategoryHierarchyDto = await _categoryHierarchyService.AddCategoryHierarchyAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdCategoryHierarchyDto.Id }, createdCategoryHierarchyDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveHierarchy([NotEmptyGuid] Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _categoryHierarchyService.RemoveCategoryHierarchyAsync(id);
            return NoContent();
        }

        [HttpGet("parent/{parentId}/children")]
        public async Task<IActionResult> GetChildren([NotEmptyGuid] Guid parentId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var children = await _categoryHierarchyService.GetChildrenAsync(parentId);
            return Ok(children);
        }

        [HttpGet("child/{childId}/parents")]
        public async Task<IActionResult> GetParents([NotEmptyGuid] Guid childId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parents = await _categoryHierarchyService.GetParentsAsync(childId);
            return Ok(parents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([NotEmptyGuid] Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parents = await _categoryHierarchyService.GetByIdAsync(id);
            if (parents == null)
                return NotFound();

            return Ok(parents);
        }
    }
}
