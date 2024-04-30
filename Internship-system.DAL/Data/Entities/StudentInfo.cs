namespace Internship_system.DAL.Data.Entities;

public class StudentInfo {
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string FullName { get; set; }
    public string? Group { get; set; }
    public int? CourseNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AttachedAt { get; set; }
}