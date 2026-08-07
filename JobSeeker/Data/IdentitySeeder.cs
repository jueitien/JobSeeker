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
    }
}
