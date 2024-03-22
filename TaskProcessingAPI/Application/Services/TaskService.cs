using Microsoft.EntityFrameworkCore;
using TaskProcessingAPI.Application.Services.Interfaces;
using TaskProcessingAPI.Domain.Entities;
using TaskProcessingAPI.Domain.Enums;
using TaskProcessingAPI.Domain.Exceptions;
using TaskProcessingAPI.Infrastructure.Persistence;
using TaskProcessingAPI.Infrastructure.Schedulers.Quartz;

namespace TaskProcessingAPI.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IJobProcessing _jobsProcessing;
        private readonly IUserService _userService;
        private readonly ILogger<TaskService> _logger;


        public TaskService(ApplicationDbContext  dbContext, IJobProcessing jobsProcessing, IUserService userService, ILogger<TaskService> logger)
        {
            _dbContext = dbContext;
            _jobsProcessing = jobsProcessing;
            _userService = userService;
            _logger = logger;
        }
        public async Task AddTaskAsync(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                _logger.LogError("Description is required");
                throw new TaskEmptyDescriptionException();
            }
            var user = await _userService.GetUserWithLeastTasksAsync();

            var task = new TaskEntity()
            {
                Id = Guid.NewGuid(),
                Description = description,
                State = TaskEntityState.Waiting,
                UserId = user?.Id,
                IsTaskProcessingStarted = user?.Id != null
            };

            await _dbContext.Tasks.AddAsync(task);
            await _dbContext.SaveChangesAsync();

            if (task.UserId.HasValue)
            {
                _logger.LogInformation("Task {TaskId} is created with assigned user {UserId}", task.Id, task.UserId.Value);
            }
            else
            {
                _logger.LogInformation("Task {TaskId} is created without assigned user", task.Id);
            }
            

            await _jobsProcessing.AddJob(task.Id);
        }

        public async Task DeleteTaskByIdAsync(Guid id)
        {
            var task = await _dbContext.Tasks.FindAsync(id) 
                ?? throw new TaskNotFoundException();

            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<TaskEntity>> GetAllTasksAsync()
            => await _dbContext.Tasks.ToListAsync();

        public async Task<TaskEntity> GetTaskByIdAsync(Guid id)
        {
            return await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new TaskNotFoundException();
        }

        public async Task UpdateTaskAsync(TaskEntity task)
        {
            if (task == null)
                throw new TaskNotFoundException();

            _dbContext.Entry(task).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
    }
}
