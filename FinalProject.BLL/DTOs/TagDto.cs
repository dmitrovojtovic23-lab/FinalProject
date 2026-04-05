namespace FinalProject.BLL.DTOs
{
    public class TagDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TaskCount { get; set; }
    }

    public class CreateTagDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
    }

    public class UpdateTagDto
    {
        public string? Name { get; set; }
        public string? Color { get; set; }
    }
}
