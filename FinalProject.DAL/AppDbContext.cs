using Microsoft.EntityFrameworkCore;

namespace FinalProject.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<TaskCategory> Categories { get; set; }
        public DbSet<TaskTag> Tags { get; set; }
        public DbSet<TaskTagItem> TaskTags { get; set; }
        public DbSet<Reminder> Reminders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // AppUser configuration
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasIndex(e => e.TelegramId).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.FirstName).IsRequired();
                entity.Property(e => e.TelegramId).IsRequired();
            });

            // TaskItem configuration
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Tasks)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Tasks)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // TaskCategory configuration
            modelBuilder.Entity<TaskCategory>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Categories)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TaskTag configuration
            modelBuilder.Entity<TaskTag>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Tags)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TaskTagItem many-to-many relationship
            modelBuilder.Entity<TaskTagItem>(entity =>
            {
                entity.HasOne(e => e.Task)
                    .WithMany(t => t.TaskTags)
                    .HasForeignKey(e => e.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Tag)
                    .WithMany(t => t.TaskTags)
                    .HasForeignKey(e => e.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.TaskId, e.TagId }).IsUnique();
            });

            // Reminder configuration
            modelBuilder.Entity<Reminder>(entity =>
            {
                entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Reminders)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Task)
                    .WithMany(t => t.Reminders)
                    .HasForeignKey(e => e.TaskId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // You can add seed data here if needed
        }
    }
}
