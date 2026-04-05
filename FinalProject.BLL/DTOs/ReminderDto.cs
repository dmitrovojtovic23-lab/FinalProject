using FinalProject.DAL;

namespace FinalProject.BLL.DTOs
{
    public class ReminderDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime ReminderTime { get; set; }
        public ReminderType Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastSentAt { get; set; }
        public int? TaskId { get; set; }
        public TaskDto? Task { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateReminderDto
    {
        public string Message { get; set; } = string.Empty;
        public DateTime ReminderTime { get; set; }
        public ReminderType Type { get; set; } = ReminderType.OneTime;
        public bool IsActive { get; set; } = true;
        public int? TaskId { get; set; }
    }

    public class UpdateReminderDto
    {
        public string? Message { get; set; }
        public DateTime? ReminderTime { get; set; }
        public ReminderType? Type { get; set; }
        public bool? IsActive { get; set; }
    }
}
