using API.DTOs;

namespace API.Interfaces
{
    public interface IUserService
    {
        public Task<List<UserDto>> GetUsers();
    }
}
