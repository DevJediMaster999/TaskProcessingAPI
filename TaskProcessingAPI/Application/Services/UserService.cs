using Microsoft.EntityFrameworkCore;
using TaskProcessingAPI.Application.Services.Interfaces;
using TaskProcessingAPI.Domain.Entities;
using TaskProcessingAPI.Domain.Exceptions;
using TaskProcessingAPI.Infrastructure.Persistence;

namespace TaskProcessingAPI.Application.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<UserService> _logger;
        public UserService(ApplicationDbContext dbContext, ILogger<UserService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;

        }

        public async Task AddUserAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                _logger.LogError("The username is required");
                throw new UserNameEmptyException();
            }
               
            if (await IsUserExists(name))
            {
                _logger.LogError("The username {name} is already taken", name);
                throw new UserNameAlreadyExistsException(name);
            }
                

            var user = new UserEntity 
            {
                Id = Guid.NewGuid(),
                Name = name 
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("User {UserId} is created", user.Id);
        }

        public async Task<IEnumerable<UserEntity>> GetAllUsersAsync()
            => await _dbContext.Users.ToListAsync();

        public async Task<UserEntity> GetUserByIdAsync(Guid id)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id) ?? throw new  UserNotFoundException();
        }

        public async Task<IEnumerable<UserEntity>> GetAllExceptAssigned(Guid id) 
            => await _dbContext.Users.Where(u => u.Id != id).ToListAsync();

        public async Task<UserEntity> GetUserWithLeastTasksAsync()
        {
            var userWithLeastTasks = await _dbContext.Users
                .OrderBy(u => u.Tasks.Count())
                .FirstOrDefaultAsync();

            return userWithLeastTasks;
        }

        private async Task<bool> IsUserExists(string name)
            => await _dbContext.Users.FirstOrDefaultAsync(u => u.Name == name) != null;
    }
}
