namespace Internship_system.BLL.DTOs;

public class ProfileDto {
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string TelegramUserName { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public IList<string> Roles { get; set; } = new List<string>();
}