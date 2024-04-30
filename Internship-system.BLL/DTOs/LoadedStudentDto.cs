namespace Internship_system.BLL.DTOs;

public class LoadedStudentDto {
    public Guid Id { get; set; }
    public required string FullName { get; set; }
    public string? Group { get; set; }
    public int? CourseNumber { get; set; }
}