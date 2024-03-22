using TaskProcessingAPI.Domain.Enums;

namespace TaskProcessingAPI.Domain.Entities
{
    public class TaskEntity
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public TaskEntityState State { get; set; }
        public Guid? UserId { get; set; }
        public UserEntity User { get; set; }
        public bool IsTaskProcessingStarted { get; set; }
    }
}
