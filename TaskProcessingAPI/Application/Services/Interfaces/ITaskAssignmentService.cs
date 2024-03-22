namespace TaskProcessingAPI.Application.Services.Interfaces
{
    public interface ITaskAssignmentService
    {
        Task<bool> ProcessTask(Guid id);
    }
}
