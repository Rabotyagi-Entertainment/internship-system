namespace Internship_system.DAL.Data.Entities;

public class Student : User {
    public string? Group { get; set; }
    public int? CourseNumber { get; set; }
    public List<InternshipProgress> InternshipProgresses { get; set; } = new();
    public List<Internship> Internships { get; set; } = new();
}