using Microsoft.AspNetCore.Mvc;
using FinalProject.BLL.DTOs;
using FinalProject.BLL.Interfaces;
using FinalProject.DAL;

namespace FinalProject.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetUserTags([FromQuery] int userId)
        {
            var tags = await _tagService.GetUserTagsAsync(userId);
            return Ok(tags);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TagDto>> GetTag(int id, [FromQuery] int userId)
        {
            var tag = await _tagService.GetTagByIdAsync(id, userId);
            if (tag == null)
                return NotFound();

            return Ok(tag);
        }

        [HttpPost]
        public async Task<ActionResult<TagDto>> CreateTag([FromBody] CreateTagDto createTagDto, [FromQuery] int userId)
        {
            try
            {
                var tag = await _tagService.CreateTagAsync(createTagDto, userId);
                return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TagDto>> UpdateTag(int id, [FromQuery] int userId, [FromBody] UpdateTagDto updateTagDto)
        {
            try
            {
                var tag = await _tagService.UpdateTagAsync(id, updateTagDto, userId);
                return Ok(tag);
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
        public async Task<ActionResult> DeleteTag(int id, [FromQuery] int userId)
        {
            var result = await _tagService.DeleteTagAsync(id, userId);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
