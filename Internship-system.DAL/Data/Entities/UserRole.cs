using Microsoft.AspNetCore.Identity;

namespace Internship_system.DAL.Data.Entities;

public class UserRole : IdentityUserRole<Guid> {
    public virtual User User { get; set; }
    public virtual Role Role { get; set; }  
}