using JobSeeker.Models;
using Microsoft.AspNetCore.Identity;

namespace JobSeeker.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(
            IServiceProvider services)
        {
            var roleManager =
                services.GetRequiredService<
                    RoleManager<IdentityRole>>();

            foreach (var roleName in UserRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(roleName));
                }
            }
        }

        public static async Task SeedAdminUserAsync(
            IServiceProvider services)
        {
            var userManager =
                services.GetRequiredService<
                    UserManager<ApplicationUser>>();

            const string adminEmail    = "admin@jobseeker.com";
            const string adminPassword = "Admin@12345";

            // Skip if admin already exists
            if (await userManager.FindByEmailAsync(adminEmail) != null)
                return;

            var admin = new ApplicationUser
            {
                FullName        = "System Administrator",
                UserName        = adminEmail,
                Email           = adminEmail,
                EmailConfirmed  = true,
                AccountStatus   = "ACTIVE",
                CreatedAt       = DateTime.UtcNow,
                UpdatedAt       = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(
                    admin, UserRoles.Administrator);
            }
            else
            {
                var errors = string.Join(", ",
                    result.Errors.Select(e => e.Description));

                throw new Exception(
                    $"Failed to seed admin user: {errors}");
            }
        }
    }
}
