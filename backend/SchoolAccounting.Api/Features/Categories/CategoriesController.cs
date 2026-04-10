using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolAccounting.Api.Common;

namespace SchoolAccounting.Api.Features.Categories;

[ApiController]
[Route("api/v1/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CategoryResponse>>> GetCategories(
        [FromQuery] string? type = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _categoryService.GetCategoriesAsync(type, isActive);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryResponse>> GetCategory(int id)
    {
        var category = await _categoryService.GetCategoryAsync(id);
        return Ok(category);
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Accountant}")]
    public async Task<ActionResult<CategoryResponse>> CreateCategory(CreateCategoryRequest request)
    {
        var category = await _categoryService.CreateCategoryAsync(request);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Accountant}")]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(int id, UpdateCategoryRequest request)
    {
        var category = await _categoryService.UpdateCategoryAsync(id, request);
        return Ok(category);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{AppRole.Admin},{AppRole.Accountant}")]
    public async Task<ActionResult> DeleteCategory(int id)
    {
        await _categoryService.DeleteCategoryAsync(id);
        return NoContent();
    }
}
