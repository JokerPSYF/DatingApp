using API.Converters;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager,
                                           RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;

            string userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

            JsonSerializerOptions option = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new DateOnlyJsonConverter() }
            };

            try
            {
                List<AppUser> users = JsonSerializer.Deserialize<List<AppUser>>(userData, options: option);

                List<AppRole> roles = new List<AppRole> 
                {
                    new AppRole {Name ="Member"},
                    new AppRole {Name ="Admin"},
                    new AppRole {Name ="Moderator"}
                
                };

                foreach (var role in roles)
                {
                    await roleManager.CreateAsync(role);
                }

                foreach (AppUser user in users)
                {
                    user.UserName = user.UserName.ToLower();
                    await userManager.CreateAsync(user, "Password");
                    await userManager.AddToRoleAsync(user, "Member");
                }

                AppUser admin = new AppUser
                {
                    UserName = "admin"
                };

                await userManager.CreateAsync(admin, "Password");
                await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex?.InnerException?.Message);
                Console.WriteLine(ex?.Message);
            }
        }
    }
}
