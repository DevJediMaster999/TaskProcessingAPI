using TaskProcessingAPI.Application.Services.Interfaces;
using TaskProcessingAPI.Domain.Entities;
using TaskProcessingAPI.Domain.Enums;
using TaskProcessingAPI.Domain.Exceptions;

namespace TaskProcessingAPI.Application.Services
{
    public class TaskAssignmentService : ITaskAssignmentService
    {
        private readonly IUserService _userService;
        private readonly ITaskService _taskService;
        private readonly ILogger<TaskAssignmentService> _logger;

        public TaskAssignmentService(ITaskService taskService, IUserService userService, ILogger<TaskAssignmentService> logger)
        {
            _userService = userService;
            _taskService = taskService;
            _logger = logger;
        }

        public async Task<bool> ProcessTask(Guid taskId)
        {

            var task = await _taskService.GetTaskByIdAsync(taskId);

            if (task == null)
            {
                _logger.LogError("Task {TaskId} not found", taskId);
                throw new TaskNotFoundException();
            }
                

            _logger.LogInformation("Processing task {TaskId}", task.Id);

            return await ProcessTaskState(task);
        }

        private async Task<bool> ProcessTaskState(TaskEntity task)
        {
            switch (task.State)
            {
                case TaskEntityState.Waiting:
                    return await ProcessTaskInStateWaiting(task);

                case TaskEntityState.InProgress:
                    return await ProcessTaskInStateProgress(task);

                case TaskEntityState.Completed:
                    return await ProcessTaskInStateCompleted(task);

                default:
                    throw new NotImplementedException("Unhandled task state");
            }
        }

        private async Task<bool> ProcessTaskInStateWaiting(TaskEntity task)
        {
            var wasUserAssigned = task.UserId.HasValue;
            var isSuccessfulAssignment = await ProcessAssigningTaskToUser(task);

            if (task.IsTaskProcessingStarted && isSuccessfulAssignment)
            {
                task.State = TaskEntityState.InProgress;
                LogStateProcessing(task);
            }

            if (!wasUserAssigned && isSuccessfulAssignment)
            {
                task.IsTaskProcessingStarted = true;
            }

            await _taskService.UpdateTaskAsync(task);

            return false;
        }

        private async Task<bool> ProcessTaskInStateProgress(TaskEntity task)
        {

            var isSuccessfulAssignment = await ProcessAssigningTaskToUser(task);
            if (isSuccessfulAssignment)
            {
                task.State = TaskEntityState.Completed;
                LogStateProcessing(task);
            }

            await _taskService.UpdateTaskAsync(task);

            return false;
        }

        private async Task<bool> ProcessTaskInStateCompleted(TaskEntity task)
        {
            task.UserId = null;
            await _taskService.UpdateTaskAsync(task);
            _logger.LogInformation("Task {TaskId} is completed", task.Id);
            return true;
        }

        private async Task<bool> ProcessAssigningTaskToUser(TaskEntity task)
        {
            if (task.UserId.HasValue)
            {
                return await ReasignTaskToUser(task);
            }

            return await AssignTaskToUser(task);
        }

        private async Task<bool> ReasignTaskToUser(TaskEntity task)
        {
            var availableUsers = await _userService.GetAllExceptAssigned(task.UserId.Value);

            if (availableUsers!= null && availableUsers.Any())
            {
                var randomUser = GetRandomUserFromList(availableUsers);
                task.UserId = randomUser.Id;
                return true;
            }

            task.UserId = null;
            return false;
        }

        private async Task<bool> AssignTaskToUser(TaskEntity task)
        {
            var userWithLeastTasks = await _userService.GetUserWithLeastTasksAsync();

            if (userWithLeastTasks != null && userWithLeastTasks != null)
            {
                task.UserId = userWithLeastTasks.Id;
                _logger.LogInformation("Task {TaskId} assigned to user {UserId}.  Execution time: {ExecutionTime}.", task.Id, task.UserId.Value, DateTime.Now);
                return true;
            }

            task.UserId = null;
            return false;
        }

        private UserEntity GetRandomUserFromList(IEnumerable<UserEntity> users)
        {
            var random = new Random();
            var randomIndex = random.Next(users.Count());
            return users.ElementAt(randomIndex);
        }

        private void LogStateProcessing(TaskEntity task)
        {
            _logger.LogInformation("Task {TaskId} has been assigned to user {UserId}. Task moved to state {TaskState}. Execution time: {ExecutionTime}.", task.Id, task.UserId.Value, task.State, DateTime.Now);
        }
    }
}
