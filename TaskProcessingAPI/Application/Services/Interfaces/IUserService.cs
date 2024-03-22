using TaskProcessingAPI.Domain.Entities;

namespace TaskProcessingAPI.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserEntity>> GetAllUsersAsync();
        Task<UserEntity> GetUserByIdAsync(Guid id);
        Task AddUserAsync(string name);
        Task<IEnumerable<UserEntity>> GetAllExceptAssigned(Guid id);
        Task<UserEntity> GetUserWithLeastTasksAsync();
    }
}
