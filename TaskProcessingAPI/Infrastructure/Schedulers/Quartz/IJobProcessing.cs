namespace TaskProcessingAPI.Infrastructure.Schedulers.Quartz
{
    public interface IJobProcessing
    {
        Task AddJob(Guid id);
        Task DeleteJob(string jobName, string groupName);
    }
}
