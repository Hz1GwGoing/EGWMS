namespace Admin.NET.Application.Job;

/// <summary>
/// 再次请求AGV出库暂存任务
/// </summary>
[JobDetail("trigger_AgvOutBoundTaskJob", Description = "再次请求AGV出库暂存任务", GroupName = "AGVTask", Concurrent = false)]
//[Daily(TriggerId = "trigger_repeatAgvOutBoundTaskJob", Description = "再次请求AGV出库暂存任务")]
[PeriodMinutes(1, TriggerId = "trigger_AgvOutBoundTaskJob", StartNow = false, RunOnStart = false)]

public class AddAgvStagingOutBoundTask : IJob
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
    private readonly SqlSugarRepository<EG_WMS_Storage> _Storage = App.GetService<SqlSugarRepository<EG_WMS_Storage>>();

    #endregion

    #region 构造函数

    public AddAgvStagingOutBoundTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    #endregion

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        using var serviceScope = _serviceProvider.CreateScope();

        // 检索暂存任务表里面有没有没有下发未完成的任务
        var taskstaging = _TaskStagingEntity.GetFirst(x => x.StagingStatus == 0 && x.InAndOutBoundNum.Contains("EGCK"));

        if (taskstaging == null)
        {
            throw Oops.Oh("当前没有暂存的出库任务！");
        }

        // 通过物料编号再次请求库位
        EG_WMS_InAndOutBoundDetail outbounddata = _InAndOutBoundDetail.GetFirst(x => x.InAndOutBoundNum == taskstaging.InAndOutBoundNum);

        string requestStorage = baseService.AGVStrategyReturnRecommendStorageOutBoundJudgeTime(outbounddata.MaterielNum);

        if (requestStorage == "没有合适的库位")
        {
            throw Oops.Oh("当前仍然没有合适的库位！");
        }

        // 重新下发任务

        TaskEntity taskEntity = taskstaging.Adapt<TaskEntity>();
        taskEntity.TaskPath = requestStorage + taskEntity.TaskPath;
        DHMessage item = await taskService.AddAsync(taskEntity);
        if (item.code == 1000)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    // 根据库位编号查询这个库位上面所存放的数据
                    var tem_InventoryDetails = _TemInventoryDetail.AsQueryable()
                        .InnerJoin<EG_WMS_Tem_Inventory>((a, b) => a.InventoryNum == b.InventoryNum)
                        .Where((a, b) => a.StorageNum == requestStorage && b.OutboundStatus == 0 && a.IsDelete == false && b.IsDelete == false)
                        .ToList();

                    //string wbnum = "";
                    int sumcount = 0;
                    for (int i = 0; i < tem_InventoryDetails.Count; i++)
                    {
                        await _TemInventory.AsUpdateable()
                                        .SetColumns(it => new EG_WMS_Tem_Inventory
                                        {
                                            OutboundStatus = 1,
                                            UpdateTime = DateTime.Now,
                                            // 出库编号
                                            OutBoundNum = taskstaging.InAndOutBoundNum,
                                        })
                                        .Where(x => x.InventoryNum == tem_InventoryDetails[i].InventoryNum)
                                        .ExecuteCommandAsync();

                        // 查询库存数量
                        var invCount = _TemInventory.AsQueryable()
                                     .Where(it => it.InventoryNum == tem_InventoryDetails[i].InventoryNum)
                                     .ToList();

                        // 计算总数
                        sumcount += invCount[0].ICountAll;

                        // 得到每个料箱编号
                        //if (tem_InventoryDetails.Count > 1)
                        //{
                        //    wbnum = tem_InventoryDetails[i].WorkBinNum + "," + wbnum;
                        //}
                        //else
                        //{
                        //    wbnum = tem_InventoryDetails[i].WorkBinNum;
                        //}

                    }

                    // 修改库位表中的状态为占用
                    await _Storage.AsUpdateable()
                              .AS("EG_WMS_Storage")
                              .SetColumns(it => new EG_WMS_Storage
                              {
                                  // 预占用
                                  StorageOccupy = 2,
                                  TaskNo = taskstaging.TaskNo,
                                  UpdateTime = DateTime.Now,

                              })
                              .Where(x => x.StorageNum == requestStorage)
                              .ExecuteCommandAsync();

                    // 根据库位编号查询所在区域
                    string regionnum = inoutboundMessage.GetStorageWhereRegion(requestStorage);
                    string whnum = inoutboundMessage.GetRegionWhereWHNum(regionnum);

                    // 根据得到的库位点，修改出库单据
                    await _InAndOutBound.AsUpdateable()
                                        .SetColumns(it => new EG_WMS_InAndOutBound
                                        {
                                            // 总数
                                            InAndOutBoundCount = sumcount,
                                            // 出库中
                                            InAndOutBoundStatus = 5,
                                            StartPoint = requestStorage,
                                            UpdateTime = DateTime.Now,

                                        })
                                        .Where(x => x.InAndOutBoundNum == taskstaging.InAndOutBoundNum)
                                        .ExecuteCommandAsync();

                    await _InAndOutBoundDetail.AsUpdateable()
                                              .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                                              {
                                                  StorageNum = requestStorage,
                                                  RegionNum = regionnum,
                                                  WHNum = whnum,
                                                  //WorkBinNum = wbnum,
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

                    scope.Complete();
                }
                catch (Exception ex)
                {
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
