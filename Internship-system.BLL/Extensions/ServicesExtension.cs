using Internship_system.BLL.Services;
using Internship_system.DAL.Data;
using Internship_system.DAL.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Internship_system.BLL.Extensions;

public static class ServicesExtension {
    public static IServiceCollection AddIdentityManagers(
        this IServiceCollection services,
        IConfiguration configuration
    ) {
        services.AddDbContext<InterDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database")));
        services.AddIdentity<User, Role>(o => {
                // configure identity options
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 5;
            })
            .AddEntityFrameworkStores<InterDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager<SignInManager<User>>()
            .AddUserManager<UserManager<User>>()
            .AddRoleManager<RoleManager<Role>>();



        services.AddScoped<AuthService>();
        services.AddScoped<PracticeDiaryService>();
        services.AddScoped<InternshipService>();
        services.AddScoped<InternshipAdminService>();

        return services;
    }
}