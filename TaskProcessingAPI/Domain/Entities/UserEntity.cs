namespace TaskProcessingAPI.Domain.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<TaskEntity> Tasks { get; set; }
    }
}
