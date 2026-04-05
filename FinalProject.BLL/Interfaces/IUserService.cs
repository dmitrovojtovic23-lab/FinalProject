using FinalProject.BLL.DTOs;
using FinalProject.DAL;

namespace FinalProject.BLL.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByTelegramIdAsync(string telegramId);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
    }
}
