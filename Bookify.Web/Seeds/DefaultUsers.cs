using Bookify.Web.Core.Consts;
using Microsoft.AspNetCore.Identity;

namespace Bookify.Web.Seeds
{
    public static class DefaultUsers
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser user = new()
            {
                FirstName = "Admin",
                LastName = "One",
                UserName = "User",
                Email = "User@Bookify.com",
                EmailConfirmed = true,
            };

            var isExist = await userManager.FindByEmailAsync(user.Email);
            if (isExist is null)
            {
                await userManager.CreateAsync(user, "P@ssword123");
                await userManager.AddToRoleAsync(user, Roles.Admin);
            }

        }
    }
}
