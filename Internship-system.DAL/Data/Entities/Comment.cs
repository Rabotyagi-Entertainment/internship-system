
using internship_system.Common.Enums;

namespace Internship_system.DAL.Data.Entities;

public class Comment {
    public Guid Id { get; set; } = Guid.NewGuid();
    public InternshipProgress? InternshipProgress { get; set; }
    public PracticeDiary? PracticeDiary { get; set; }
    public User User { get; set; }
    public RoleType RoleType { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}