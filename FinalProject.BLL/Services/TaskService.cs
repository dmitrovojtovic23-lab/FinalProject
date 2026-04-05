using FinalProject.BLL.DTOs;
using FinalProject.DAL;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using TaskStatus = FinalProject.DAL.TaskStatus;
using ITaskService = FinalProject.BLL.Interfaces.ITaskService;

namespace FinalProject.BLL.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public TaskService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TaskDto>> GetUserTasksAsync(int userId, TaskStatus? status = null)
        {
            var query = _context.Tasks
                .Where(t => t.UserId == userId);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            var tasks = await query
                .Include(t => t.Category)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Reminders)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }

        public async Task<TaskDto?> GetTaskByIdAsync(int id, int userId)
        {
            var task = await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Reminders)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            return task != null ? _mapper.Map<TaskDto>(task) : null;
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto)
        {
            var task = new TaskItem
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                Priority = createTaskDto.Priority,
                DueDate = createTaskDto.DueDate,
                Status = TaskStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Add tags if provided
            if (createTaskDto.TagIds != null && createTaskDto.TagIds.Any())
            {
                var taskTags = createTaskDto.TagIds.Select(tagId => new TaskTagItem
                {
                    TaskId = task.Id,
                    TagId = tagId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.TaskTags.AddRange(taskTags);
                await _context.SaveChangesAsync();
            }

            return await GetTaskByIdAsync(task.Id, task.UserId);
        }

        public async Task<TaskDto> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto, int userId)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskTags)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
                throw new KeyNotFoundException($"Task with id {id} not found");

            if (updateTaskDto.Title != null) task.Title = updateTaskDto.Title;
            if (updateTaskDto.Description != null) task.Description = updateTaskDto.Description;
            if (updateTaskDto.Status.HasValue) 
            {
                task.Status = updateTaskDto.Status.Value;
                if (updateTaskDto.Status.Value == TaskStatus.Completed)
                    task.CompletedAt = DateTime.UtcNow;
            }
            if (updateTaskDto.Priority.HasValue) task.Priority = updateTaskDto.Priority.Value;
            if (updateTaskDto.DueDate.HasValue) task.DueDate = updateTaskDto.DueDate.Value;
            if (updateTaskDto.CategoryId.HasValue) task.CategoryId = updateTaskDto.CategoryId.Value;

            task.UpdatedAt = DateTime.UtcNow;

            // Update tags if provided
            if (updateTaskDto.TagIds != null)
            {
                // Remove existing tags
                _context.TaskTags.RemoveRange(task.TaskTags);
                
                // Add new tags
                if (updateTaskDto.TagIds.Any())
                {
                    var newTaskTags = updateTaskDto.TagIds.Select(tagId => new TaskTagItem
                    {
                        TaskId = task.Id,
                        TagId = tagId,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();
                    _context.TaskTags.AddRange(newTaskTags);
                }
            }

            await _context.SaveChangesAsync();
            return await GetTaskByIdAsync(task.Id, userId);
        }

        public async Task<bool> DeleteTaskAsync(int id, int userId)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
                return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TaskDto> CompleteTaskAsync(int id, int userId)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
                throw new KeyNotFoundException($"Task with id {id} not found");

            task.Status = TaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<TaskDto>(task);
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByCategoryAsync(int userId, int categoryId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId && t.CategoryId == categoryId)
                .Include(t => t.Category)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Reminders)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByTagAsync(int userId, int tagId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .Where(t => t.TaskTags.Any(tt => tt.TagId == tagId))
                .Include(t => t.Category)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Reminders)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }

        public async Task<IEnumerable<TaskDto>> GetOverdueTasksAsync(int userId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId && 
                           t.Status != TaskStatus.Completed && 
                           t.DueDate.HasValue && 
                           t.DueDate < DateTime.UtcNow)
                .Include(t => t.Category)
                .Include(t => t.TaskTags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Reminders)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }
    }
}
