using internship_system.Common.Enums;
using Internship_system.DAL.Configuration;
using Internship_system.DAL.Data;
using Internship_system.DAL.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Internship_system.BLL.Extensions;

public static class ConfigureIdentityRolesExt {
     /// <summary>
    /// Create default roles and administrator user
    /// </summary>
    /// <param name="app"></param>
    public static async Task ConfigureIdentity(this WebApplication app) {
        using var serviceScope = app.Services.CreateScope();

        // Migrate database
        var context = serviceScope.ServiceProvider.GetService<InterDbContext>();
        
        // Get services
        var userManager = serviceScope.ServiceProvider.GetService<UserManager<User>>();
        if (userManager == null) {
            throw new ArgumentNullException(nameof(userManager));
        }

        var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<Role>>();
        if (roleManager == null) {
            throw new ArgumentNullException(nameof(roleManager));
        }

        // Try to create Roles
        foreach (var roleName in Enum.GetValues(typeof(RoleType))) {
            var strRoleName = roleName.ToString();
            if (strRoleName == null) {
                throw new ArgumentNullException(nameof(roleName), "Some role name is null");
            }

            var role = await roleManager.FindByNameAsync(strRoleName);
            if (role == null) {
                var roleResult =
                    await roleManager.CreateAsync(new Role {
                        Name = strRoleName,
                        RoleType = (RoleType)Enum.Parse(typeof(RoleType), strRoleName),
                    });
                if (!roleResult.Succeeded) {
                    throw new InvalidOperationException($"Unable to create {strRoleName} role.");
                }

                role = await roleManager.FindByNameAsync(strRoleName);
            }

            if (role == null || role.Name == null) {
                throw new ArgumentNullException(nameof(role), "Can't find role");
            }
        }

        // Try to create Administrator user
        var deanUser = await userManager.FindByEmailAsync("ketova@mail.ru");
        if (deanUser == null) {
            var user = new Dean() {
                FullName = "Кетова Татьяна Семеновна",
                UserName = "ketova",
                Email = "ketova@mail.ru",
                JoinedAt = DateTime.Now.ToUniversalTime(),
            };
            
            
            var userResult = await userManager.CreateAsync(user, "qwerty123");
            if (!userResult.Succeeded) {
                throw new InvalidOperationException($"Unable to create administrator user");
            }
        }
        deanUser = await userManager.FindByEmailAsync("ketova@mail.ru");
        
        if (deanUser == null) {
            throw new ArgumentNullException(nameof(deanUser), "Can't find admin user");
        }
        
        if (!await userManager.IsInRoleAsync(deanUser, ApplicationRoleNames.Dean)) {
            await userManager.AddToRoleAsync(deanUser, ApplicationRoleNames.Dean);
        }
    }
}