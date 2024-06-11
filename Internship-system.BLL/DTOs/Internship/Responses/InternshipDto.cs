using Internship_system.BLL.DTOs.PracticeDiary;

namespace Internship_system.BLL.DTOs.Internship.Responses;

public class InternshipDto {
    public Guid Id { get; set; } = Guid.NewGuid();
    public CompanyDto Company { get; set; }
    public List<PracticeDiaryDto> PracticeDiaries { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}