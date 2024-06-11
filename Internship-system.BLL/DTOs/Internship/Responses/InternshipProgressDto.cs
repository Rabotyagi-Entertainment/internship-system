using Internship_system.BLL.DTOs.InternshipAdmin;
using internship_system.Common.Enums;
using Internship_system.DAL.Data.Entities;

namespace Internship_system.BLL.DTOs.Internship.Responses;

public class InternshipProgressDto {
    public Guid Id { get; set; } = Guid.NewGuid();
    public CompanyDto Company { get; set; }
    public int? Priority { get; set; }
    public ProgressStatus ProgressStatus { get; set; }
    public string? AdditionalInfo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    public List<CommentDto> Comments { get; set; } = new();
}