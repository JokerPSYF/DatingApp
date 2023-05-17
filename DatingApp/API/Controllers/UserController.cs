using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    public class UserController : BaseApiController
    {
        private readonly IUserService userService;
        private readonly IUserRepository repository;
        private readonly IMapper mapper;

        public UserController(IUserService userService, IUserRepository repository, IMapper mapper)
        {
            this.userService = userService;
            this.repository = repository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await repository.GetMembersAsync();

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await repository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await this.repository.GetUserByUsernameAsync(username);

            if (user == null) return NotFound();

            mapper.Map(memberUpdateDto, user);

            if (await repository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");

        }
    }
}
