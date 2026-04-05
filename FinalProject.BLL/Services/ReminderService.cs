using FinalProject.BLL.DTOs;
using FinalProject.DAL;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using IReminderService = FinalProject.BLL.Interfaces.IReminderService;

namespace FinalProject.BLL.Services
{
    public class ReminderService : IReminderService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ReminderService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReminderDto>> GetUserRemindersAsync(int userId)
        {
            var reminders = await _context.Reminders
                .Where(r => r.UserId == userId)
                .Include(r => r.Task)
                .OrderBy(r => r.ReminderTime)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ReminderDto>>(reminders);
        }

        public async Task<ReminderDto?> GetReminderByIdAsync(int id, int userId)
        {
            var reminder = await _context.Reminders
                .Include(r => r.Task)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            return reminder != null ? _mapper.Map<ReminderDto>(reminder) : null;
        }

        public async Task<ReminderDto> CreateReminderAsync(CreateReminderDto createReminderDto, int userId)
        {
            var reminder = new Reminder
            {
                Message = createReminderDto.Message,
                ReminderTime = createReminderDto.ReminderTime,
                Type = createReminderDto.Type,
                IsActive = createReminderDto.IsActive,
                TaskId = createReminderDto.TaskId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync();

            return _mapper.Map<ReminderDto>(reminder);
        }

        public async Task<ReminderDto> UpdateReminderAsync(int id, UpdateReminderDto updateReminderDto, int userId)
        {
            var reminder = await _context.Reminders
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (reminder == null)
                throw new KeyNotFoundException($"Reminder with id {id} not found");

            if (updateReminderDto.Message != null) reminder.Message = updateReminderDto.Message;
            if (updateReminderDto.ReminderTime.HasValue) reminder.ReminderTime = updateReminderDto.ReminderTime.Value;
            if (updateReminderDto.Type.HasValue) reminder.Type = updateReminderDto.Type.Value;
            if (updateReminderDto.IsActive.HasValue) reminder.IsActive = updateReminderDto.IsActive.Value;

            reminder.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<ReminderDto>(reminder);
        }

        public async Task<bool> DeleteReminderAsync(int id, int userId)
        {
            var reminder = await _context.Reminders
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (reminder == null)
                return false;

            _context.Reminders.Remove(reminder);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ReminderDto>> GetPendingRemindersAsync()
        {
            var reminders = await _context.Reminders
                .Where(r => r.IsActive && 
                           r.ReminderTime <= DateTime.UtcNow && 
                           (r.LastSentAt == null || r.LastSentAt < r.ReminderTime))
                .Include(r => r.Task)
                .Include(r => r.User)
                .OrderBy(r => r.ReminderTime)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ReminderDto>>(reminders);
        }
    }
}
