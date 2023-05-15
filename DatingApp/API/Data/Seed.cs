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

            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

            var option = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new DateOnlyJsonConverter() }
            };

            try
            {
                var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options: option);

                foreach (var user in users)
                {
                    using var hmac = new HMACSHA512();

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
