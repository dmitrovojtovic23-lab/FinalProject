using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject.DAL
{
    public class TaskTag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Color { get; set; } = "#e74c3c";

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<TaskTagItem> TaskTags { get; set; } = new List<TaskTagItem>();
    }
}
