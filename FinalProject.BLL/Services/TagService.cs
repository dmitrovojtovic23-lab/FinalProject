using FinalProject.BLL.DTOs;
using FinalProject.DAL;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using ITagService = FinalProject.BLL.Interfaces.ITagService;

namespace FinalProject.BLL.Services
{
    public class TagService : ITagService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public TagService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TagDto>> GetUserTagsAsync(int userId)
        {
            var tags = await _context.Tags
                .Where(t => t.UserId == userId)
                .Include(t => t.TaskTags)
                .OrderBy(t => t.Name)
                .ToListAsync();

            var tagDtos = _mapper.Map<IEnumerable<TagDto>>(tags);
            
            // Add task count
            foreach (var tag in tagDtos)
            {
                tag.TaskCount = tags.First(t => t.Id == tag.Id)?.TaskTags?.Count ?? 0;
            }

            return tagDtos;
        }

        public async Task<TagDto?> GetTagByIdAsync(int id, int userId)
        {
            var tag = await _context.Tags
                .Include(t => t.TaskTags)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (tag == null)
                return null;

            var tagDto = _mapper.Map<TagDto>(tag);
            tagDto.TaskCount = tag.TaskTags?.Count ?? 0;
            
            return tagDto;
        }

        public async Task<TagDto> CreateTagAsync(CreateTagDto createTagDto, int userId)
        {
            var tag = new TaskTag
            {
                Name = createTagDto.Name,
                Color = createTagDto.Color ?? "#e74c3c",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            var tagDto = _mapper.Map<TagDto>(tag);
            tagDto.TaskCount = 0;
            
            return tagDto;
        }

        public async Task<TagDto> UpdateTagAsync(int id, UpdateTagDto updateTagDto, int userId)
        {
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (tag == null)
                throw new KeyNotFoundException($"Tag with id {id} not found");

            if (updateTagDto.Name != null) tag.Name = updateTagDto.Name;
            if (updateTagDto.Color != null) tag.Color = updateTagDto.Color;

            await _context.SaveChangesAsync();
            
            var tagDto = _mapper.Map<TagDto>(tag);
            var taskCount = await _context.TaskTags.CountAsync(tt => tt.TagId == id);
            tagDto.TaskCount = taskCount;
            
            return tagDto;
        }

        public async Task<bool> DeleteTagAsync(int id, int userId)
        {
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (tag == null)
                return false;

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
