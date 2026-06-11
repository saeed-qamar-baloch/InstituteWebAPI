using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InstituteWebAPI.Data
{
    public static class AuthDbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager  = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager  = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var config       = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger       = scope.ServiceProvider
                                    .GetRequiredService<ILoggerFactory>()
                                    .CreateLogger("AuthDbSeeder");

            // Ensure roles exist
            string[] roles = ["Admin", "Teacher"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed a default admin user from configuration.
            // Set SeedAdmin:Email and SeedAdmin:Password via environment variables
            // (SeedAdmin__Email / SeedAdmin__Password) or appsettings.Local.json.
            // If not configured, seeding is skipped — no hardcoded credentials in source.
            var adminEmail    = config["SeedAdmin:Email"];
            var adminPassword = config["SeedAdmin:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogWarning(
                    "SeedAdmin:Email or SeedAdmin:Password is not configured. " +
                    "Default admin user was NOT created. " +
                    "Set these values before first production startup.");
                return;
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName       = adminEmail,
                    Email          = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Seeded default admin user: {Email}", adminEmail);
                }
                else
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to seed admin user: {Errors}", errors);
                }
            }
            else
            {
                // Ensure existing user is in Admin role
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                    await userManager.AddToRoleAsync(adminUser, "Admin");
            }





        }
    }
}
