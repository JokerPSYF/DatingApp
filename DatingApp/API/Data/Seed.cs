using API.Converters;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(DataContext context)
        {
            if (await context.Users.AnyAsync()) return;

            string userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

            JsonSerializerOptions option = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new DateOnlyJsonConverter() }
            };

            try
            {
                List<AppUser> users = JsonSerializer.Deserialize<List<AppUser>>(userData, options: option);

                foreach (AppUser user in users)
                {
                    using HMACSHA512 hmac = new HMACSHA512();

                    user.UserName = user.UserName.ToLower();
                    user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Password"));
                    user.PasswordSalt = hmac.Key;

                    context.Users.Add(user);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex?.InnerException?.Message);
                Console.WriteLine(ex?.Message);
            }




            await context.SaveChangesAsync();
        }
    }
}
