using FinalProject.BLL.DTOs;
using FinalProject.DAL;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using ICategoryService = FinalProject.BLL.Interfaces.ICategoryService;

namespace FinalProject.BLL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CategoryService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetUserCategoriesAsync(int userId)
        {
            var categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .Include(c => c.Tasks)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            
            // Add task count
            foreach (var category in categoryDtos)
            {
                category.TaskCount = categories.First(c => c.Id == category.Id)?.Tasks?.Count ?? 0;
            }

            return categoryDtos;
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id, int userId)
        {
            var category = await _context.Categories
                .Include(c => c.Tasks)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
                return null;

            var categoryDto = _mapper.Map<CategoryDto>(category);
            categoryDto.TaskCount = category.Tasks?.Count ?? 0;
            
            return categoryDto;
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, int userId)
        {
            var category = new TaskCategory
            {
                Name = createCategoryDto.Name,
                Color = createCategoryDto.Color ?? "#3498db",
                Icon = createCategoryDto.Icon,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var categoryDto = _mapper.Map<CategoryDto>(category);
            categoryDto.TaskCount = 0;
            
            return categoryDto;
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto, int userId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
                throw new KeyNotFoundException($"Category with id {id} not found");

            if (updateCategoryDto.Name != null) category.Name = updateCategoryDto.Name;
            if (updateCategoryDto.Color != null) category.Color = updateCategoryDto.Color;
            if (updateCategoryDto.Icon != null) category.Icon = updateCategoryDto.Icon;

            await _context.SaveChangesAsync();
            
            var categoryDto = _mapper.Map<CategoryDto>(category);
            var taskCount = await _context.Tasks.CountAsync(t => t.CategoryId == id);
            categoryDto.TaskCount = taskCount;
            
            return categoryDto;
        }

        public async Task<bool> DeleteCategoryAsync(int id, int userId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
                return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
