namespace Admin.NET.Application.Job;

[JobDetail("trigger_AgvInAndOutBoundTask", Description = "重复潜伏举升出入库任务", GroupName = "AGVTask", Concurrent = false)]
[PeriodSeconds(97, TriggerId = "trigger_AgvInAndOutBoundTask", StartNow = false, RunOnStart = false)]
public class AgvLatentLiftInAndOutBoundTask : IJob
{
    private static readonly TaskService taskService = new TaskService();

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        TaskEntity taskEntity = new TaskEntity();
        TaskEntity taskEntity1 = new TaskEntity();
        taskEntity.TaskPath = "57540064" + "," + "57540119";
        taskEntity.ModelNo = "liftMoveShelf1";
        taskEntity.CreateTime = DateTime.Now;
        taskEntity.AGV = "D001";
        await taskService.AddAsync(taskEntity);

        taskEntity1.TaskPath = "57540119" + "," + "57540064";
        taskEntity1.ModelNo = "liftMoveShelf1";
        taskEntity1.CreateTime = DateTime.Now;
        taskEntity1.AGV = "D001";
        await taskService.AddAsync(taskEntity1);

    }
}
