﻿using Bookify.Web.Core.Consts;
using Microsoft.AspNetCore.Identity;

namespace Bookify.Web.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            if(!roleManager.Roles.Any()) 
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
                await roleManager.CreateAsync(new IdentityRole(Roles.Archive));
                await roleManager.CreateAsync(new IdentityRole(Roles.Reciption));
            }
        }
    }
}
