using Microsoft.AspNetCore.Mvc;
using FinalProject.BLL.DTOs;
using FinalProject.BLL.Interfaces;
using FinalProject.DAL;

namespace FinalProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetUserCategories([FromQuery] int userId)
        {
            var categories = await _categoryService.GetUserCategoriesAsync(userId);
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id, [FromQuery] int userId)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id, userId);
            if (category == null)
                return NotFound();

            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto createCategoryDto, [FromQuery] int userId)
        {
            try
            {
                var category = await _categoryService.CreateCategoryAsync(createCategoryDto, userId);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromQuery] int userId, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                var category = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto, userId);
                return Ok(category);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategory(int id, [FromQuery] int userId)
        {
            var result = await _categoryService.DeleteCategoryAsync(id, userId);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
