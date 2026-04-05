using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.DAL
{
    public enum ReminderType
    {
        OneTime = 0,
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Yearly = 4
    }

    public class Reminder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        public DateTime ReminderTime { get; set; }

        public ReminderType Type { get; set; } = ReminderType.OneTime;

        public bool IsActive { get; set; } = true;

        public DateTime? LastSentAt { get; set; }

        public int? TaskId { get; set; }

        [ForeignKey("TaskId")]
        public virtual TaskItem? Task { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
