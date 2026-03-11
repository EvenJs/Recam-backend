using Microsoft.AspNetCore.Identity;
using Remp.Models.Constants;
using Remp.Models.Entities;

namespace Remp.DataAccess.Data;

public static class DataSeeder
{
  public static async Task SeedAsync(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager)
  {
    // Seed Roles
    await SeedRolesAsync(roleManager);

    // Seed default Admin account
    await SeedAdminAsync(userManager);
  }

  private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
  {
    string[] roles = { Roles.Admin, Roles.Agent };

    foreach (var role in roles)
    {
      if (!await roleManager.RoleExistsAsync(role))
      {
        await roleManager.CreateAsync(new IdentityRole(role));
      }
    }
  }

  private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
  {
    var adminEmail = "admin@remp.com";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
      var admin = new PhotographyCompany
      {
        UserName = adminEmail,
        Email = adminEmail,
        EmailConfirmed = true,
        PhotographyCompanyName = "Remp Photography",
        CreatedAt = DateTime.UtcNow
      };

      var result = await userManager.CreateAsync(admin, "Admin@123!");

      if (result.Succeeded)
      {
        await userManager.AddToRoleAsync(admin, Roles.Admin);
      }
    }
  }
}
