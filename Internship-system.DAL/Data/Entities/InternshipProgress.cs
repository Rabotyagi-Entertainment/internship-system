using internship_system.Common.Enums;

namespace Internship_system.DAL.Data.Entities;

public class InternshipProgress {
    public Guid Id { get; set;} = Guid.NewGuid();
    public Student Student { get; set; }
    public Company Company { get; set; }
    public int? Priority { get; set; }
    public ProgressStatus ProgressStatus { get; set; }
    public string? AdditionalInfo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    public List<Comment> Comments { get; set; } = new();
}