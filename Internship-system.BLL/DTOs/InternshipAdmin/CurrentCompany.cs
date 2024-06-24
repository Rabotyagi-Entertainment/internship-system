namespace Internship_system.BLL.DTOs.InternshipAdmin;

public record CurrentCompany(Guid Id, string Name, DateTime StartAt, DateTime? EndAt);