using internship_system.Common.Enums;

namespace Internship_system.BLL.DTOs.InternshipAdmin;

public class CommentDto
{
    public string Text { get; set; }
    public string Author { get; set; }
    public RoleType? RoleType { get; set; }
}