namespace Admin.NET.Application.Job;

/// <summary>
/// 堆高车自动任务
/// TODO：直接实现
/// </summary>
[JobDetail("trigger_RepetitionAGVTask", Description = "重复AGV搬运任务", GroupName = "AGVTask", Concurrent = false)]
//[Daily(TriggerId = "trigger_RepetitionAGVTaskJob", Description = "重复AGV搬运任务")]
[PeriodMinutes(21, TriggerId = "trigger_RepetitionAGVTaskJob", StartNow = false, RunOnStart = false)]
public class AddAgvHighJoinBoundTask : IJob
{
    #region 关系注入
    private readonly IServiceProvider _serviceProvider;
    private static readonly TaskService taskService = new TaskService();
    private readonly SqlSugarRepository<EG_WMS_Storage> _Storage = App.GetService<SqlSugarRepository<EG_WMS_Storage>>();

    #endregion

    #region 构造函数
    public AddAgvHighJoinBoundTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    #endregion

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        using var serviceScope = _serviceProvider.CreateScope();

        // 根据库位编号从小到大，依次选择库位，出库
        // 根据库位编号从大到小，依次选择库位，入库

        // 查询已占用的库位，从小到大
        var dataStorage = _Storage.AsQueryable()
                                  .Where(x => x.StorageOccupy == 1 && x.RegionNum == "nearDoor")
                                  .OrderBy(x => x.StorageNum, OrderByType.Asc)
                                  .Select(x => x.StorageNum)
                                  .ToList();
        // 查询未占用的库位,从大到小排序
        var dataStorageNoOccupy = _Storage.AsQueryable()
                  .Where(x => x.StorageOccupy == 0 && x.RegionNum == "nearDoor")
                  .OrderBy(x => x.StorageNum, OrderByType.Desc)
                  .Select(x => x.StorageNum)
                  .ToList();

        for (int i = 0; i < dataStorage.Count; i++)
        {
            var positions = dataStorage[i] + "," + dataStorageNoOccupy[i];

            TaskEntity taskEntity = new TaskEntity();
            taskEntity.TaskPath = positions;
            taskEntity.ModelNo = "stackStoreV21";
            taskEntity.CreateTime = DateTime.Now;
            taskEntity.AGV = "M001";
            DHMessage item = await taskService.AddAsync(taskEntity);
            if (item.code == 1000)
            {
                await _Storage.AsUpdateable()
                        .SetColumns(it => new EG_WMS_Storage
                        {
                            StorageOccupy = 0
                        })
                        .Where(x => x.StorageNum == dataStorage[i])
                        .ExecuteCommandAsync();

                await _Storage.AsUpdateable()
                              .SetColumns(it => new EG_WMS_Storage
                              {
                                  StorageOccupy = 1
                              })
                              .Where(x => x.StorageNum == dataStorageNoOccupy[i])
                              .ExecuteCommandAsync();
            }
            else
            {
                throw Oops.Oh("任务下发失败");
            }

        }
    }
}
