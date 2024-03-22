using TaskProcessingAPI.Domain.Entities;

namespace TaskProcessingAPI.Application.Services.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskEntity>> GetAllTasksAsync();
        Task<TaskEntity> GetTaskByIdAsync(Guid id);
        Task AddTaskAsync(string description);
        Task UpdateTaskAsync(TaskEntity task);
        Task DeleteTaskByIdAsync(Guid id);
    }
}
