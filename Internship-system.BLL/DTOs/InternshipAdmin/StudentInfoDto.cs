using internship_system.Common.Enums;

namespace Internship_system.BLL.DTOs.InternshipAdmin;

public class StudentInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Group { get; set; }
    public List<string> Companies { get; set; } = new();
}