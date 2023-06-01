using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRespository : ILikesRepository
    {
        private readonly DataContext context;

        public LikesRespository(DataContext context)
        {
            this.context = context;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
            return await context.Likes.FindAsync(sourceUserId, targetUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            IQueryable<AppUser> users = context.Users.OrderBy(u => u.UserName).AsQueryable();
            IQueryable<UserLike> likes = context.Likes.AsQueryable();

            switch (likesParams.Predicate)
            {
                case "liked":
                    {
                        likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                        users = likes.Select(like => like.TargetUser);
                        break;
                    }

                case "likedBy":
                    {
                        likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
                        users = likes.Select(like => like.SourceUser);
                        break;
                    }
            }

            IQueryable<LikeDto> likedUsers = users.Select(user => new LikeDto
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                City = user.City,
                Id = user.Id
            });

            return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await context.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}
