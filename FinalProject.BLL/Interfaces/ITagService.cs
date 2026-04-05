using FinalProject.BLL.DTOs;
using FinalProject.DAL;

namespace FinalProject.BLL.Interfaces
{
    public interface ITagService
    {
        Task<IEnumerable<TagDto>> GetUserTagsAsync(int userId);
        Task<TagDto?> GetTagByIdAsync(int id, int userId);
        Task<TagDto> CreateTagAsync(CreateTagDto createTagDto, int userId);
        Task<TagDto> UpdateTagAsync(int id, UpdateTagDto updateTagDto, int userId);
        Task<bool> DeleteTagAsync(int id, int userId);
    }
}
