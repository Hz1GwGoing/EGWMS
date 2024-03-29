﻿namespace Admin.NET.Application;

/// <summary>
/// 出入库接口服务（agv、人工）
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EG_WMS_InAndOutBoundService : IDynamicApiController, ITransient
{

    #region 关系注入
    // agv接口
    private static readonly TaskService taskService = new TaskService();
    private static readonly BaseService BaseService = new BaseService();
    private static readonly ToolTheCurrentTime _TimeStamp = new ToolTheCurrentTime();
    EG_WMS_InAndOutBoundMessage InAndOutBoundMessage = new EG_WMS_InAndOutBoundMessage();
    private readonly SqlSugarRepository<EG_WMS_InAndOutBound> _rep = App.GetService<SqlSugarRepository<EG_WMS_InAndOutBound>>();
    private readonly SqlSugarRepository<EG_WMS_InAndOutBoundDetail> _InAndOutBoundDetail = App.GetService<SqlSugarRepository<EG_WMS_InAndOutBoundDetail>>();
    private readonly SqlSugarRepository<EG_WMS_Inventory> _Inventory = App.GetService<SqlSugarRepository<EG_WMS_Inventory>>();
    private readonly SqlSugarRepository<EG_WMS_InventoryDetail> _InventoryDetail = App.GetService<SqlSugarRepository<EG_WMS_InventoryDetail>>();
    private readonly SqlSugarRepository<EG_WMS_Storage> _Storage = App.GetService<SqlSugarRepository<EG_WMS_Storage>>();
    private readonly SqlSugarRepository<EG_WMS_Region> _Region = App.GetService<SqlSugarRepository<EG_WMS_Region>>();
    private readonly SqlSugarRepository<EG_WMS_WorkBin> _workbin = App.GetService<SqlSugarRepository<EG_WMS_WorkBin>>();
    private readonly SqlSugarRepository<EG_WMS_Tem_Inventory> _InventoryTem = App.GetService<SqlSugarRepository<EG_WMS_Tem_Inventory>>();
    private readonly SqlSugarRepository<EG_WMS_Tem_InventoryDetail> _InventoryDetailTem = App.GetService<SqlSugarRepository<EG_WMS_Tem_InventoryDetail>>();
    private readonly SqlSugarRepository<TaskEntity> _TaskEntity = App.GetService<SqlSugarRepository<TaskEntity>>();
    private readonly SqlSugarRepository<TemLogicEntity> _TemLogicEntity = App.GetService<SqlSugarRepository<TemLogicEntity>>();
    private readonly SqlSugarRepository<TaskStagingEntity> _TaskStagingEntity = App.GetService<SqlSugarRepository<TaskStagingEntity>>();


    #endregion

    #region 构造函数
    public EG_WMS_InAndOutBoundService()
    {

    }
    #endregion

    #region  AGV入库（入库WMS自动推荐库位）（密集库）（用于测试）

    /// <summary>
    /// AGV入库（入库WMS自动推荐库位）（密集库）（用于测试）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AgvJoinBoundTask", Order = 1)]
    public async Task AgvJoinBoundTask(AgvJoinBoundNewDto input)
    {
        try
        {
            // 生成当前时间时间戳
            string joinboundnum = _TimeStamp.GetTheCurrentTimeTimeStamp("EGRK");
            // 起始点
            string startpoint = input.StartPoint;
            if (startpoint == null || input.EndPoint == "")
            {
                throw Oops.Oh("起始点不可为空");
            }

            // 目标点
            string endpoint = "";
            if (input.EndPoint == null || input.EndPoint == "")
            {
                // 根据策略推荐
                input.EndPoint = BaseService.AGVStrategyReturnRecommEndStorage(input.materielWorkBins[0].MaterielNum);
                // 添加暂存任务
                if (input.EndPoint == "没有合适的库位")
                {
                    await InAndOutBoundMessage.NotStorageAddStagingTask(input, joinboundnum);
                    return;
                }
            }
            else
            {
                // 判断用户输入的是否符合逻辑
                var storageGroup = _Storage.GetFirstAsync(x => x.StorageNum == input.EndPoint);
                if (storageGroup == null)
                {
                    throw Oops.Oh("没有查找到该目标点的库位编号！");
                }
                else if (storageGroup.Result.StorageOccupy == 1 || storageGroup.Result.StorageOccupy == 2)
                {
                    throw Oops.Oh("目标库位已被占用！");
                }
                var selectData = _Storage.AsQueryable()
                     .Where(x => x.StorageGroup == storageGroup.Result.StorageGroup && x.StorageOccupy == 1)
                     .OrderBy(x => x.StorageNum, OrderByType.Asc)
                     .Select(x => x.StorageNum)
                     .ToList();
                if (storageGroup.Result.StorageNum.ToInt() > selectData[0].ToInt())
                {
                    throw Oops.Oh("当前选择的这个库位，入库时有库位阻挡，请重新选择库位！");
                }
            }
            endpoint = input.EndPoint;

            // 任务点集
            var positions = startpoint + "," + endpoint;

            TaskEntity taskEntity = input.Adapt<TaskEntity>();
            taskEntity.TaskPath = positions;
            taskEntity.InAndOutBoundNum = joinboundnum;

            // 下达agv任务

            DHMessage item = await taskService.AddAsync(taskEntity);

            // 下达agv任务成功
            if (item.code == 1000)
            {
                //使用Task.Delay方法让当前任务暂停执行1秒钟（1000毫秒）。await关键字表示等待异步操作完成。
                //await Task.Delay(1000);
                using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {

                    try
                    {
                        // 得到入库的数据
                        List<MaterielProductDto> list = input.materielWorkBins;
                        // 根据入库编号查询任务编号
                        var datatask = await _TaskEntity.GetFirstAsync(x => x.InAndOutBoundNum == joinboundnum);

                        List<DateTime> datetime = new List<DateTime>();
                        // 将料箱的生产日期保存
                        for (int i = 0; i < list.Count; i++)
                        {
                            datetime.Add(list[i].ProductionDate);
                        }

                        // 修改库位表中的状态为占用
                        await _Storage.AsUpdateable()
                                  .AS("EG_WMS_Storage")
                                  .SetColumns(it => new EG_WMS_Storage
                                  {
                                      // 预占用
                                      StorageOccupy = 2,
                                      TaskNo = datatask.TaskNo,
                                      // 得到日期最大的生产日期
                                      StorageProductionDate = datetime.Max(),
                                  })
                                  .Where(x => x.StorageNum == input.EndPoint)
                                  .ExecuteCommandAsync();


                        #region 入库操作

                        // 生成入库单
                        EG_WMS_InAndOutBound joinbound = new EG_WMS_InAndOutBound
                        {
                            InAndOutBoundNum = joinboundnum,
                            InAndOutBoundType = 0,
                            InAndOutBoundTime = DateTime.Now,
                            InAndOutBoundUser = input.AddName,
                            InAndOutBoundRemake = input.InAndOutBoundRemake,
                            CreateTime = DateTime.Now,
                            StartPoint = input.StartPoint,
                            EndPoint = input.EndPoint,
                        };

                        // 查询库位编号所在的区域编号
                        string regionnum = InAndOutBoundMessage.GetStorageWhereRegion(input.EndPoint);
                        string whnum = InAndOutBoundMessage.GetRegionWhereWHNum(regionnum);

                        // 生成入库详单

                        EG_WMS_InAndOutBoundDetail joinbounddetail = new EG_WMS_InAndOutBoundDetail()
                        {
                            InAndOutBoundNum = joinboundnum,
                            CreateTime = DateTime.Now,
                            RegionNum = regionnum,
                            StorageNum = input.EndPoint,
                        };
                        #endregion

                        await _rep.InsertAsync(joinbound);
                        await _InAndOutBoundDetail.InsertAsync(joinbounddetail);

                        string wbnum = "";
                        int sumcount = 0;
                        for (int i = 0; i < list.Count; i++)
                        {
                            // 库存编号
                            string inventorynum = $"{i}EGKC" + _TimeStamp.GetTheCurrentTimeTimeStamp();
                            // 物料编号
                            string materienum = list[i].MaterielNum;
                            // 物料的数量
                            int productcount = list[i].ProductCount;
                            // 生产日期
                            DateTime productiondate = list[i].ProductionDate;
                            // 生产批次
                            string productionlot = list[i].ProductionLot;
                            // 总数
                            sumcount += productcount;

                            // 临时库存主表
                            EG_WMS_Tem_Inventory addInventory = new EG_WMS_Tem_Inventory()
                            {
                                InventoryNum = inventorynum,
                                MaterielNum = materienum,
                                ICountAll = productcount,
                                CreateTime = DateTime.Now,
                                InBoundNum = joinboundnum,
                                IsDelete = false,
                                OutboundStatus = 0,
                                ProductionDate = productiondate,
                            };

                            // 临时详细表
                            EG_WMS_Tem_InventoryDetail addInventoryDetail = new EG_WMS_Tem_InventoryDetail()
                            {
                                InventoryNum = inventorynum,
                                //WorkBinNum = workbinnum,
                                ProductionLot = productionlot,
                                CreateTime = DateTime.Now,
                                StorageNum = input.EndPoint,
                                RegionNum = regionnum,
                                WHNum = whnum,
                                IsDelete = false,
                            };

                            // 料箱表
                            //EG_WMS_WorkBin addWorkBin = new EG_WMS_WorkBin()
                            //{
                            //    ProductCount = productcount,
                            //    ProductionLot = productionlot,
                            //    CreateTime = DateTime.Now,
                            //    ProductionDate = productiondate,
                            //    WorkBinStatus = 0,
                            //    MaterielNum = materienum,
                            //    StorageNum = input.EndPoint,
                            //    InAndOutBoundNum = joinboundnum,
                            //};

                            // 将数据保存到临时表中
                            await _InventoryTem.InsertAsync(addInventory);
                            await _InventoryDetailTem.InsertAsync(addInventoryDetail);
                            //await _workbin.InsertOrUpdateAsync(addWorkBin);

                            // 得到每个料箱编号
                            //if (list.Count > 1)
                            //{
                            //    wbnum = workbinnum + "," + wbnum;
                            //}
                            //else
                            //{
                            //    wbnum = workbinnum;
                            //}
                        }
                        // 修改入库详情表里面的料箱编号和物料编号

                        await _InAndOutBoundDetail.AsUpdateable()
                                             .AS("EG_WMS_InAndOutBoundDetail")
                                             .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                                             {
                                                 WHNum = whnum,
                                                 WorkBinNum = wbnum,
                                                 // 只会有一种物料
                                                 MaterielNum = list[0].MaterielNum,
                                             })
                                             .Where(u => u.InAndOutBoundNum == joinboundnum)
                                             .ExecuteCommandAsync();

                        // 改变入库状态
                        await _rep.AsUpdateable()
                             .AS("EG_WMS_InAndOutBound")
                             .SetColumns(it => new EG_WMS_InAndOutBound
                             {
                                 InAndOutBoundCount = sumcount,
                                 // 入库中
                                 InAndOutBoundStatus = 4,
                             })
                             .Where(u => u.InAndOutBoundNum == joinboundnum)
                             .ExecuteCommandAsync();

                        // 提交事务
                        scope.Complete();
                    }
                    catch (Exception ex)
                    {
                        // 回滚事务
                        scope.Dispose();
                        throw Oops.Oh(ex.Message);
                    }
                }
            }
            else
            {
                throw Oops.Oh("下达AGV任务失败");
            }
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ex.Message);
        }

    }

    #endregion

    #region 潜伏举升AGV入库（需要前往等待点，到达等待点再获取库位点）

    ///// <summary>
    ///// 潜伏举升AGV入库（需要前往等待点，到达等待点再获取库位点）
    ///// </summary>
    ///// <param name="input"></param>
    ///// <returns></returns>
    ///// <exception cref="Exception"></exception>
    //[HttpPost]
    //[ApiDescriptionSettings(Name = "AgvJoinBoundTaskGOPoint", Order = 97)]
    //public async Task<string> AgvJoinBoundTaskGOPoint(AgvJoinDto input)
    //{
    //    try
    //    {
    //        // 生成当前时间时间戳
    //        string joinboundnum = _TimeStamp.GetTheCurrentTimeTimeStamp("EGRK");

    //        // 起始点
    //        string startpoint = input.StartPoint;
    //        if (startpoint == null || input.EndPoint == "")
    //        {
    //            throw Oops.Oh("起始点不可为空");
    //        }

    //        // 目标点（前往等待点）
    //        if (input.EndPoint == null || input.EndPoint == "")
    //        {
    //            // 前往等待点，到达等待点后rcs自动返回目标库位
    //            // 查询得到这个物料属于那个区域（一个区域下只有一个等待点）
    //            var regionList = _Region.AsQueryable()
    //                    .Where(x => x.RegionMaterielNum == input.materielWorkBins[0].MaterielNum)
    //                    .ToList();

    //            var storageList = _Storage.AsQueryable()
    //                     .Where(x => x.RegionNum == regionList[0].RegionNum && x.StorageHoldingPoint == 1)
    //                     .Select(x => x.StorageNum)
    //                     .ToList();

    //            if (storageList == null || storageList.Count == 0)
    //            {
    //                throw Oops.Oh("此区域未设置等待点");
    //            }

    //            input.EndPoint = storageList[0];

    //        }

    //        string endpoint = input.EndPoint;

    //        // 任务点集
    //        var positions = startpoint + "," + endpoint;

    //        TaskEntity taskEntity = input.Adapt<TaskEntity>();
    //        taskEntity.TaskPath = positions;
    //        taskEntity.InAndOutBoundNum = joinboundnum;

    //        // 下达agv任务

    //        DHMessage item = await taskService.AddAsync(taskEntity);

    //        // 下达agv任务成功
    //        if (item.code == 1000)
    //        {
    //            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
    //            {

    //                try
    //                {
    //                    // 得到入库的数据
    //                    List<MaterielWorkBin> list = input.materielWorkBins;
    //                    #region 入库操作
    //                    // 生成入库单
    //                    EG_WMS_InAndOutBound joinbound = new EG_WMS_InAndOutBound
    //                    {
    //                        InAndOutBoundNum = joinboundnum,
    //                        InAndOutBoundType = 0,
    //                        InAndOutBoundTime = DateTime.Now,
    //                        InAndOutBoundUser = input.AddName,
    //                        InAndOutBoundRemake = input.InAndOutBoundRemake,
    //                        CreateTime = DateTime.Now,
    //                        StartPoint = input.StartPoint,
    //                        EndPoint = "AGV正在前往等待点，存放的库位等AGV到达等待点后自动获取",
    //                    };
    //                    // 生成入库详单
    //                    EG_WMS_InAndOutBoundDetail joinbounddetail = new EG_WMS_InAndOutBoundDetail()
    //                    {
    //                        InAndOutBoundNum = joinboundnum,
    //                        CreateTime = DateTime.Now,
    //                        RegionNum = null,
    //                        WHNum = null,
    //                        // 目标点就是存储的点位即库位编号
    //                        StorageNum = "AGV正在前往等待点，存放的库位等AGV到达等待点后自动获取",
    //                    };
    //                    #endregion
    //                    await _rep.InsertAsync(joinbound);
    //                    await _InAndOutBoundDetail.InsertAsync(joinbounddetail);
    //                    string wbnum = "";
    //                    int sumcount = 0;
    //                    for (int i = 0; i < list.Count; i++)
    //                    {
    //                        string inventorynum = $"{i}EGKC" + _TimeStamp.GetTheCurrentTimeTimeStamp();
    //                        string workbinnum = list[i].WorkBinNum;
    //                        string materienum = list[i].MaterielNum;
    //                        int productcount = list[i].ProductCount;
    //                        DateTime productiondate = list[i].ProductionDate;
    //                        string productionlot = list[i].ProductionLot;
    //                        // 总数
    //                        sumcount += productcount;
    //                        // 将得到的数据，保存在临时的库存主表和详细表中
    //                        EG_WMS_Tem_Inventory addInventory = new EG_WMS_Tem_Inventory()
    //                        {
    //                            InventoryNum = inventorynum,
    //                            MaterielNum = materienum,
    //                            ICountAll = productcount,
    //                            CreateTime = DateTime.Now,
    //                            InBoundNum = joinboundnum,
    //                            IsDelete = false,
    //                            OutboundStatus = 0,
    //                            ProductionDate = productiondate,
    //                        };
    //                        EG_WMS_Tem_InventoryDetail addInventoryDetail = new EG_WMS_Tem_InventoryDetail()
    //                        {
    //                            InventoryNum = inventorynum,
    //                            WorkBinNum = workbinnum,
    //                            ProductionLot = productionlot,
    //                            CreateTime = DateTime.Now,
    //                            StorageNum = "AGV正在前往等待点，存放的库位等AGV到达等待点后自动获取",
    //                            RegionNum = null,
    //                            WHNum = null,
    //                            IsDelete = false,
    //                        };
    //                        EG_WMS_WorkBin addWorkBin = new EG_WMS_WorkBin()
    //                        {
    //                            WorkBinNum = workbinnum,
    //                            ProductCount = productcount,
    //                            ProductionLot = productionlot,
    //                            CreateTime = DateTime.Now,
    //                            ProductionDate = productiondate,
    //                            WorkBinStatus = 0,
    //                            MaterielNum = materienum,
    //                            StorageNum = "AGV正在前往等待点，存放的库位等AGV到达等待点后自动获取",
    //                            InAndOutBoundNum = joinboundnum,
    //                        };
    //                        // 将数据保存到临时表中
    //                        await _InventoryTem.InsertAsync(addInventory);
    //                        await _InventoryDetailTem.InsertAsync(addInventoryDetail);
    //                        await _workbin.InsertOrUpdateAsync(addWorkBin);
    //                        // 得到每个料箱编号
    //                        if (list.Count > 1)
    //                        {
    //                            wbnum = workbinnum + "," + wbnum;
    //                        }
    //                        else
    //                        {
    //                            wbnum = workbinnum;
    //                        }
    //                    }
    //                    // 修改入库详情表里面的料箱编号和物料编号

    //                    await _InAndOutBoundDetail.AsUpdateable()
    //                                         .AS("EG_WMS_InAndOutBoundDetail")
    //                                         .SetColumns(it => new EG_WMS_InAndOutBoundDetail
    //                                         {
    //                                             WorkBinNum = wbnum,
    //                                             // 只会有一种物料
    //                                             MaterielNum = list[0].MaterielNum,
    //                                         })
    //                                         .Where(u => u.InAndOutBoundNum == joinboundnum)
    //                                         .ExecuteCommandAsync();

    //                    // 改变入库状态
    //                    await _rep.AsUpdateable()
    //                         .AS("EG_WMS_InAndOutBound")
    //                         .SetColumns(it => new EG_WMS_InAndOutBound
    //                         {
    //                             InAndOutBoundCount = sumcount,
    //                             // 入库中
    //                             InAndOutBoundStatus = 4,
    //                         })
    //                         .Where(u => u.InAndOutBoundNum == joinboundnum)
    //                         .ExecuteCommandAsync();

    //                    // 提交事务
    //                    scope.Complete();
    //                    return "下发AGV前往等待点任务成功！";
    //                }
    //                catch (Exception ex)
    //                {
    //                    // 回滚事务
    //                    scope.Dispose();
    //                    throw Oops.Oh(ex.Message);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            throw Oops.Oh("下发AGV前往等待点任务失败！");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        throw Oops.Oh(ex.Message);
    //    }
    //}


    #endregion

    #region 潜伏举升AGV入库动态切换策略（入库WMS自动推荐库位）
    ///// <summary>
    ///// 潜伏举升AGV入库（入库WMS自动推荐库位）
    ///// </summary>
    ///// <param name="input"></param>
    ///// <param name="type"></param>
    ///// <returns></returns>
    //[HttpPost]
    //[ApiDescriptionSettings(Name = "AgvJoinBoundTasks", Order = 99)]
    //public async Task<string> AgvJoinBoundTasks(AgvJoinDto input, string? type = "LatentLiftInA")
    //{
    //    try
    //    {
    //        // 生成当前时间时间戳
    //        string joinboundnum = _TimeStamp.GetTheCurrentTimeTimeStamp("EGRK");
    //        // 起始点
    //        string startpoint = input.StartPoint;
    //        if (startpoint == null || input.EndPoint == "")
    //        {
    //            throw Oops.Oh("起始点不可为空");
    //        }

    //        // 目标点
    //        string endpoint = "";
    //        if (input.EndPoint == null || input.EndPoint == "")
    //        {
    //            // 动态切换策略
    //            switch (type)
    //            {
    //                case "LatentLiftInA":
    //                    StrategyClient strategyClient = new StrategyClient(new AGVStrategyReturnRecommEndStorage());
    //                    input.EndPoint = strategyClient.ContextInterface(input.materielWorkBins[0].MaterielNum);
    //                    break;
    //                default:
    //                    input.EndPoint = BaseService.AGVStrategyReturnRecommEndStorage(input.materielWorkBins[0].MaterielNum);
    //                    break;
    //            }

    //            // 添加暂存任务
    //            if (input.EndPoint == "没有合适的库位")
    //            {
    //                // TODO：查找有没有重复暂存的任务

    //                await InAndOutBoundMessage.NotStorageAddStagingTask(input, joinboundnum);
    //                return "添加AGV入库暂存任务成功！";
    //            }
    //        }
    //        else
    //        {
    //            // 判断用户输入的是否符合逻辑
    //            var storageGroup = _Storage.GetFirst(x => x.StorageNum == input.EndPoint);
    //            if (storageGroup == null)
    //            {
    //                throw Oops.Oh("没有查找到该目标点的库位编号！");
    //            }
    //            else if (storageGroup.StorageOccupy == 1 || storageGroup.StorageOccupy == 2)
    //            {
    //                throw Oops.Oh("目标库位已被占用！");
    //            }
    //            var selectData = _Storage.AsQueryable()
    //                 .Where(x => x.StorageGroup == storageGroup.StorageGroup && x.StorageOccupy == 1)
    //                 .OrderBy(x => x.StorageNum, OrderByType.Asc)
    //                 .Select(x => x.StorageNum)
    //                 .ToList();
    //            if (storageGroup.StorageNum.ToInt() > selectData[0].ToInt())
    //            {
    //                throw Oops.Oh("当前这个库位，入库时有库位阻挡，请重新选择库位！");
    //            }
    //        }
    //        endpoint = input.EndPoint;

    //        // 任务点集
    //        var positions = startpoint + "," + endpoint;

    //        TaskEntity taskEntity = input.Adapt<TaskEntity>();
    //        taskEntity.TaskPath = positions;
    //        taskEntity.InAndOutBoundNum = joinboundnum;

    //        // 下达agv任务

    //        DHMessage item = await taskService.AddAsync(taskEntity);

    //        // 下达agv任务成功
    //        if (item.code == 1000)
    //        {
    //            await InAndOutBoundMessage.ProcessInbound(input, joinboundnum);
    //            return "下发AGV入库任务成功！";
    //        }
    //        else
    //        {
    //            throw Oops.Oh("下发AGV入库任务失败！");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        throw Oops.Oh(ex.Message);
    //    }

    //}

    #endregion

    #region 潜伏举升AGV出库动态切换策略（出库WMS自动推荐库位）

    ///// <summary>
    ///// 潜伏举升AGV出库（出库WMS自动推荐库位）
    ///// </summary>
    ///// <param name="input"></param>
    ///// <param name="type"></param>
    ///// <returns></returns>
    ///// <exception cref="Exception"></exception>
    //[HttpPost]
    //[ApiDescriptionSettings(Name = "AgvOutBoundTask", Order = 98)]
    //public async Task<string> AgvOutBoundTask(AgvBoundDto input, string? type = "LatentLiftOutA")
    //{
    //    // 生成当前时间时间戳
    //    string outboundnum = _TimeStamp.GetTheCurrentTimeTimeStamp("EGCK");

    //    try
    //    {
    //        // 目标点
    //        if (input.EndPoint == null || input.EndPoint == "")
    //        {
    //            throw Oops.Oh("目标点不能为空");
    //        }

    //        // 起始点
    //        string startpoint = "";
    //        if (input.StartPoint == null || input.StartPoint == "")
    //        {
    //            // 动态切换策略
    //            switch (type)
    //            {
    //                case "LatentLiftOutA":
    //                    StrategyClient strategyClient = new StrategyClient(new AGVStrategyReturnRecommendStorageOutBoundJudgeTime());
    //                    input.EndPoint = strategyClient.ContextInterface(input.MaterielNum);
    //                    break;
    //                case "LatentLiftOutB":
    //                    StrategyClient strategyclient = new StrategyClient(new AGVStrategyReturnRecommendStorageOutBound());
    //                    input.EndPoint = strategyclient.ContextInterface(input.MaterielNum);
    //                    break;
    //                default:
    //                    input.StartPoint = BaseService.AGVStrategyReturnRecommendStorageOutBoundJudgeTime(input.MaterielNum);
    //                    break;
    //            }

    //            if (input.StartPoint == "没有合适的库位")
    //            {
    //                // 添加出库暂存任务
    //                await InAndOutBoundMessage.NotBoundNotStorageAddStagingTask(input, outboundnum);
    //                return "添加AGV出库暂存任务成功！";
    //            }
    //        }
    //        else
    //        {
    //            // 判断用户输入的是否符合逻辑
    //            var storageGroup = _Storage.GetFirst(x => x.StorageNum == input.StartPoint);
    //            if (storageGroup == null)
    //            {
    //                throw Oops.Oh("没有查找到该起始点的库位编号！");
    //            }
    //            else if (storageGroup.StorageOccupy != 1)
    //            {
    //                throw Oops.Oh("当前库位上不存在库存！");
    //            }
    //            var selectData = _Storage.AsQueryable()
    //                 .Where(x => x.StorageGroup == storageGroup.StorageGroup && x.StorageOccupy == 1)
    //                 .OrderBy(x => x.StorageNum, OrderByType.Asc)
    //                 .Select(x => x.StorageNum)
    //                 .ToList();
    //            if (storageGroup.StorageNum.ToInt() > selectData[0].ToInt())
    //            {
    //                throw Oops.Oh("当前这个库位，出库时有库位阻挡，请重新选择库位！");
    //            }
    //        }
    //        startpoint = input.StartPoint;
    //        string endpoint = input.EndPoint;

    //        // 任务点集
    //        var positions = startpoint + "," + endpoint;

    //        TaskEntity taskEntity = input.Adapt<TaskEntity>();
    //        taskEntity.TaskPath = positions;
    //        taskEntity.InAndOutBoundNum = outboundnum;

    //        // 下达agv任务

    //        DHMessage item = await taskService.AddAsync(taskEntity);

    //        // 判断agv出库是否成功

    //        if (item.code == 1000)
    //        {
    //            // 根据出库编号查询任务编号
    //            var datatask = await _TaskEntity.GetFirstAsync(x => x.InAndOutBoundNum == outboundnum);

    //            // 修改库位表中的状态为占用
    //            await _Storage.AsUpdateable()
    //                      .AS("EG_WMS_Storage")
    //                      .SetColumns(it => new EG_WMS_Storage
    //                      {
    //                          // 预占用
    //                          StorageOccupy = 2,
    //                          TaskNo = datatask.TaskNo,
    //                          UpdateTime = DateTime.Now,
    //                      })
    //                      .Where(x => x.StorageNum == input.StartPoint)
    //                      .ExecuteCommandAsync();
    //            #region 生成出库

    //            // 生成出库单
    //            EG_WMS_InAndOutBound outbound = new EG_WMS_InAndOutBound
    //            {
    //                InAndOutBoundNum = outboundnum,
    //                InAndOutBoundType = 1,
    //                InAndOutBoundTime = DateTime.Now,
    //                InAndOutBoundUser = input.AddName,
    //                InAndOutBoundRemake = input.InAndOutBoundRemake,
    //                CreateTime = DateTime.Now,
    //                StartPoint = startpoint,
    //                EndPoint = endpoint,
    //            };

    //            // 查询库位编号所在的区域编号
    //            var regionnum = InAndOutBoundMessage.GetStorageWhereRegion(startpoint);
    //            var whnum = InAndOutBoundMessage.GetRegionWhereWHNum(regionnum);

    //            // 生成出库详单
    //            EG_WMS_InAndOutBoundDetail outbounddetail = new EG_WMS_InAndOutBoundDetail()
    //            {
    //                InAndOutBoundNum = outboundnum,
    //                CreateTime = DateTime.Now,
    //                MaterielNum = input.MaterielNum,
    //                RegionNum = regionnum,
    //                StorageNum = input.StartPoint,
    //            };

    //            await _rep.InsertAsync(outbound);
    //            await _InAndOutBoundDetail.InsertAsync(outbounddetail);

    //            // 得到这个库位上的库存信息（先修改临时表里面的信息）

    //            var tem_InventoryDetails = _InventoryDetailTem.AsQueryable()
    //                                .InnerJoin<EG_WMS_Tem_Inventory>((a, b) => a.InventoryNum == b.InventoryNum)
    //                                .Where((a, b) => a.StorageNum == startpoint && b.OutboundStatus == 0 && a.IsDelete == false && b.IsDelete == false)
    //                                .ToList();

    //            string wbnum = "";
    //            int sumcount = 0;
    //            for (int i = 0; i < tem_InventoryDetails.Count; i++)
    //            {
    //                await _InventoryTem.AsUpdateable()
    //                                .SetColumns(it => new EG_WMS_Tem_Inventory
    //                                {
    //                                    OutboundStatus = 1,
    //                                    UpdateTime = DateTime.Now,
    //                                    // 出库编号
    //                                    OutBoundNum = outboundnum,
    //                                })
    //                                .Where(x => x.InventoryNum == tem_InventoryDetails[i].InventoryNum)
    //                                .ExecuteCommandAsync();

    //                // 查询库存数量
    //                var invCount = _InventoryTem.AsQueryable()
    //                             .Where(it => it.InventoryNum == tem_InventoryDetails[i].InventoryNum)
    //                             .ToList();

    //                // 计算总数
    //                sumcount += invCount[0].ICountAll;

    //                // 得到每个料箱编号
    //                if (tem_InventoryDetails.Count > 1)
    //                {
    //                    wbnum = tem_InventoryDetails[i].WorkBinNum + "," + wbnum;
    //                }
    //                else
    //                {
    //                    wbnum = tem_InventoryDetails[i].WorkBinNum;
    //                }

    //            }
    //            // 修改出库详情表里面的料箱编号和物料编号
    //            await _InAndOutBoundDetail.AsUpdateable()
    //                                 .AS("EG_WMS_InAndOutBoundDetail")
    //                                 .SetColumns(it => new EG_WMS_InAndOutBoundDetail
    //                                 {
    //                                     WHNum = whnum,
    //                                     WorkBinNum = wbnum,
    //                                     MaterielNum = input.MaterielNum,
    //                                 })
    //                                 .Where(u => u.InAndOutBoundNum == outboundnum)
    //                                 .ExecuteCommandAsync();
    //            // 改变出库状态
    //            await _rep.AsUpdateable()
    //                 .AS("EG_WMS_InAndOutBound")
    //                 .SetColumns(it => new EG_WMS_InAndOutBound
    //                 {
    //                     // 总数
    //                     InAndOutBoundCount = sumcount,
    //                     // 出库中
    //                     InAndOutBoundStatus = 5,
    //                 })
    //                 .Where(u => u.InAndOutBoundNum == outboundnum)
    //                 .ExecuteCommandAsync();

    //            #endregion
    //            return "下发AGV出库任务成功！";
    //        }
    //        else
    //        {
    //            throw Oops.Oh("下发AGV出库任务失败！");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        throw Oops.Oh(ex.Message);
    //    }
    //}

    #endregion

    #region 潜伏举升AGV入库（需要前往等待点，到达等待点再获取库位点）（宇翔项目）

    /// <summary>
    /// 潜伏举升AGV入库（需要前往等待点，到达等待点再获取库位点）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AgvJoinBoundTaskGOPoint", Order = 97)]
    public async Task<string> AgvJoinBoundTaskGOPoint(AgvJoinBoundNewDto input)
    {
        try
        {
            // 生成当前时间时间戳
            string joinboundnum = _TimeStamp.GetTheCurrentTimeTimeStamp("EGRK");

            // 起始点
            string startpoint = input.StartPoint;
            if (startpoint == null || input.EndPoint == "")
            {
                throw Oops.Oh("起始点不可为空");
            }

            // 目标点（前往等待点）
            if (input.EndPoint == null || input.EndPoint == "")
            {
                // 前往等待点，到达等待点后rcs自动返回目标库位
                // 查询得到这个物料属于那个区域（一个区域下只有一个等待点）
                var regionList = _Region.AsQueryable()
                        .Where(x => x.RegionMaterielNum == input.materielWorkBins[0].MaterielNum)
                        .ToList();

                var storageList = _Storage.AsQueryable()
                         .Where(x => x.RegionNum == regionList[0].RegionNum && x.StorageHoldingPoint == 1)
                         .Select(x => x.StorageNum)
                         .ToList();

                if (storageList == null || storageList.Count == 0)
                {
                    throw Oops.Oh("此区域未设置等待点");
                }

                input.EndPoint = storageList[0];

            }

            string endpoint = input.EndPoint;

            // 任务点集
            var positions = startpoint + "," + endpoint;

            TaskEntity taskEntity = input.Adapt<TaskEntity>();
            taskEntity.TaskPath = positions;
            taskEntity.InAndOutBoundNum = joinboundnum;

            // 下达agv任务

            DHMessage item = await taskService.AddAsync(taskEntity);

            // 下达agv任务成功
            if (item.code == 1000)
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {

                    try
                    {
                        // 得到入库的数据
                        List<MaterielProductDto> list = input.materielWorkBins;
                        #region 入库操作
                        // 生成入库单
                        EG_WMS_InAndOutBound joinbound = new EG_WMS_InAndOutBound
                        {
                            InAndOutBoundNum = joinboundnum,
                            InAndOutBoundType = 0,
                            InAndOutBoundTime = DateTime.Now,
                            InAndOutBoundUser = input.AddName,
                            InAndOutBoundRemake = input.InAndOutBoundRemake,
                            CreateTime = DateTime.Now,
                            StartPoint = input.StartPoint,
                            EndPoint = "AGV正在前往等待点，存放的库位等AGV到达等待点后自动获取",
                        };
                        // 生成入库详单
                        EG_WMS_InAndOutBoundDetail joinbounddetail = new EG_WMS_InAndOutBoundDetail()
                        {
                            InAndOutBoundNum = joinboundnum,
                            CreateTime = DateTime.Now,
                            MaterielNum = list[0].MaterielNum,
                            RegionNum = null,
                            WHNum = null,
                            // 目标点就是存储的点位即库位编号
                            StorageNum = "AGV正在前往等待点，存放的库位等AGV到达等待点后自动获取",
                        };
                        #endregion
                        await _rep.InsertAsync(joinbound);
                        await _InAndOutBoundDetail.InsertAsync(joinbounddetail);
                        int sumcount = 0;
                        for (int i = 0; i < list.Count; i++)
                        {
                            string inventorynum = $"{i}EGKC" + _TimeStamp.GetTheCurrentTimeTimeStamp();
                            string materienum = list[i].MaterielNum;
                            int productcount = list[i].ProductCount;
                            DateTime productiondate = list[i].ProductionDate;
                            string productionlot = list[i].ProductionLot;
                            // 总数
                            sumcount += productcount;
                            // 将得到的数据，保存在临时的库存主表和详细表中
                            EG_WMS_Tem_Inventory addInventory = new EG_WMS_Tem_Inventory()
                            {
                                InventoryNum = inventorynum,
                                MaterielNum = materienum,
                                ICountAll = productcount,
                                CreateTime = DateTime.Now,
                                InBoundNum = joinboundnum,
                                IsDelete = false,
                                OutboundStatus = 0,
                                ProductionDate = productiondate,
                            };
                            EG_WMS_Tem_InventoryDetail addInventoryDetail = new EG_WMS_Tem_InventoryDetail()
                            {
                                InventoryNum = inventorynum,
                                ProductionLot = productionlot,
                                CreateTime = DateTime.Now,
                                StorageNum = "AGV正在前往等待点，存放的库位等AGV到达等待点后自动获取",
                                RegionNum = null,
                                WHNum = null,
                                IsDelete = false,
                            };
                            // 将数据保存到临时表中
                            await _InventoryTem.InsertAsync(addInventory);
                            await _InventoryDetailTem.InsertAsync(addInventoryDetail);
                        }

                        // 改变入库状态
                        await _rep.AsUpdateable()
                             .AS("EG_WMS_InAndOutBound")
                             .SetColumns(it => new EG_WMS_InAndOutBound
                             {
                                 InAndOutBoundCount = sumcount,
                                 // 入库中
                                 InAndOutBoundStatus = 4,
                             })
                             .Where(u => u.InAndOutBoundNum == joinboundnum)
                             .ExecuteCommandAsync();

                        // 提交事务
                        scope.Complete();
                        return "下发AGV前往等待点任务成功！";
                    }
                    catch (Exception ex)
                    {
                        // 回滚事务
                        scope.Dispose();
                        throw Oops.Oh(ex.Message);
                    }
                }
            }
            else
            {
                throw Oops.Oh("下发AGV前往等待点任务失败！");
            }
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ex.Message);
        }
    }


    #endregion

    #region 潜伏举升AGV入库（入库WMS自动推荐库位）（宇翔项目）
    /// <summary>
    /// 潜伏举升AGV入库（入库WMS自动推荐库位）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AgvJoinBoundTasks", Order = 99)]
    public async Task<string> AgvJoinBoundTasks(AgvJoinBoundNewDto input)
    {
        try
        {
            // 生成当前时间时间戳
            string joinboundnum = _TimeStamp.GetTheCurrentTimeTimeStamp("EGRK");
            // 起始点
            string startpoint = input.StartPoint;
            if (startpoint == null || input.EndPoint == "")
            {
                throw Oops.Oh("起始点不可为空");
            }

            // 目标点
            string endpoint = "";
            if (input.EndPoint == null || input.EndPoint == "")
            {
                input.EndPoint = BaseService.AGVStrategyReturnRecommEndStorage(input.materielWorkBins[0].MaterielNum);

                // 添加暂存任务
                if (input.EndPoint == "没有合适的库位")
                {
                    // TODO：查找有没有重复暂存的任务

                    await InAndOutBoundMessage.NotStorageAddStagingTask(input, joinboundnum);
                    return "添加AGV入库暂存任务成功！";
                }
            }
            else
            {
                // 判断用户输入的是否符合逻辑
                var storageGroup = _Storage.GetFirst(x => x.StorageNum == input.EndPoint);
                if (storageGroup == null)
                {
                    throw Oops.Oh("没有查找到该目标点的库位编号！");
                }
                else if (storageGroup.StorageOccupy == 1 || storageGroup.StorageOccupy == 2)
                {
                    throw Oops.Oh("目标库位已被占用！");
                }
                var selectData = _Storage.AsQueryable()
                     .Where(x => x.StorageGroup == storageGroup.StorageGroup && x.StorageOccupy == 1)
                     .OrderBy(x => x.StorageNum, OrderByType.Asc)
                     .Select(x => x.StorageNum)
                     .ToList();
                if (storageGroup.StorageNum.ToInt() > selectData[0].ToInt())
                {
                    throw Oops.Oh("当前这个库位，入库时有库位阻挡，请重新选择库位！");
                }
            }
            endpoint = input.EndPoint;

            // 任务点集
            var positions = startpoint + "," + endpoint;

            TaskEntity taskEntity = input.Adapt<TaskEntity>();
            taskEntity.TaskPath = positions;
            taskEntity.InAndOutBoundNum = joinboundnum;

            // 下达agv任务

            DHMessage item = await taskService.AddAsync(taskEntity);

            // 下达agv任务成功
            if (item.code == 1000)
            {
                await InAndOutBoundMessage.ProcessInbound(input, joinboundnum);
                return "下发AGV入库任务成功！";
            }
            else
            {
                throw Oops.Oh("下发AGV入库任务失败！");
            }
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ex.Message);
        }

    }

    #endregion

    #region 潜伏举升AGV出库（出库WMS自动推荐库位）（宇翔项目）

    /// <summary>
    /// 潜伏举升AGV出库（出库WMS自动推荐库位）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AgvOutBoundTask", Order = 98)]
    public async Task<string> AgvOutBoundTask(AgvBoundDto input)
    {
        // 生成当前时间时间戳
        string outboundnum = _TimeStamp.GetTheCurrentTimeTimeStamp("EGCK");

        try
        {
            // 目标点
            if (input.EndPoint == null || input.EndPoint == "")
            {
                throw Oops.Oh("目标点不能为空");
            }

            // 起始点
            string startpoint = "";
            if (input.StartPoint == null || input.StartPoint == "")
            {
                input.StartPoint = BaseService.AGVStrategyReturnRecommendStorageOutBoundJudgeTime(input.MaterielNum);

                if (input.StartPoint == "没有合适的库位")
                {
                    // 添加出库暂存任务
                    await InAndOutBoundMessage.NotBoundNotStorageAddStagingTask(input, outboundnum);
                    return "添加AGV出库暂存任务成功！";
                }
            }
            else
            {
                // 判断用户输入的是否符合逻辑
                var storageGroup = _Storage.GetFirst(x => x.StorageNum == input.StartPoint);
                if (storageGroup == null)
                {
                    throw Oops.Oh("没有查找到该起始点的库位编号！");
                }
                else if (storageGroup.StorageOccupy != 1)
                {
                    throw Oops.Oh("当前库位上不存在库存！");
                }
                var selectData = _Storage.AsQueryable()
                     .Where(x => x.StorageGroup == storageGroup.StorageGroup && x.StorageOccupy == 1)
                     .OrderBy(x => x.StorageNum, OrderByType.Asc)
                     .Select(x => x.StorageNum)
                     .ToList();
                if (storageGroup.StorageNum.ToInt() > selectData[0].ToInt())
                {
                    throw Oops.Oh("当前这个库位，出库时有库位阻挡，请重新选择库位！");
                }
            }
            startpoint = input.StartPoint;
            string endpoint = input.EndPoint;

            // 任务点集
            var positions = startpoint + "," + endpoint;
            TaskEntity taskEntity = input.Adapt<TaskEntity>();
            taskEntity.TaskPath = positions;
            taskEntity.InAndOutBoundNum = outboundnum;

            // 下达agv任务
            DHMessage item = await taskService.AddAsync(taskEntity);

            // 判断agv出库是否成功
            if (item.code == 1000)
            {
                var datatask = await _TaskEntity.GetFirstAsync(x => x.InAndOutBoundNum == outboundnum);

                await _Storage.AsUpdateable()
                          .AS("EG_WMS_Storage")
                          .SetColumns(it => new EG_WMS_Storage
                          {
                              StorageOccupy = 2,
                              TaskNo = datatask.TaskNo,
                              UpdateTime = DateTime.Now,
                          })
                          .Where(x => x.StorageNum == input.StartPoint)
                          .ExecuteCommandAsync();

                EG_WMS_InAndOutBound outbound = new EG_WMS_InAndOutBound
                {
                    InAndOutBoundNum = outboundnum,
                    InAndOutBoundType = 1,
                    InAndOutBoundTime = DateTime.Now,
                    InAndOutBoundUser = input.AddName,
                    InAndOutBoundRemake = input.InAndOutBoundRemake,
                    CreateTime = DateTime.Now,
                    StartPoint = startpoint,
                    EndPoint = endpoint,
                };

                var regionnum = InAndOutBoundMessage.GetStorageWhereRegion(startpoint);
                var whnum = InAndOutBoundMessage.GetRegionWhereWHNum(regionnum);

                EG_WMS_InAndOutBoundDetail outbounddetail = new EG_WMS_InAndOutBoundDetail()
                {
                    InAndOutBoundNum = outboundnum,
                    CreateTime = DateTime.Now,
                    WHNum = whnum,
                    MaterielNum = input.MaterielNum,
                    RegionNum = regionnum,
                    StorageNum = input.StartPoint,
                };

                await _rep.InsertAsync(outbound);
                await _InAndOutBoundDetail.InsertAsync(outbounddetail);

                // 得到这个库位上的库存信息（先修改临时表里面的信息）

                var tem_InventoryDetails = _InventoryDetailTem.AsQueryable()
                                    .InnerJoin<EG_WMS_Tem_Inventory>((a, b) => a.InventoryNum == b.InventoryNum)
                                    .Where((a, b) => a.StorageNum == startpoint && b.OutboundStatus == 0 && a.IsDelete == false && b.IsDelete == false)
                                    .ToList();

                int sumcount = 0;
                for (int i = 0; i < tem_InventoryDetails.Count; i++)
                {
                    await _InventoryTem.AsUpdateable()
                                    .SetColumns(it => new EG_WMS_Tem_Inventory
                                    {
                                        OutboundStatus = 1,
                                        UpdateTime = DateTime.Now,
                                        // 出库编号
                                        OutBoundNum = outboundnum,
                                    })
                                    .Where(x => x.InventoryNum == tem_InventoryDetails[i].InventoryNum)
                                    .ExecuteCommandAsync();

                    // 查询库存数量
                    var invCount = _InventoryTem.AsQueryable()
                                 .Where(it => it.InventoryNum == tem_InventoryDetails[i].InventoryNum)
                                 .ToList();

                    // 计算总数
                    sumcount += invCount[0].ICountAll;

                }
                // 改变出库状态
                await _rep.AsUpdateable()
                     .AS("EG_WMS_InAndOutBound")
                     .SetColumns(it => new EG_WMS_InAndOutBound
                     {
                         // 总数
                         InAndOutBoundCount = sumcount,
                         // 出库中
                         InAndOutBoundStatus = 5,
                     })
                     .Where(u => u.InAndOutBoundNum == outboundnum)
                     .ExecuteCommandAsync();

                return "下发AGV出库任务成功！";
            }
            else
            {
                throw Oops.Oh("下发AGV出库任务失败！");
            }
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ex.Message);
        }
    }

    #endregion

    #region AGV堆高车入库（入库WMS自动推荐库位）（立库）

    /// <summary>
    /// AGV堆高车入库（入库WMS自动推荐库位）（立库）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AgvJoinBoundTasksSetUpStoreHouse", Order = 44)]
    public async Task AgvJoinBoundTasksSetUpStoreHouse(AgvJoinDto input)
    {
        try
        {
            // 生成当前时间时间戳
            string joinboundnum = _TimeStamp.GetTheCurrentTimeTimeStamp("EGRK");
            // 起始点
            string startpoint = input.StartPoint;
            if (startpoint == null || input.EndPoint == "")
            {
                throw Oops.Oh("起始点不可为空");
            }
            string wbnum = "";
            int sumcount = 0;
            string endpoint = "";
            string endpoints = "";
            for (int i = 0; i < input.materielWorkBins.Count; i++)
            {
                // 目标点
                if (input.EndPoint == null || input.EndPoint == "")
                {
                    endpoint = BaseService.AGVStacKingHighCarsIntoReturnStorage();

                    if (endpoint == "当前没有合适的库位！")
                    {
                        throw Oops.Oh("当前没有合适的库位！");
                    }

                    // 生成入库单,只生成一次
                    if (i == 0)
                    {
                        //await InAndOutBoundMessage.WarehousingOperationTask(input, joinboundnum);
                    }
                }

                endpoints += "," + endpoint;
                // 任务点集
                var positions = startpoint + "," + endpoint;

                TaskEntity taskEntity = input.Adapt<TaskEntity>();
                taskEntity.TaskPath = positions;
                taskEntity.InAndOutBoundNum = joinboundnum;

                // 下达agv任务
                DHMessage item = await taskService.AddAsync(taskEntity);

                // 下达agv任务成功

                if (item.code == 1000)
                {
                    // 查询任务列表
                    var agvTask = _TaskEntity.AsQueryable()
                                 .Where(x => x.InAndOutBoundNum == joinboundnum)
                                 .ToList()
                                 .Last();

                    string inventorynum = $"{i}EGKC" + _TimeStamp.GetTheCurrentTimeTimeStamp();
                    string workbinnum = input.materielWorkBins[i].WorkBinNum;
                    string materienum = input.materielWorkBins[i].MaterielNum;
                    int productcount = input.materielWorkBins[i].ProductCount;
                    DateTime productiondate = input.materielWorkBins[i].ProductionDate;
                    string productionlot = input.materielWorkBins[i].ProductionLot;
                    // 将库位占用
                    await _Storage.AsUpdateable()
                              .SetColumns(it => new EG_WMS_Storage
                              {
                                  StorageOccupy = 2,
                                  TaskNo = agvTask.TaskNo,
                                  StorageProductionDate = productiondate,
                              })
                              .Where(x => x.StorageNum == endpoint)
                              .ExecuteCommandAsync();
                    // 计算总数
                    sumcount += productcount;
                    // 临时库存主表
                    EG_WMS_Tem_Inventory addInventory = new EG_WMS_Tem_Inventory()
                    {
                        InventoryNum = inventorynum,
                        MaterielNum = materienum,
                        ICountAll = productcount,
                        CreateTime = DateTime.Now,
                        InBoundNum = joinboundnum,
                        OutboundStatus = 0,
                        ProductionDate = productiondate,
                    };
                    string _regionnum = InAndOutBoundMessage.GetStorageWhereRegion(endpoint);
                    string _whnum = InAndOutBoundMessage.GetRegionWhereWHNum(_regionnum);
                    // 临时详细表
                    EG_WMS_Tem_InventoryDetail addInventoryDetail = new EG_WMS_Tem_InventoryDetail()
                    {
                        InventoryNum = inventorynum,
                        WorkBinNum = workbinnum,
                        ProductionLot = productionlot,
                        CreateTime = DateTime.Now,
                        StorageNum = endpoint,
                        RegionNum = _regionnum,
                        WHNum = _whnum,
                    };
                    // 料箱表 将料箱内容保存到料箱表中
                    EG_WMS_WorkBin addWorkBin = new EG_WMS_WorkBin()
                    {
                        WorkBinNum = workbinnum,
                        ProductCount = productcount,
                        ProductionLot = productionlot,
                        CreateTime = DateTime.Now,
                        ProductionDate = productiondate,
                        WorkBinStatus = 0,
                        MaterielNum = materienum,
                        StorageNum = endpoint,
                        InAndOutBoundNum = joinboundnum,
                    };
                    wbnum += workbinnum;
                    // 将数据保存到临时表中
                    await _InventoryTem.InsertAsync(addInventory);
                    await _InventoryDetailTem.InsertAsync(addInventoryDetail);
                    await _workbin.InsertOrUpdateAsync(addWorkBin);

                    // 是不是最后一条数据
                    if (i == input.materielWorkBins.Count - 1)
                    {
                        // 修改入库单里面的库位信息
                        await _rep.AsUpdateable()
                             .SetColumns(it => new EG_WMS_InAndOutBound
                             {
                                 EndPoint = endpoints,
                             })
                             .Where(x => x.InAndOutBoundNum == joinboundnum)
                             .ExecuteCommandAsync();

                        await _InAndOutBoundDetail.AsUpdateable()
                             .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                             {
                                 StorageNum = endpoints,
                                 RegionNum = _regionnum
                             })
                             .Where(x => x.InAndOutBoundNum == joinboundnum)
                             .ExecuteCommandAsync();

                        // 修改出入库表
                        //await InAndOutBoundMessage.UpdateInAndOutBoundTask(wbnum, sumcount, joinboundnum, _whnum, materienum);
                    }
                }
                else
                {
                    throw Oops.Oh("下达AGV任务失败");
                }
            }
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ex.Message);
        }

    }
    #endregion

    #region AGV堆高车出库（出库WMS自动推荐库位）（立库）

    /// <summary>
    /// AGV堆高车出库（出库WMS自动推荐库位）（立库）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AgvOutBoundTasksSetUpStoreHouse")]
    public async Task AgvOutBoundTasksSetUpStoreHouse(AgvHighBoundBO input)
    {
        if (input.EndPoint == null || input.EndPoint.Length == 0 || input.EndPoint == "")
        {
            throw Oops.Oh("目标点不能为空！");
        }
        // 生成当前时间时间戳
        string outboundnum = _TimeStamp.GetTheCurrentTimeTimeStamp("EGCK");
        List<string> result = new List<string>();
        // 系统推荐的库位
        result = BaseService.AGVStackingHighCarStorageOutBound(input.MaterielNum, input.Sumcount);
        string startpoints = string.Join(",", result);
        string wbnum = "";
        int sumcount = 0;
        try
        {
            for (int i = 0; i < result.Count; i++)
            {
                string startpoint = result[i];
                string endpoint = input.EndPoint;
                // 任务点集
                var positions = startpoint + "," + endpoint;
                TaskEntity taskEntity = input.Adapt<TaskEntity>();
                taskEntity.TaskPath = positions;
                taskEntity.InAndOutBoundNum = outboundnum;
                // 下达agv任务
                DHMessage item = await taskService.AddAsync(taskEntity);
                if (item.code == 1000)
                {
                    // 只生成一次出库单
                    if (i == 0)
                    {
                        // 生成出库单
                        EG_WMS_InAndOutBound outbound = new EG_WMS_InAndOutBound
                        {
                            InAndOutBoundNum = outboundnum,
                            InAndOutBoundType = 1,
                            InAndOutBoundTime = DateTime.Now,
                            InAndOutBoundUser = input.AddName,
                            InAndOutBoundRemake = input.InAndOutBoundRemake,
                            CreateTime = DateTime.Now,
                            StartPoint = startpoints,
                            InAndOutBoundStatus = 5,
                            EndPoint = endpoint,
                        };

                        // 查询库位编号所在的区域编号
                        string regionnum = InAndOutBoundMessage.GetStorageWhereRegion(result[0]);
                        string whnum = InAndOutBoundMessage.GetRegionWhereWHNum(regionnum);
                        // 生成出库详单
                        EG_WMS_InAndOutBoundDetail outbounddetail = new EG_WMS_InAndOutBoundDetail()
                        {
                            InAndOutBoundNum = outboundnum,
                            CreateTime = DateTime.Now,
                            MaterielNum = input.MaterielNum,
                            RegionNum = regionnum,
                            WHNum = whnum,
                            StorageNum = startpoints,
                        };

                        await _rep.InsertAsync(outbound);
                        await _InAndOutBoundDetail.InsertAsync(outbounddetail);
                    }
                    // 根据出库编号查询任务编号
                    var datatask = await _TaskEntity.GetFirstAsync(x => x.InAndOutBoundNum == outboundnum);

                    // 修改库位表中的状态为占用
                    await _Storage.AsUpdateable()
                              .AS("EG_WMS_Storage")
                              .SetColumns(it => new EG_WMS_Storage
                              {
                                  // 预占用
                                  StorageOccupy = 2,
                                  TaskNo = datatask.TaskNo,
                              })
                              .Where(x => x.StorageNum == result[i])
                              .ExecuteCommandAsync();

                    // 得到这个库位上的库存信息（先修改临时表里面的信息）

                    var tem_InventoryDetails = _InventoryDetailTem.AsQueryable()
                                        .InnerJoin<EG_WMS_Tem_Inventory>((a, b) => a.InventoryNum == b.InventoryNum)
                                        .Where((a, b) => a.StorageNum == startpoint && b.OutboundStatus == 0 && a.IsDelete == false && b.IsDelete == false)
                                        .ToList();

                    await _InventoryTem.AsUpdateable()
                                    .SetColumns(it => new EG_WMS_Tem_Inventory
                                    {
                                        OutboundStatus = 1,
                                        UpdateTime = DateTime.Now,
                                        // 出库编号
                                        OutBoundNum = outboundnum,
                                    })
                                    .Where(x => x.InventoryNum == tem_InventoryDetails[0].InventoryNum)
                                    .ExecuteCommandAsync();

                    // 查询库存数量
                    var invCount = _InventoryTem.AsQueryable()
                                 .Where(it => it.InventoryNum == tem_InventoryDetails[0].InventoryNum)
                                 .ToList();

                    // 计算总数
                    sumcount += invCount[0].ICountAll;
                    wbnum += "," + tem_InventoryDetails[0].WorkBinNum;
                    // 是不是最后一条数据
                    if (i == input.StartPoint.Length - 1)
                    {
                        // 修改出库详情表里面的料箱编号和物料编号
                        await _InAndOutBoundDetail.AsUpdateable()
                                             .AS("EG_WMS_InAndOutBoundDetail")
                                             .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                                             {
                                                 WorkBinNum = wbnum,
                                                 UpdateTime = DateTime.Now,
                                             })
                                             .Where(u => u.InAndOutBoundNum == outboundnum)
                                             .ExecuteCommandAsync();
                        // 改变出库状态
                        await _rep.AsUpdateable()
                             .AS("EG_WMS_InAndOutBound")
                             .SetColumns(it => new EG_WMS_InAndOutBound
                             {
                                 InAndOutBoundCount = sumcount,
                                 UpdateTime = DateTime.Now,
                             })
                             .Where(u => u.InAndOutBoundNum == outboundnum)
                             .ExecuteCommandAsync();
                    }
                }
            }
        }
        catch (Exception ex)
        {

            throw Oops.Oh("错误：" + ex.Message);
        }

    }


    #endregion

    #region AGV堆高车出库（人工选择库位出库）（立库）

    /// <summary>
    /// AGV堆高车出库（人工选择库位出库）（立库）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AgvOutBoundTasksSetUpManualSelectionStoreHouse")]
    public async Task AgvOutBoundTasksSetUpManualSelectionStoreHouse(ManualSelectionBO input)
    {
        // 校验
        if (input.StartPoint == null || input.StartPoint.Length == 0 || input.EndPoint == null || input.EndPoint == "")
        {
            throw Oops.Oh("起始的库位或目标的库位不能为空！");
        }
        // 生成当前时间时间戳
        string outboundnum = _TimeStamp.GetTheCurrentTimeTimeStamp("EGCK");
        string endpoint = input.EndPoint;
        // 将数组转换成字符串
        string startpoints = string.Join(",", input.StartPoint);
        try
        {
            string wbnum = "";
            int sumcount = 0;
            // 人工选择的库位一共有几个
            for (int i = 0; i < input.StartPoint.Length; i++)
            {
                string startpoint = input.StartPoint[i];
                // 任务点集
                string positions = startpoint + "," + endpoint;

                TaskEntity taskEntity = input.Adapt<TaskEntity>();
                taskEntity.TaskPath = positions;
                taskEntity.InAndOutBoundNum = outboundnum;
                // 下达agv任务

                DHMessage item = await taskService.AddAsync(taskEntity);
                if (item.code == 1000)
                {
                    // 只生成一次出库单
                    if (i == 0)
                    {
                        // 生成出库单
                        EG_WMS_InAndOutBound outbound = new EG_WMS_InAndOutBound
                        {
                            InAndOutBoundNum = outboundnum,
                            InAndOutBoundType = 1,
                            InAndOutBoundTime = DateTime.Now,
                            InAndOutBoundUser = input.AddName,
                            InAndOutBoundRemake = input.InAndOutBoundRemake,
                            CreateTime = DateTime.Now,
                            StartPoint = startpoints,
                            InAndOutBoundStatus = 5,
                            EndPoint = endpoint,
                        };

                        // 查询库位编号所在的区域编号
                        string regionnum = InAndOutBoundMessage.GetStorageWhereRegion(input.StartPoint[0]);
                        string whnum = InAndOutBoundMessage.GetRegionWhereWHNum(regionnum);
                        // 生成出库详单
                        EG_WMS_InAndOutBoundDetail outbounddetail = new EG_WMS_InAndOutBoundDetail()
                        {
                            InAndOutBoundNum = outboundnum,
                            CreateTime = DateTime.Now,
                            MaterielNum = input.MaterielNum,
                            RegionNum = regionnum,
                            WHNum = whnum,
                            StorageNum = startpoints,
                        };

                        await _rep.InsertAsync(outbound);
                        await _InAndOutBoundDetail.InsertAsync(outbounddetail);
                    }

                    // 根据出库编号查询任务编号
                    var datatask = await _TaskEntity.GetFirstAsync(x => x.InAndOutBoundNum == outboundnum);

                    // 修改库位表中的状态为占用
                    await _Storage.AsUpdateable()
                              .AS("EG_WMS_Storage")
                              .SetColumns(it => new EG_WMS_Storage
                              {
                                  // 预占用
                                  StorageOccupy = 2,
                                  TaskNo = datatask.TaskNo,
                              })
                              .Where(x => x.StorageNum == input.StartPoint[i])
                              .ExecuteCommandAsync();

                    // 得到这个库位上的库存信息（先修改临时表里面的信息）

                    var tem_InventoryDetails = _InventoryDetailTem.AsQueryable()
                                        .InnerJoin<EG_WMS_Tem_Inventory>((a, b) => a.InventoryNum == b.InventoryNum)
                                        .Where((a, b) => a.StorageNum == startpoint && b.OutboundStatus == 0 && a.IsDelete == false && b.IsDelete == false)
                                        .ToList();

                    await _InventoryTem.AsUpdateable()
                                    .SetColumns(it => new EG_WMS_Tem_Inventory
                                    {
                                        OutboundStatus = 1,
                                        UpdateTime = DateTime.Now,
                                        // 出库编号
                                        OutBoundNum = outboundnum,
                                    })
                                    .Where(x => x.InventoryNum == tem_InventoryDetails[0].InventoryNum)
                                    .ExecuteCommandAsync();

                    // 查询库存数量
                    var invCount = _InventoryTem.AsQueryable()
                                 .Where(it => it.InventoryNum == tem_InventoryDetails[0].InventoryNum)
                                 .ToList();

                    // 计算总数
                    sumcount += invCount[0].ICountAll;
                    wbnum += "," + tem_InventoryDetails[0].WorkBinNum;
                    // 是不是最后一条数据
                    if (i == input.StartPoint.Length - 1)
                    {
                        // 修改出库详情表里面的料箱编号和物料编号
                        await _InAndOutBoundDetail.AsUpdateable()
                                             .AS("EG_WMS_InAndOutBoundDetail")
                                             .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                                             {
                                                 WorkBinNum = wbnum,
                                                 UpdateTime = DateTime.Now,
                                             })
                                             .Where(u => u.InAndOutBoundNum == outboundnum)
                                             .ExecuteCommandAsync();
                        // 改变出库状态
                        await _rep.AsUpdateable()
                             .AS("EG_WMS_InAndOutBound")
                             .SetColumns(it => new EG_WMS_InAndOutBound
                             {
                                 InAndOutBoundCount = sumcount,
                                 UpdateTime = DateTime.Now,
                             })
                             .Where(u => u.InAndOutBoundNum == outboundnum)
                             .ExecuteCommandAsync();
                    }
                }
            }
        }
        catch (Exception ex)
        {

            throw Oops.Oh("错误：" + ex.Message);
        }

    }


    #endregion

    #region 人工入库（密集库）
    /// <summary>
    /// 人工入库（密集库）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>

    [HttpPost]
    [ApiDescriptionSettings(Name = "ArtificialJoinBoundAdd", Order = 96)]
    public async Task<string> ArtificialJoinBoundAdd(EGInBoundDto input)
    {
        // 生成当前时间时间戳
        string joinboundnum = _TimeStamp.GetTheCurrentTimeTimeStamp("EGRK");

        try
        {
            // 判断用户输入的库位编号是否合法
            var isvstorage = _Storage.GetFirst(x => x.StorageNum == input.EndPoint) ?? throw Oops.Oh("没有查找到该库位编号！");
            if (isvstorage.StorageOccupy == 1 || isvstorage.StorageOccupy == 2)
            {
                throw Oops.Oh("目标库位已被占用！");
            }
            // 得到区域和仓库编号
            string regionnum = InAndOutBoundMessage.GetStorageWhereRegion(input.EndPoint) ?? throw Oops.Oh("当前库位查询不到区域编号！");
            string whnum = InAndOutBoundMessage.GetRegionWhereWHNum(regionnum) ?? throw Oops.Oh("当前区域查询不到仓库编号！");

            // 得到这个区域绑定的是哪种物料编号
            EG_WMS_Region regiondata = await _Region.GetFirstAsync(x => x.RegionNum == regionnum);
            if (!(input.materielWorkBins[0].MaterielNum == regiondata.RegionMaterielNum))
            {
                throw Oops.Oh("当前选择库位的区域不能存放该种物料！");
            }

            EG_WMS_InAndOutBound inandoutbound = new EG_WMS_InAndOutBound
            {
                InAndOutBoundNum = joinboundnum,
                InAndOutBoundType = 0,
                InAndOutBoundTime = DateTime.Now,
                InAndOutBoundUser = input.InAndOutBoundUser,
                InAndOutBoundRemake = input.InAndOutBoundRemake,
                EndPoint = input.EndPoint,
            };
            EG_WMS_InAndOutBoundDetail inandoutbounddetail = new EG_WMS_InAndOutBoundDetail
            {
                InAndOutBoundNum = joinboundnum,
                StorageNum = input.EndPoint,
                RegionNum = regionnum,
                WHNum = whnum,
                CreateTime = DateTime.Now,
            };

            // 保存到数据库中
            await _rep.InsertAsync(inandoutbound);
            await _InAndOutBoundDetail.InsertAsync(inandoutbounddetail);

            // 得到入库的数据
            List<MaterielWorkBin> item = input.materielWorkBins;
            List<DateTime> datetime = new List<DateTime>();
            // 将料箱的生产日期保存
            for (int i = 0; i < item.Count; i++)
            {
                datetime.Add(item[i].ProductionDate);
            }

            // 循环遍历，一共有多少个需要入库的料箱
            string wbnum = "";
            int sumcount = 0;
            for (int i = 0; i < item.Count; i++)
            {
                var idone = SnowFlakeSingle.instance.NextId();
                var idtwo = SnowFlakeSingle.instance.NextId();
                string inventorynum = $"{i}EGKC" + _TimeStamp.GetTheCurrentTimeTimeStamp();
                string workbinnum = item[i].WorkBinNum;
                string materienum = item[i].MaterielNum;
                int productcount = item[i].ProductCount;
                DateTime productiondate = item[i].ProductionDate;
                string productionlot = item[i].ProductionLot;
                // 总数
                sumcount += productcount;
                // 库存主表
                EG_WMS_Inventory addInventory = new EG_WMS_Inventory()
                {
                    Id = idone,
                    InventoryNum = inventorynum,
                    MaterielNum = materienum,
                    ICountAll = productcount,
                    CreateTime = DateTime.Now,
                    InBoundNum = joinboundnum,
                    IsDelete = false,
                    OutboundStatus = 0,
                    ProductionDate = productiondate,
                };
                // 详细表
                EG_WMS_InventoryDetail addInventoryDetail = new EG_WMS_InventoryDetail()
                {
                    Id = idtwo,
                    InventoryNum = inventorynum,
                    WorkBinNum = workbinnum,
                    ProductionLot = productionlot,
                    CreateTime = DateTime.Now,
                    StorageNum = input.EndPoint,
                    RegionNum = regionnum,
                    WHNum = whnum,
                    IsDelete = false,
                };
                // 料箱表 将料箱内容保存到料箱表中（生成新料箱或修改）
                EG_WMS_WorkBin addWorkBin = new EG_WMS_WorkBin()
                {
                    WorkBinNum = workbinnum,
                    ProductCount = productcount,
                    ProductionLot = productionlot,
                    CreateTime = DateTime.Now,
                    ProductionDate = productiondate,
                    WorkBinStatus = 0,
                    MaterielNum = materienum,
                    StorageNum = input.EndPoint,
                };
                // 临时表
                EG_WMS_Tem_Inventory tem_Inventory = new EG_WMS_Tem_Inventory();
                EG_WMS_Tem_InventoryDetail tem_InventoryDetail = new EG_WMS_Tem_InventoryDetail();

                var teminv = addInventory.Adapt(tem_Inventory);
                var teminvd = addInventoryDetail.Adapt(tem_InventoryDetail);

                // 临时表
                await _InventoryTem.InsertAsync(teminv);
                await _InventoryDetailTem.InsertAsync(teminvd);

                await _Inventory.InsertAsync(addInventory);
                await _InventoryDetail.InsertAsync(addInventoryDetail);
                await _workbin.InsertOrUpdateAsync(addWorkBin);

                // 得到每个料箱编号
                if (item.Count > 1)
                {
                    wbnum = workbinnum + "," + wbnum;
                }
                else
                {
                    wbnum = workbinnum;
                }
            }

            // 修改入库详情表里面的料箱编号

            await _InAndOutBoundDetail.AsUpdateable()
                                .AS("EG_WMS_InAndOutBoundDetail")
                                .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                                {
                                    // 只会有一种物料
                                    MaterielNum = item[0].MaterielNum,
                                    WorkBinNum = wbnum,
                                    UpdateTime = DateTime.Now
                                })
                                .Where(u => u.InAndOutBoundNum == joinboundnum)
                                .ExecuteCommandAsync();

            // 改变入库状态
            await _rep.AsUpdateable()
                  .AS("EG_WMS_InAndOutBound")
                  .SetColumns(it => new EG_WMS_InAndOutBound
                  {
                      // 总数
                      InAndOutBoundCount = sumcount,
                      InAndOutBoundStatus = 1,
                      SuccessOrNot = 0,
                      UpdateTime = DateTime.Now
                  })
                  .Where(u => u.InAndOutBoundNum == joinboundnum)
                  .ExecuteCommandAsync();


            // 修改库位表中的状态为占用
            await _Storage.AsUpdateable()
                      .AS("EG_WMS_Storage")
                      .SetColumns(it => new EG_WMS_Storage
                      {
                          // 占用
                          StorageOccupy = 1,
                          // 得到日期最大的生产日期
                          StorageProductionDate = datetime.Max(),
                          UpdateTime = DateTime.Now
                      })
                      .Where(x => x.StorageNum == input.EndPoint)
                      .ExecuteCommandAsync();

            return "人工入库成功！";
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ex.Message);
        }

    }



    #endregion

    #region 人工出库（密集库）（人工扫描库位库位上所有的料箱都出）

    /// <summary>
    /// 人工出库（密集库）（人工扫描库位库位上所有的料箱都出）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>

    [HttpPost]
    [ApiDescriptionSettings(Name = "ArtificialOutBoundAdd", Order = 95)]
    public async Task<string> ArtificialOutBoundAdd(EGOutBoundDto input)
    {
        // 生成当前时间时间戳
        string outboundnum = _TimeStamp.GetTheCurrentTimeTimeStamp("EGCK");

        try
        {
            // 人拿着pda扫
            // 扫库位，得到这个库位上所有的料箱，所有的料箱全部都出
            // 得到pda扫到的库位上所有的料箱（根据库位得到这个库位上所有的料箱(从库存中得到)）

            // 判断用户输入的库位是否合法
            var isvstorage = _Storage.GetFirst(x => x.StorageNum == input.StorageNum) ?? throw Oops.Oh("没有查找到该库位编号！");
            if (isvstorage.StorageOccupy != 1)
            {
                throw Oops.Oh("当前库位上不存在库存！");
            }
            string regionnum = InAndOutBoundMessage.GetStorageWhereRegion(input.StorageNum) ?? throw Oops.Oh("当前库位查询不到区域编号！");
            string whnum = InAndOutBoundMessage.GetRegionWhereWHNum(regionnum) ?? throw Oops.Oh("当前区域查询不到仓库编号！");

            // 1.得到这个库位上所有的数据
            var dataList = _Inventory.AsQueryable()
                                        .InnerJoin<EG_WMS_InventoryDetail>((a, b) => a.InventoryNum == b.InventoryNum)
                                        .Where((a, b) => b.StorageNum == input.StorageNum && a.OutboundStatus == 0)
                                        .Select((a, b) => new
                                        {
                                            a.MaterielNum,
                                            a.InventoryNum,
                                            b.StorageNum,
                                            b.WorkBinNum
                                        })
                                        .ToList();

            if (dataList.Count == 0)
            {
                throw Oops.Oh("此库位上没有数据");
            }

            // 2.得到这个库位上所有的库存
            // 所有料箱的总数
            int? sumcount = 0;
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    EG_WMS_InAndOutBound inandoutbound = new EG_WMS_InAndOutBound
                    {
                        InAndOutBoundNum = outboundnum,
                        InAndOutBoundType = 1,
                        InAndOutBoundTime = DateTime.Now,
                        InAndOutBoundRemake = input.Remake,
                        CreateTime = DateTime.Now,
                        InAndOutBoundUser = input.OutBoundUser,
                        StartPoint = input.StorageNum
                    };
                    EG_WMS_InAndOutBoundDetail inandoutbounddetail = new EG_WMS_InAndOutBoundDetail
                    {
                        InAndOutBoundNum = outboundnum,
                        StorageNum = input.StorageNum,
                        RegionNum = regionnum,
                        WHNum = whnum,
                        CreateTime = DateTime.Now,
                    };

                    // 保存到数据库中
                    await _rep.InsertAsync(inandoutbound);
                    await _InAndOutBoundDetail.InsertAsync(inandoutbounddetail);

                    string wbnum = "";
                    for (int i = 0; i < dataList.Count; i++)
                    {
                        // 得到库存表中的每个物料和料箱编号
                        string workbinnum = dataList[i].WorkBinNum;
                        string materienum = dataList[i].MaterielNum;
                        if (dataList.Count > 1)
                        {
                            wbnum += workbinnum + ",";
                        }
                        else
                        {
                            wbnum = workbinnum;
                        }
                        // 得到每一个料箱里面的数量
                        var _incountall = _Inventory.AsQueryable()
                                   .Where(u => u.InventoryNum == dataList[i].InventoryNum)
                                   .Select(f => new
                                   {
                                       f.ICountAll
                                   })
                                   .ToList();

                        sumcount += _incountall[0].ICountAll;

                        // 并且将临时库存表中的数据状态也修改
                        await _InventoryTem.AsUpdateable()
                                           .SetColumns(it => new EG_WMS_Tem_Inventory
                                           {
                                               OutboundStatus = 1,
                                               OutBoundNum = outboundnum,
                                               UpdateTime = DateTime.Now,
                                           })
                                           .Where(it => it.InventoryNum == dataList[i].InventoryNum)
                                           .ExecuteCommandAsync();

                        // 将库存表中的出库状态改变
                        await _Inventory.AsUpdateable()
                                   .AS("EG_WMS_Inventory")
                                   .SetColumns(u => new EG_WMS_Inventory
                                   {
                                       OutboundStatus = 1,
                                       OutBoundNum = outboundnum,
                                       UpdateTime = DateTime.Now
                                   })
                                   .Where(it => it.InventoryNum == dataList[i].InventoryNum)
                                   .ExecuteCommandAsync();
                    }
                    // 修改出入库主表
                    await _rep.AsUpdateable()
                         .AS("EG_WMS_InAndOutBound")
                         .SetColumns(u => new EG_WMS_InAndOutBound
                         {
                             InAndOutBoundStatus = 3,
                             // 出入库数量
                             InAndOutBoundCount = sumcount,
                             SuccessOrNot = 0,
                             UpdateTime = DateTime.Now
                         })
                         .Where(it => it.InAndOutBoundNum == outboundnum)
                         .ExecuteCommandAsync();
                    // 修改出入库详情表
                    await _InAndOutBoundDetail.AsUpdateable()
                                              .AS("EG_WMS_InAndOutBoundDetail")
                                              .SetColumns(u => new EG_WMS_InAndOutBoundDetail
                                              {
                                                  MaterielNum = dataList[0].MaterielNum,
                                                  WorkBinNum = wbnum,
                                                  UpdateTime = DateTime.Now
                                              })
                                              .ExecuteCommandAsync();
                    // 修改库位表中的信息
                    await _Storage.AsUpdateable()
                                  .AS("EG_WMS_Storage")
                                  .SetColumns(it => new Entity.EG_WMS_Storage
                                  {
                                      // 未占用
                                      StorageOccupy = 0,
                                      TaskNo = null,
                                      StorageProductionDate = null,
                                      UpdateTime = DateTime.Now
                                  })
                                  .Where(x => x.StorageNum == input.StorageNum)
                                  .ExecuteCommandAsync();
                    scope.Complete();
                    return "人工出库成功！";
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    throw Oops.Oh("错误：" + ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ex.Message);
        }
    }


    #endregion

    #region 分页查询出入库信息（成功）
    /// <summary>
    /// 分页查询出入库信息（成功）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page", Order = 30)]
    public async Task<SqlSugarPagedList<EG_WMS_InAndOutBoundOutput>> Page(EG_WMS_InAndOutBoundInput input)
    {
        var query = _rep.AsQueryable()
                        .InnerJoin<EG_WMS_InAndOutBoundDetail>((a, b) => a.InAndOutBoundNum == b.InAndOutBoundNum)
                        .InnerJoin<EG_WMS_Materiel>((a, b, c) => b.MaterielNum == c.MaterielNum)
                        .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielName), (a, b, c) => c.MaterielName.Contains(input.MaterielName.Trim()))
                        .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), (a, b) => b.MaterielNum == input.MaterielNum)
                        .WhereIF(!string.IsNullOrWhiteSpace(input.InAndOutBoundNum), (a, b) => a.InAndOutBoundNum.Contains(input.InAndOutBoundNum.Trim()))
                        .WhereIF(input.InAndOutBoundStatus >= 0, (a, b) => a.InAndOutBoundStatus == input.InAndOutBoundStatus)
                        .WhereIF(!string.IsNullOrWhiteSpace(input.InAndOutBoundUser), (a, b) => a.InAndOutBoundUser.Contains(input.InAndOutBoundUser.Trim()))
                        .WhereIF(input.InAndOutBoundType >= 0, (a, b) => a.InAndOutBoundType == input.InAndOutBoundType)
                        .Where(a => a.IsDelete == false && a.SuccessOrNot == 0)
                        // 倒序
                        .OrderBy(a => a.CreateTime, OrderByType.Desc)
                        .Select<EG_WMS_InAndOutBoundOutput>();

        // 日期查询
        if (input.InAndOutBoundTimeRange != null && input.InAndOutBoundTimeRange.Count > 0)
        {
            DateTime? start = input.InAndOutBoundTimeRange[0];
            query = query.WhereIF(start.HasValue, a => a.InAndOutBoundTime > start);
            if (input.InAndOutBoundTimeRange.Count > 1 && input.InAndOutBoundTimeRange[1].HasValue)
            {
                var end = input.InAndOutBoundTimeRange[1].Value.AddDays(1);
                query = query.Where(a => a.InAndOutBoundTime < end);
            }
        }
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }
    #endregion

    #region 分页查询出入库信息（失败）
    /// <summary>
    /// 分页查询出入库信息（失败）
    /// </summary> 
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "PageFail", Order = 29)]
    public async Task<SqlSugarPagedList<EG_WMS_InAndOutBoundOutputAGV>> PageFail(EG_WMS_InAndOutBoundInput input)
    {
        var query = _rep.AsQueryable()
                        .InnerJoin<EG_WMS_InAndOutBoundDetail>((a, b) => a.InAndOutBoundNum == b.InAndOutBoundNum)
                        .InnerJoin<EG_WMS_Materiel>((a, b, c) => b.MaterielNum == c.MaterielNum)
                        .InnerJoin<TaskEntity>((a, b, c, d) => a.InAndOutBoundNum == d.InAndOutBoundNum)
                        .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielName), (a, b, c) => c.MaterielName.Contains(input.MaterielName.Trim()))
                        .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), (a, b) => b.MaterielNum == input.MaterielNum)
                        .WhereIF(!string.IsNullOrWhiteSpace(input.InAndOutBoundNum), (a, b) => a.InAndOutBoundNum.Contains(input.InAndOutBoundNum.Trim()))
                        .WhereIF(input.InAndOutBoundStatus >= 0, (a, b) => a.InAndOutBoundStatus == input.InAndOutBoundStatus)
                        .WhereIF(input.InAndOutBoundType >= 0, (a, b) => a.InAndOutBoundType == input.InAndOutBoundType)
                        .WhereIF(!string.IsNullOrWhiteSpace(input.InAndOutBoundUser), (a, b) => a.InAndOutBoundUser.Contains(input.InAndOutBoundUser.Trim()))
                    // 倒序
                    .Where(a => a.IsDelete == false && a.SuccessOrNot == 1)
                    .OrderBy(a => a.CreateTime, OrderByType.Desc)
                    .Select((a, b, c, d) => new EG_WMS_InAndOutBoundOutputAGV
                    {

                    }, true);

        // 日期查询
        if (input.InAndOutBoundTimeRange != null && input.InAndOutBoundTimeRange.Count > 0)
        {
            DateTime? start = input.InAndOutBoundTimeRange[0];
            query = query.WhereIF(start.HasValue, a => a.InAndOutBoundTime > start);
            if (input.InAndOutBoundTimeRange.Count > 1 && input.InAndOutBoundTimeRange[1].HasValue)
            {
                var end = input.InAndOutBoundTimeRange[1].Value.AddDays(1);
                query = query.Where(a => a.InAndOutBoundTime < end);
            }
        }
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }
    #endregion

    #region 获取出入库信息
    /// <summary>
    /// 获取出入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail", Order = 20)]
    public async Task<EG_WMS_InAndOutBound> Get([FromQuery] QueryByIdEG_WMS_InAndOutBoundInput input)
    {
        return await _rep.GetFirstAsync(u => u.Id == input.Id);
    }
    #endregion

    #region 获取出入库信息列表
    /// <summary>
    /// 获取出入库信息列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List", Order = 15)]
    public async Task<List<EG_WMS_InAndOutBoundOutput>> List([FromQuery] EG_WMS_InAndOutBoundInput input)
    {
        return await _rep.AsQueryable().Select<EG_WMS_InAndOutBoundOutput>().ToListAsync();
    }
    #endregion

    #region 得到正在进行中的任务列表

    /// <summary>
    /// 得到正在进行中的任务列表
    /// </summary>
    /// <returns></returns>
    [HttpGet(Name = "GetInoutBoundAndAGVInforMation")]
    public List<InoutBoundAndAGVInforMationDto> GetInoutBoundAndAGVInforMation()
    {
        return _rep.AsQueryable()
              .InnerJoin<TaskEntity>((inoutbound, agvtask) => inoutbound.InAndOutBoundNum == agvtask.InAndOutBoundNum)
              .InnerJoin<EG_WMS_InAndOutBoundDetail>(((inoutbound, agvtask, inoutbounddetail) => inoutbound.InAndOutBoundNum == inoutbounddetail.InAndOutBoundNum))
              .Where(inoutbound => inoutbound.InAndOutBoundStatus == 4 || inoutbound.InAndOutBoundStatus == 5)
              .Select<InoutBoundAndAGVInforMationDto>()
              .ToList();

    }


    #endregion

    //-------------------------------------//-------------------------------------// 
}
