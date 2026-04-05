using FinalProject.BLL.DTOs;
using FinalProject.DAL;

namespace FinalProject.BLL.Interfaces
{
    public interface IReminderService
    {
        Task<IEnumerable<ReminderDto>> GetUserRemindersAsync(int userId);
        Task<ReminderDto?> GetReminderByIdAsync(int id, int userId);
        Task<ReminderDto> CreateReminderAsync(CreateReminderDto createReminderDto, int userId);
        Task<ReminderDto> UpdateReminderAsync(int id, UpdateReminderDto updateReminderDto, int userId);
        Task<bool> DeleteReminderAsync(int id, int userId);
        Task<IEnumerable<ReminderDto>> GetPendingRemindersAsync();
    }
}
