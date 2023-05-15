using API.Data;
using API.DTOs;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class UserService : IUserService
    {
        private readonly DataContext context;

        public UserService(DataContext context)
        {
            this.context = context;
        }

        public async Task<List<UserDto>> GetUsers()
        {
            List<UserDto> users = await context.Users
                                         // .Include(x => x.Photos)
                                         .Select(x => new UserDto()
                                         {
                                             Username = x.UserName,
                                             Token = "",
                                             //PhotoUrl = x.Photos.Where(y => y.IsMain).Select(z => z.Url).FirstOrDefault(),
                                             KnownAs = x.KnownAs,
                                             Gender = x.Gender
                                         })
                                         .ToListAsync();


            return users;
        }
    }
}