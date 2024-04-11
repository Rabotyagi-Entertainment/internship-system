using Internship_system.DAL.Data.Entities.Enums;
using Microsoft.AspNetCore.Identity;

namespace Internship_system.DAL.Data.Entities;

public class Role : IdentityRole<Guid> {
    public RoleType RoleType { get; set; }
    public ICollection<UserRole> Users { get; set; }
}
