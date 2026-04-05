using FinalProject.BLL.DTOs;
using FinalProject.DAL;

namespace FinalProject.BLL.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetUserCategoriesAsync(int userId);
        Task<CategoryDto?> GetCategoryByIdAsync(int id, int userId);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, int userId);
        Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto, int userId);
        Task<bool> DeleteCategoryAsync(int id, int userId);
    }
}
