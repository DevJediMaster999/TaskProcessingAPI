using Quartz;
using TaskProcessingAPI.Application.Services.Interfaces;

namespace TaskProcessingAPI.Infrastructure.Schedulers.Quartz
{
    public class TaskProcessingJob : IJob
    {
        private readonly ITaskAssignmentService _taskAssignmentService;
        private readonly IJobProcessing _jobsProcessing;
        public TaskProcessingJob(ITaskAssignmentService taskAssignmentService, IJobProcessing jobsProcessing)
        {
            _taskAssignmentService = taskAssignmentService;
            _jobsProcessing = jobsProcessing;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobDataMap = context.JobDetail.JobDataMap;
            var taskId = jobDataMap.GetGuid("TaskId");

            var isTaskCompleted = await _taskAssignmentService.ProcessTask(taskId);

            if (isTaskCompleted)
            {
                var jobKey = context.JobDetail.Key;
                var groupName = context.JobDetail.Key.Group;
                await _jobsProcessing.DeleteJob(jobKey.Name, groupName);
            }
        }
    }
}