using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly IMapper mapper;
        private readonly UserManager<AppUser> userManager;
        //private readonly SignInManager<AppUser> signInManager;
        private readonly ITokenService tokenService;

        public AccountController(UserManager<AppUser> userManager,
                                 //SignInManager<AppUser> signInManager,
                                 ITokenService _tokenService,
                                 IMapper mapper)
        {
            this.userManager = userManager;
            //this.signInManager = signInManager;
            this.mapper = mapper;
            this.tokenService = _tokenService;
        }

        [HttpPost("register")] // POST: api/account/register
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken :(");

            AppUser user = mapper.Map<AppUser>(registerDto);

            user.UserName = registerDto.Username.ToLower();

            IdentityResult result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);

            return new UserDto
            {
                Username = user.UserName,
                Token = await  tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            AppUser user = await userManager.Users
                                            .Include(x => x.Photos)
                                            .SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

            if (user == null) return Unauthorized("Invalid username");

            bool result = await userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!result) return Unauthorized("Invalid password");

            string photoUrl = user.Photos.Any(x => x.IsMain)
                ? user.Photos.FirstOrDefault(x => x.IsMain).Url
                : "";

            UserDto userDto = new UserDto
            {
                Username = user.UserName,
                Token = await tokenService.CreateToken(user),
                PhotoUrl = photoUrl,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
            
            return userDto;
        }

        private async Task<bool> UserExists(string username)
        {
            return await userManager.Users.AnyAsync(x => x.UserName == username);
        }
    }
}
