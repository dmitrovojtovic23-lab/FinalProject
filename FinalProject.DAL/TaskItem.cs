using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.DAL
{
    public enum TaskStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }

    public enum TaskPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Urgent = 3
    }

    public class TaskItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual TaskCategory? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
        public virtual ICollection<TaskTagItem> TaskTags { get; set; } = new List<TaskTagItem>();
    }
}
