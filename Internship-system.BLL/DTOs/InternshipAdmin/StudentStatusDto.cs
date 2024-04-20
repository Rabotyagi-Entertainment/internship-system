using Internship_system.DAL.Data.Entities.Enums;

namespace Internship_system.BLL.DTOs.InternshipAdmin;

public class StudentStatusDto
{
    public Guid StudentId { get; set; }
    public string CompanyName { get; set; }
    public ProgressStatus Status { get; set; }
    public Guid InternshipProgressId { get; set; }
    public List<CommentDto> Comments { get; set; }
}