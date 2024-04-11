namespace Internship_system.DAL.Data.Entities;

public class PracticeDiary {
    public Guid Id { get; set; } = Guid.NewGuid();
    public Internship Internship { get; set; }
    public byte[]? File { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Дальше идет инфа для заполнения дневника(общая для всех дневников)
    /// </summary>
    public string? StudentFullName { get; set; }
    public string? CuratorFullName { get; set; }
    public string? TaskReportTable { get; set; }
    /// <summary>
    /// Поля для курсового дневника
    /// </summary>
    public string? CourseWorkName { get; set; } // Название курсача
    public string? PlanTable { get; set; } // План курсача
    
}