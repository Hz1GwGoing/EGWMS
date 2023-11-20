﻿using Admin.NET.Application.Service.EG_WMS_InAndOutBound;
using Furion.Schedule;
using System.Threading;

namespace Admin.NET.Application.Job;

/// <summary>
/// 再次请求AGV任务
/// </summary>
[JobDetail("repeat_AgvTaskJob", Description = "再次请求AGV任务", GroupName = "AGVTask", Concurrent = false)]
[Daily(TriggerId = "trigger_repeatAgvTaskJob", Description = "再次请求AGV任务")]
public class AddAgvStagingTask : IJob
{
    #region 关系注入
    private readonly IServiceProvider _serviceProvider;
    private static readonly TaskService taskService = new TaskService();
    private readonly BaseService baseService;
    private readonly EG_WMS_InAndOutBoundMessage inoutboundMessage;
    private readonly SqlSugarRepository<Entity.EG_WMS_InAndOutBound> _InAndOutBound = App.GetService<SqlSugarRepository<Entity.EG_WMS_InAndOutBound>>();
    private readonly SqlSugarRepository<Entity.EG_WMS_InAndOutBoundDetail> _InAndOutBoundDetail = App.GetService<SqlSugarRepository<Entity.EG_WMS_InAndOutBoundDetail>>();
    private readonly SqlSugarRepository<EG_WMS_Tem_Inventory> _TemInventory = App.GetService<SqlSugarRepository<EG_WMS_Tem_Inventory>>();
    private readonly SqlSugarRepository<EG_WMS_Tem_InventoryDetail> _TemInventoryDetail = App.GetService<SqlSugarRepository<EG_WMS_Tem_InventoryDetail>>();
    private readonly SqlSugarRepository<TaskStagingEntity> _TaskStagingEntity = App.GetService<SqlSugarRepository<TaskStagingEntity>>();
    private readonly SqlSugarRepository<EG_WMS_WorkBin> _WorkBin = App.GetService<SqlSugarRepository<EG_WMS_WorkBin>>();

    #endregion

    #region 构造函数
    public AddAgvStagingTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    #endregion

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        // 检索暂存任务表里面有没有没有下发未完成的任务
        var taskstaging = _TaskStagingEntity.GetFirstAsync(x => x.StagingStatus == 0);

        if (taskstaging == null)
        {
            throw Oops.Oh("当前没有暂存的任务");
        }

        // 通过检索出来的任务得到入库编号
        // 得到临时库存表里面这次入库的物料编号
        // 因为一次入库里面的物料只能是一种（前提）
        var teminvData = _TemInventory.AsQueryable()
                      .Where(x => x.InAndOutBoundNum == taskstaging.Result.InAndOutBoundNum)
                      .ToList();

        // 重新获取库位编号
        string endStorage = baseService.AGVStrategyReturnRecommEndStorage(teminvData[0].MaterielNum);

        if (endStorage == "没有合适的库位")
        {
            throw Oops.Oh("当前依然没有合适的库位！");
        }

        // 重新下发AGV任务

        TaskEntity taskEntity = taskstaging.Adapt<TaskEntity>();
        taskstaging.Result.TaskPath += endStorage;
        DHMessage item = await taskService.AddAsync(taskEntity);
        if (item.code == 1000)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    // 修改库位状态
                    await inoutboundMessage.ModifyInventoryLocationOccupancy(taskstaging.Result.InAndOutBoundNum, endStorage);

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
                                   })
                                   .Where(x => x.InAndOutBoundNum == taskstaging.Result.InAndOutBoundNum)
                                   .ExecuteCommandAsync();

                    await _InAndOutBoundDetail.AsUpdateable()
                                              .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                                              {
                                                  StorageNum = endStorage,
                                                  WHNum = _regionWHNum,
                                                  RegionNum = _storageRegionNum,
                                              })
                                              .Where(x => x.InAndOutBoundNum == taskstaging.Result.InAndOutBoundNum)
                                              .ExecuteCommandAsync();

                    await _WorkBin.AsUpdateable()
                          .SetColumns(it => new EG_WMS_WorkBin
                          {
                              StorageNum = endStorage
                          })
                          .Where(x => x.InAndOutBoundNum == taskstaging.Result.InAndOutBoundNum)
                          .ExecuteCommandAsync();

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

    }
}


