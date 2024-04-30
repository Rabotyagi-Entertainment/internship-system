using Microsoft.AspNetCore.Identity;

namespace Internship_system.DAL.Data.Entities;

public class User : IdentityUser<Guid> {
    /// <summary>
    /// User`s full name (surname, name, patronymic)
    /// </summary>
    public required string FullName { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public ICollection<UserRole> Roles { get; set; }
    public List<Comment> Comments { get; set; } = new();
}