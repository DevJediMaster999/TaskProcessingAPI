using Microsoft.Extensions.Logging;
using Moq;
using TaskProcessingAPI.Application.Services;
using TaskProcessingAPI.Application.Services.Interfaces;
using TaskProcessingAPI.Domain.Entities;
using TaskProcessingAPI.Domain.Enums;
using TaskProcessingAPI.Domain.Exceptions;

namespace WebAppTaskProcessingTest
{
    public class TaskAssignmentServiceTests
    {

        private Mock<ITaskService> _mockTaskService = new Mock<ITaskService>();
        private Mock<IUserService> _mockUserService= new Mock<IUserService>();
        private Mock<ILogger<TaskAssignmentService>> _mockLogger = new Mock<ILogger<TaskAssignmentService>>();

        private TaskAssignmentService CreateService(Mock<ITaskService> mockTaskService, Mock<IUserService> mockUserService,
            Mock<ILogger<TaskAssignmentService>> mockLogger)
            => new TaskAssignmentService(mockTaskService.Object, mockUserService.Object, mockLogger.Object);

        private UserEntity _user = new UserEntity { Id = Guid.NewGuid(), Name = "TestUser1" };

        private List<UserEntity> _users = new List<UserEntity>
                        {
                            new UserEntity { Id = Guid.NewGuid(), Name = "TestUser2" }
                        };

        [Theory]
        [InlineData(false, true, false, TaskEntityState.Waiting, TaskEntityState.Waiting)] // The user was unavailable when creating the task. The task state is not changed to the inProgress.
        [InlineData(true, false, true, TaskEntityState.Waiting, TaskEntityState.Waiting)] // The user was assigned to the task upon creation, but there is no available user for reassignment. Task state is no changed
        [InlineData(false, false, false, TaskEntityState.Waiting, TaskEntityState.Waiting)] // The user was unavailable during task creation and there is no available user for assignment.reassignment.
        [InlineData(true, true, true, TaskEntityState.Waiting, TaskEntityState.InProgress)] // The user was assigned to the task upon creation, and the task was successfully reassigned to another user. Task state is changed to inProgress
        [InlineData(true, false, true, TaskEntityState.InProgress, TaskEntityState.InProgress)] // The user was assigned, but there is no available user for reassignment. Task state is not changed
        [InlineData(false, false, true, TaskEntityState.InProgress, TaskEntityState.InProgress)] // The user was unassigned from the task, and there is no available user to assign. Task state is not changed
        [InlineData(false, true,  true, TaskEntityState.InProgress, TaskEntityState.Completed)] // The user was unassigned from the task, but there is an available user to assign. Task state changed to completed

        public async Task ProcessTaskState_Test(bool isUserAssigned, bool isAValibleUserToAssign,bool isProcessingTaskStarted,
            TaskEntityState state, TaskEntityState expectedTaskState)
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = new TaskEntity { Id = taskId, State = state, UserId = isUserAssigned ? Guid.NewGuid() : null, IsTaskProcessingStarted = isProcessingTaskStarted };
            var taskAssignmentService = CreateService(_mockTaskService, _mockUserService, _mockLogger);

            _mockTaskService.Setup(repo => repo.GetTaskByIdAsync(taskId)).ReturnsAsync(task);
            if (isAValibleUserToAssign && !isUserAssigned)
            {
                _mockUserService.Setup(repo => repo.GetUserWithLeastTasksAsync()).ReturnsAsync(_user);
            }
            else if(isAValibleUserToAssign && isUserAssigned)
            {
                _mockUserService.Setup(repo => repo.GetAllExceptAssigned(task.UserId.Value)).ReturnsAsync(_users);
            }
            else if(!isAValibleUserToAssign && isUserAssigned)
            {
                _mockUserService.Setup(repo => repo.GetAllExceptAssigned(task.UserId.Value)).ReturnsAsync(new List<UserEntity>());
            }
           

            // Act
            var isTaskCompleted = await taskAssignmentService.ProcessTask(taskId);

            // Assert
            Assert.False(isTaskCompleted);
            Assert.Equal(expectedTaskState, task.State);
        }

        [Fact]
        public async Task ProcessTask_WithCompletedTask_ClearsUserAndUpdatesState()
        {
            // Arrange
            var taskAssignmentService = CreateService(_mockTaskService, _mockUserService, _mockLogger);
            var taskId = Guid.NewGuid();
            var task = new TaskEntity { Id = taskId, State = TaskEntityState.Completed };

            _mockTaskService.Setup(repo => repo.GetTaskByIdAsync(taskId)).ReturnsAsync(task);

            // Act
            var isTaskCompleted = await taskAssignmentService.ProcessTask(taskId);

            // Assert
            Assert.True(isTaskCompleted);
            Assert.Null(task.UserId);
            Assert.Equal(TaskEntityState.Completed, task.State);
        }

        [Fact]
        public async Task ProcessTask_NonExistingTask_ReturnsETaskNotFoundException_Test()
        {
            // Arrange
            var taskAssignmentService = CreateService(_mockTaskService, _mockUserService, _mockLogger);
            var nonExistingTaskId = Guid.NewGuid();
            _mockTaskService.Setup(repo => repo.GetTaskByIdAsync(nonExistingTaskId)).ReturnsAsync((TaskEntity)null);

            // Assert
            await Assert.ThrowsAsync<TaskNotFoundException>(() => taskAssignmentService.ProcessTask(nonExistingTaskId));
        }

    }
}