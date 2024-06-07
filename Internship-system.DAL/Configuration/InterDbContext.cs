using Internship_system.DAL.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Internship_system.DAL.Configuration;

public class InterDbContext : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>,
    UserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>> {
    
    public override DbSet<User> Users { get; set; } 
    public override DbSet<Role> Roles { get; set; }
    public override DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<StudentInfo> StudentInfos { get; set; }
    public DbSet<StudentTelegram> StudentTelegrams { get; set; }
    public DbSet<Dean> Deans { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Internship> Internships { get; set; }
    public DbSet<InternshipProgress> InternshipProgresses { get; set; }
    public DbSet<PracticeDiary> PracticeDiaries { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Role>(o => {
            //   o.ToTable("Roles");
        });
        
        modelBuilder.Entity<UserRole>(o => {
            // o.ToTable("UserRoles");
            o.HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            o.HasOne(x => x.User)
                .WithMany(x => x.Roles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    public InterDbContext(DbContextOptions<InterDbContext> options) : base(options) {
    }
}