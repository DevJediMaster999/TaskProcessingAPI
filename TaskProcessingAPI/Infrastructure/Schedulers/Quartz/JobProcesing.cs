using Microsoft.Extensions.Options;
using Quartz;
using TaskProcessingAPI.Settings;

namespace TaskProcessingAPI.Infrastructure.Schedulers.Quartz
{
    public class JobProcessing : IJobProcessing
    {
        private readonly IScheduler _scheduler;
        private readonly QuartzConfig _quartzSettings;
        private readonly ILogger<JobProcessing> _logger;

        public JobProcessing(ISchedulerFactory factory, IOptions<QuartzConfig> _options, ILogger<JobProcessing> logger)
        {
            _scheduler = factory.GetScheduler().Result;
            _quartzSettings = _options.Value;
            _logger = logger;
        }

        public async Task AddJob(Guid id)
        {
            var jobData = new JobDataMap
            {
                { "TaskId", id }
            };

            var jobName = _quartzSettings.JobName + id;

            var job = JobBuilder.Create<TaskProcessingJob>()
                .WithIdentity(jobName, _quartzSettings.JobGroup)
                .SetJobData(jobData)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{_quartzSettings.JobName + id}Trigger", _quartzSettings.JobGroup)
                .StartNow()
                .WithSimpleSchedule(x => x
                .WithIntervalInMinutes(_quartzSettings.Frequency)
                .RepeatForever())
                .Build();

            await _scheduler.ScheduleJob(job, trigger);
            _logger.LogInformation("Job {JobName} has been created for the task {TaskId}", jobName, id);
        }

        public async Task DeleteJob(string jobName, string groupName)
        {
            var jobKey = new JobKey(jobName, groupName);
            await _scheduler.DeleteJob(jobKey);
            _logger.LogInformation("Job {JobName} has been deleted", jobName);
        }
    }
}
