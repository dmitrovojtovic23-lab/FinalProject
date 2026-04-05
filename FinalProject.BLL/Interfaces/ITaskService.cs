using FinalProject.BLL.DTOs;
using FinalProject.DAL;
using TaskStatus = FinalProject.DAL.TaskStatus;

namespace FinalProject.BLL.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetUserTasksAsync(int userId, TaskStatus? status = null);
        Task<TaskDto?> GetTaskByIdAsync(int id, int userId);
        Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto);
        Task<TaskDto> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto, int userId);
        Task<bool> DeleteTaskAsync(int id, int userId);
        Task<TaskDto> CompleteTaskAsync(int id, int userId);
        Task<IEnumerable<TaskDto>> GetTasksByCategoryAsync(int userId, int categoryId);
        Task<IEnumerable<TaskDto>> GetTasksByTagAsync(int userId, int tagId);
        Task<IEnumerable<TaskDto>> GetOverdueTasksAsync(int userId);
    }
}
