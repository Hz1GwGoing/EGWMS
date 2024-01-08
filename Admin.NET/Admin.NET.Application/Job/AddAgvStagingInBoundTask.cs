namespace Admin.NET.Application.Job;

/// <summary>
/// 再次请求AGV入库暂存任务
/// </summary>
[JobDetail("trigger_AgvInBoundTaskJob", Description = "再次请求AGV入库暂存任务", GroupName = "AGVTask", Concurrent = false)]
//[Daily(TriggerId = "trigger_repeatAgvInBoundTaskJob", Description = "再次请求AGV入库暂存任务")]
[PeriodMinutes(1, TriggerId = "trigger_AgvInBoundTaskJob", StartNow = true, RunOnStart = false)]
public class AddAgvStagingInBoundTask : IJob
{
    #region 关系注入
    private readonly IServiceProvider _serviceProvider;
    private static readonly TaskService taskService = new TaskService();
    private readonly BaseService baseService = new BaseService();
    private readonly EG_WMS_InAndOutBoundMessage inoutboundMessage = new EG_WMS_InAndOutBoundMessage();
    private readonly SqlSugarRepository<Entity.EG_WMS_InAndOutBound> _InAndOutBound = App.GetService<SqlSugarRepository<Entity.EG_WMS_InAndOutBound>>();
    private readonly SqlSugarRepository<Entity.EG_WMS_InAndOutBoundDetail> _InAndOutBoundDetail = App.GetService<SqlSugarRepository<Entity.EG_WMS_InAndOutBoundDetail>>();
    private readonly SqlSugarRepository<EG_WMS_Tem_Inventory> _TemInventory = App.GetService<SqlSugarRepository<EG_WMS_Tem_Inventory>>();
    private readonly SqlSugarRepository<EG_WMS_Tem_InventoryDetail> _TemInventoryDetail = App.GetService<SqlSugarRepository<EG_WMS_Tem_InventoryDetail>>();
    private readonly SqlSugarRepository<TaskStagingEntity> _TaskStagingEntity = App.GetService<SqlSugarRepository<TaskStagingEntity>>();
    private readonly SqlSugarRepository<EG_WMS_WorkBin> _WorkBin = App.GetService<SqlSugarRepository<EG_WMS_WorkBin>>();

    #endregion

    #region 构造函数
    public AddAgvStagingInBoundTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    #endregion

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        using var serviceScope = _serviceProvider.CreateScope();

        // 检索暂存任务表里面有没有没有下发未完成的任务
        var taskstaging = _TaskStagingEntity.GetFirst(x => x.StagingStatus == 0 && x.InAndOutBoundNum.Contains("EGRK"));

        if (taskstaging == null)
        {
            throw Oops.Oh("当前没有暂存的入库任务！");
        }

        // 通过检索出来的任务得到入库编号
        // 得到临时库存表里面这次入库的物料编号
        // 因为一次入库里面的物料只能是一种（前提）
        var teminvData = _TemInventory.AsQueryable()
                      .Where(x => x.InBoundNum == taskstaging.InAndOutBoundNum)
                      .ToList();

        // 重新获取库位编号
        string endStorage = baseService.AGVStrategyReturnRecommEndStorage(teminvData[0].MaterielNum);

        if (endStorage == "没有合适的库位")
        {
            throw Oops.Oh("当前依然没有合适的库位！");
        }

        // 重新下发AGV任务

        TaskEntity taskEntity = taskstaging.Adapt<TaskEntity>();
        taskEntity.TaskPath = taskstaging.TaskPath + endStorage;

        DHMessage item = await taskService.AddAsync(taskEntity);
        if (item.code == 1000)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    // 修改库位状态
                    await inoutboundMessage.ModifyInventoryLocationOccupancy(taskstaging.InAndOutBoundNum, endStorage);

                    // 根据库位编号得到区域、仓库编号
                    var _storageRegionNum = inoutboundMessage.GetStorageWhereRegion(endStorage);
                    var _regionWHNum = inoutboundMessage.GetRegionWhereWHNum(_storageRegionNum);

                    // 修改入库单状态
                    await _InAndOutBound.AsUpdateable()
                                   .SetColumns(it => new EG_WMS_InAndOutBound
                                   {
                                       EndPoint = endStorage,
                                       // 现在任务下发才是入库中的状态
                                       InAndOutBoundStatus = 4,
                                       UpdateTime = DateTime.Now,
                                   })
                                   .Where(x => x.InAndOutBoundNum == taskstaging.InAndOutBoundNum)
                                   .ExecuteCommandAsync();

                    await _InAndOutBoundDetail.AsUpdateable()
                                              .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                                              {
                                                  StorageNum = endStorage,
                                                  WHNum = _regionWHNum,
                                                  RegionNum = _storageRegionNum,
                                                  UpdateTime = DateTime.Now,
                                              })
                                              .Where(x => x.InAndOutBoundNum == taskstaging.InAndOutBoundNum)
                                              .ExecuteCommandAsync();

                    await _WorkBin.AsUpdateable()
                          .SetColumns(it => new EG_WMS_WorkBin
                          {
                              StorageNum = endStorage,
                              UpdateTime = DateTime.Now,
                          })
                          .Where(x => x.InAndOutBoundNum == taskstaging.InAndOutBoundNum)
                          .ExecuteCommandAsync();

                    // 修改暂存任务

                    await _TaskStagingEntity.AsUpdateable()
                                       .SetColumns(it => new TaskStagingEntity
                                       {
                                           StagingStatus = 1,
                                           UpdateTime = DateTime.Now,
                                       })
                                       .Where(x => x.InAndOutBoundNum == taskstaging.InAndOutBoundNum)
                                       .ExecuteCommandAsync();


                    // 查询临时库存主表里面的库存编号

                    var datas = _TemInventory.AsQueryable()
                                   .Where(x => x.InBoundNum == taskstaging.InAndOutBoundNum)
                                   .Select(x => new
                                   {
                                       x.InventoryNum
                                   })
                                   .ToList();

                    for (int i = 0; i < datas.Count; i++)
                    {
                        // 修改临时库存明细表里面的库位信息

                        await _TemInventoryDetail.AsUpdateable()
                                                 .SetColumns(it => new EG_WMS_Tem_InventoryDetail
                                                 {
                                                     StorageNum = endStorage,
                                                     WHNum = _regionWHNum,
                                                     RegionNum = _storageRegionNum,
                                                     UpdateTime = DateTime.Now,
                                                 })
                                                 .Where(x => x.InventoryNum == datas[i].InventoryNum)
                                                 .ExecuteCommandAsync();
                    }

                    // 提交事务
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    // 回滚事务
                    scope.Dispose();
                    throw Oops.Oh("错误：" + ex);
                }
            }
        }
        else
        {
            throw Oops.Oh("任务下发失败");
        }
    }
}


