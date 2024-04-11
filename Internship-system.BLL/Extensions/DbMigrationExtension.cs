using Internship_system.DAL.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Internship_system.BLL.Extensions;

public static class DbMigrationExtension {
    /// <summary>
    /// Migrate database
    /// </summary>
    /// <param name="app"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task MigrateDbAsync(this WebApplication app) {
        using var serviceScope = app.Services.CreateScope();

        // Migrate database
        var context = serviceScope.ServiceProvider.GetService<InterDbContext>();
        if (context == null) {
            throw new ArgumentNullException(nameof(context));
        }

        await context.Database.MigrateAsync();
    }
}