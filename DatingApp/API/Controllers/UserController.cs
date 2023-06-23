using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UserController : BaseApiController
    {
        private readonly IUnitOfWork uow;
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;

        public UserController(IUnitOfWork uow,
                              IMapper mapper,
                              IPhotoService photoService)
        {
            this.uow = uow;
            this.mapper = mapper;
            this.photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            string gender = await uow.UserRepository.GetUserGender(User.GetUsername());
            userParams.CurrentUsername = User.GetUsername();


            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = gender == "male" ? "female" : "male";
            }

            PagedList<MemberDto> users = await uow.UserRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }

         [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await uow.UserRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            AppUser user = await this.uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return NotFound();

            mapper.Map(memberUpdateDto, user);

            if (await uow.Complete()) return NoContent();

            return BadRequest("Failed to update user");

        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            AppUser user = await uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return NotFound();

            ImageUploadResult result = await photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            Photo photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
            };

            if (user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);

            if (await uow.Complete())
            {
                return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            AppUser user = await uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return NotFound();

            Photo photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("This is already your main photo");

            Photo currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if (currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;

            if (await uow.Complete()) return NoContent();

            return BadRequest("Problem setting the main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            AppUser user = await uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return NotFound();

            Photo photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("You cannot delete your main photo");

            if (photo.PublicId != null)
            {
                DeletionResult result = await photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await uow.Complete()) return Ok();

            return BadRequest("Problem deleting photo");
        }
    }
}
