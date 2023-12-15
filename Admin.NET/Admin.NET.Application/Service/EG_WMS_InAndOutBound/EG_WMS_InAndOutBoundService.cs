namespace Admin.NET.Application;

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


    #endregion

    #region 构造函数
    public EG_WMS_InAndOutBoundService()
    {

    }
    #endregion

    #region  AGV入库（入库WMS自动推荐库位）（密集库）（用于测试）（潜伏举升）

    /// <summary>
    /// AGV入库（入库WMS自动推荐库位）（密集库）（用于测试）（潜伏举升）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AgvJoinBoundTask", Order = 50)]
    public async Task AgvJoinBoundTask(AgvJoinDto input)
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

            string endpoint = "";
            // 判断用户输入的是否符合逻辑
            var storageGroup = _Storage.GetFirstAsync(x => x.StorageNum == input.EndPoint);
            var selectData = _Storage.AsQueryable()
                 .Where(x => x.StorageGroup == storageGroup.Result.StorageGroup && x.StorageOccupy == 1)
                 .OrderBy(x => x.StorageNum, OrderByType.Asc)
                 .Select(x => x.StorageNum)
                 .ToList();
            if (storageGroup.Result.StorageNum.ToInt() > selectData[0].ToInt())
            {
                throw Oops.Oh("当前选择的这个库位，入库时有库位阻挡，请重新选择库位！");
            }
            else
            {
                endpoint = input.EndPoint;
            }

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
                        List<MaterielWorkBin> list = input.materielWorkBins;
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
                            // 编号
                            InAndOutBoundNum = joinboundnum,
                            // 出入库类型（入库还是出库）
                            InAndOutBoundType = 0,
                            // 时间
                            InAndOutBoundTime = DateTime.Now,
                            // 操作人
                            InAndOutBoundUser = input.AddName,
                            // 备注
                            InAndOutBoundRemake = input.InAndOutBoundRemake,
                            // 创建时间
                            CreateTime = DateTime.Now,
                            // 起始点
                            StartPoint = input.StartPoint,
                            // 目标点
                            EndPoint = input.EndPoint,
                        };

                        // 查询库位编号所在的区域编号

                        var _storagelistdata = await _Storage.GetFirstAsync(u => u.StorageNum == input.EndPoint);


                        if (_storagelistdata == null || string.IsNullOrEmpty(_storagelistdata.RegionNum))
                        {
                            throw Oops.Oh("没有查询到这个库位编号所在的区域编号");
                        }

                        string regionnum = _storagelistdata.RegionNum;

                        // 通过查询出来的区域得到仓库编号

                        var _regionlistdata = await _Region.GetFirstAsync(x => x.RegionNum == regionnum);

                        if (_regionlistdata == null || string.IsNullOrEmpty(_regionlistdata.WHNum))
                        {
                            throw Oops.Oh("没有查询到这个仓库");
                        }

                        // 生成入库详单

                        EG_WMS_InAndOutBoundDetail joinbounddetail = new EG_WMS_InAndOutBoundDetail()
                        {
                            // 出入库编号
                            InAndOutBoundNum = joinboundnum,
                            CreateTime = DateTime.Now,
                            // 区域编号
                            RegionNum = _storagelistdata.RegionNum,
                            // 目标点就是存储的点位即库位编号
                            StorageNum = input.EndPoint,
                        };
                        #endregion

                        await _rep.InsertAsync(joinbound);
                        await _InAndOutBoundDetail.InsertAsync(joinbounddetail);


                        string wbnum = "";
                        int sumcount = 0;
                        for (int i = 0; i < list.Count; i++)
                        {
                            // 库存编号（主表和详细表）
                            string inventorynum = $"{i}EGKC" + _TimeStamp.GetTheCurrentTimeTimeStamp();
                            // 料箱编号（详细表、料箱表）
                            string workbinnum = list[i].WorkBinNum;
                            // 物料编号（主表）
                            string materienum = list[i].MaterielNum;
                            // 物料的数量（主表、料箱表）
                            int productcount = list[i].ProductCount;
                            // 生产日期（料箱表）
                            DateTime productiondate = list[i].ProductionDate;
                            // 生产批次（详细表、料箱表）
                            string productionlot = list[i].ProductionLot;

                            // 总数
                            sumcount += productcount;

                            // 将得到的数据，保存在临时的库存主表和详细表中

                            // 临时库存主表
                            EG_WMS_Tem_Inventory addInventory = new EG_WMS_Tem_Inventory()
                            {
                                // 雪花id
                                //Id = idone,
                                // 库存编号
                                InventoryNum = inventorynum,
                                // 物料编号
                                MaterielNum = materienum,
                                // 库存总数
                                ICountAll = productcount,
                                // 创建时间
                                CreateTime = DateTime.Now,
                                // 入库编号
                                InBoundNum = joinboundnum,
                                // 是否删除
                                IsDelete = false,
                                // 是否出库
                                OutboundStatus = 0,
                            };

                            // 临时详细表
                            EG_WMS_Tem_InventoryDetail addInventoryDetail = new EG_WMS_Tem_InventoryDetail()
                            {
                                // 雪花id
                                //Id = idtwo,
                                // 库存编号
                                InventoryNum = inventorynum,
                                // 料箱编号
                                WorkBinNum = workbinnum,
                                // 生产批次
                                ProductionLot = productionlot,
                                // 创建时间
                                CreateTime = DateTime.Now,
                                // 库位编号
                                StorageNum = input.EndPoint,
                                // 区域编号
                                RegionNum = _storagelistdata.RegionNum,
                                // 仓库编号
                                WHNum = _regionlistdata.WHNum,
                                // 是否删除
                                IsDelete = false,

                            };

                            // 料箱表 将料箱内容保存到料箱表中
                            EG_WMS_WorkBin addWorkBin = new EG_WMS_WorkBin()
                            {
                                // 编号
                                WorkBinNum = workbinnum,
                                // 产品数量
                                ProductCount = productcount,
                                // 生产批次
                                ProductionLot = productionlot,
                                CreateTime = DateTime.Now,
                                // 生产日期
                                ProductionDate = productiondate,
                                WorkBinStatus = 0,
                                MaterielNum = materienum,
                                // 库位编号
                                StorageNum = input.EndPoint,
                                // 出入库编号
                                InAndOutBoundNum = joinboundnum,
                            };

                            // 将数据保存到临时表中
                            await _InventoryTem.InsertAsync(addInventory);
                            await _InventoryDetailTem.InsertAsync(addInventoryDetail);
                            await _workbin.InsertOrUpdateAsync(addWorkBin);


                            // 得到每个料箱编号
                            if (list.Count > 1)
                            {
                                wbnum = workbinnum + "," + wbnum;
                            }
                            else
                            {
                                wbnum = workbinnum;
                            }
                        }
                        // 修改入库详情表里面的料箱编号和物料编号

                        await _InAndOutBoundDetail.AsUpdateable()
                                             .AS("EG_WMS_InAndOutBoundDetail")
                                             .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                                             {
                                                 WHNum = _regionlistdata.WHNum,
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

    #region AGV入库（需要前往等待点）（密集库）（潜伏举升）

    /// <summary>
    ///  AGV入库（需要前往等待点，到达等待点再获取库位点）（密集库）（潜伏举升）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AgvJoinBoundTaskGOPoint")]
    public async Task AgvJoinBoundTaskGOPoint(AgvJoinDto input)
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
                        List<MaterielWorkBin> list = input.materielWorkBins;

                        #region 入库操作

                        // 生成入库单
                        EG_WMS_InAndOutBound joinbound = new EG_WMS_InAndOutBound
                        {
                            // 编号
                            InAndOutBoundNum = joinboundnum,
                            // 出入库类型（入库还是出库）
                            InAndOutBoundType = 0,
                            // 时间
                            InAndOutBoundTime = DateTime.Now,
                            // 操作人
                            InAndOutBoundUser = input.AddName,
                            // 备注
                            InAndOutBoundRemake = input.InAndOutBoundRemake,
                            // 创建时间
                            CreateTime = DateTime.Now,
                            // 起始点
                            StartPoint = input.StartPoint,
                            // 目标点
                            EndPoint = "AGV正在前往等待点，存放的库位等AGV到达等待点后自动获取",
                        };

                        // 查询库位编号所在的区域编号

                        var _storagelistdata = await _Storage.GetFirstAsync(u => u.StorageNum == input.EndPoint);


                        if (_storagelistdata == null || string.IsNullOrEmpty(_storagelistdata.RegionNum))
                        {
                            throw Oops.Oh("没有查询到这个库位编号所在的区域编号");
                        }

                        string regionnum = _storagelistdata.RegionNum;

                        // 通过查询出来的区域得到仓库编号

                        var _regionlistdata = await _Region.GetFirstAsync(x => x.RegionNum == regionnum);

                        if (_regionlistdata == null || string.IsNullOrEmpty(_regionlistdata.WHNum))
                        {
                            throw Oops.Oh("没有查询到这个仓库");
                        }

                        // 生成入库详单

                        EG_WMS_InAndOutBoundDetail joinbounddetail = new EG_WMS_InAndOutBoundDetail()
                        {
                            // 出入库编号
                            InAndOutBoundNum = joinboundnum,
                            CreateTime = DateTime.Now,
                            // 区域编号
                            RegionNum = _storagelistdata.RegionNum,
                            // 目标点就是存储的点位即库位编号
                            StorageNum = "AGV正在前往等待点，存放的库位等AGV到达等待点后自动获取",
                        };
                        #endregion

                        await _rep.InsertAsync(joinbound);
                        await _InAndOutBoundDetail.InsertAsync(joinbounddetail);


                        string wbnum = "";
                        int sumcount = 0;
                        for (int i = 0; i < list.Count; i++)
                        {
                            // 库存编号（主表和详细表）
                            string inventorynum = $"{i}EGKC" + _TimeStamp.GetTheCurrentTimeTimeStamp();
                            // 料箱编号（详细表、料箱表）
                            string workbinnum = list[i].WorkBinNum;
                            // 物料编号（主表）
                            string materienum = list[i].MaterielNum;
                            // 物料的数量（主表、料箱表）
                            int productcount = list[i].ProductCount;
                            // 生产日期（料箱表）
                            DateTime productiondate = list[i].ProductionDate;
                            // 生产批次（详细表、料箱表）
                            string productionlot = list[i].ProductionLot;

                            // 总数
                            sumcount += productcount;

                            // 将得到的数据，保存在临时的库存主表和详细表中

                            // 临时库存主表
                            EG_WMS_Tem_Inventory addInventory = new EG_WMS_Tem_Inventory()
                            {
                                // 库存编号
                                InventoryNum = inventorynum,
                                // 物料编号
                                MaterielNum = materienum,
                                // 库存总数
                                ICountAll = productcount,
                                // 创建时间
                                CreateTime = DateTime.Now,
                                // 入库编号
                                InBoundNum = joinboundnum,
                                // 是否删除
                                IsDelete = false,
                                // 是否出库
                                OutboundStatus = 0,
                            };

                            // 临时详细表
                            EG_WMS_Tem_InventoryDetail addInventoryDetail = new EG_WMS_Tem_InventoryDetail()
                            {
                                // 库存编号
                                InventoryNum = inventorynum,
                                // 料箱编号
                                WorkBinNum = workbinnum,
                                // 生产批次
                                ProductionLot = productionlot,
                                // 创建时间
                                CreateTime = DateTime.Now,
                                // 库位编号
                                StorageNum = "AGV正在前往等待点，存放的库位等AGV到达等待点后自动获取",
                                // 区域编号
                                RegionNum = _storagelistdata.RegionNum,
                                // 仓库编号
                                WHNum = _regionlistdata.WHNum,
                                // 是否删除
                                IsDelete = false,

                            };

                            // 料箱表 将料箱内容保存到料箱表中
                            EG_WMS_WorkBin addWorkBin = new EG_WMS_WorkBin()
                            {
                                // 编号
                                WorkBinNum = workbinnum,
                                // 产品数量
                                ProductCount = productcount,
                                // 生产批次
                                ProductionLot = productionlot,
                                CreateTime = DateTime.Now,
                                // 生产日期
                                ProductionDate = productiondate,
                                WorkBinStatus = 0,
                                MaterielNum = materienum,
                                // 库位编号
                                StorageNum = "AGV正在前往等待点，存放的库位等AGV到达等待点后自动获取",
                                // 出入库编号
                                InAndOutBoundNum = joinboundnum,
                            };

                            // 将数据保存到临时表中
                            await _InventoryTem.InsertAsync(addInventory);
                            await _InventoryDetailTem.InsertAsync(addInventoryDetail);
                            await _workbin.InsertOrUpdateAsync(addWorkBin);


                            // 得到每个料箱编号
                            if (list.Count > 1)
                            {
                                wbnum = workbinnum + "," + wbnum;
                            }
                            else
                            {
                                wbnum = workbinnum;
                            }
                        }
                        // 修改入库详情表里面的料箱编号和物料编号

                        await _InAndOutBoundDetail.AsUpdateable()
                                             .AS("EG_WMS_InAndOutBoundDetail")
                                             .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                                             {
                                                 WHNum = _regionlistdata.WHNum,
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
                        throw Oops.Oh("错误：" + ex.Message);
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

            throw Oops.Oh("错误：" + ex.Message);
        }
    }


    #endregion

    #region AGV入库（入库WMS自动推荐库位）（封装）（密集库）（潜伏举升）

    /// <summary>
    /// AGV入库（入库WMS自动推荐库位）（封装）（密集库）（潜伏举升）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AgvJoinBoundTasks", Order = 50)]
    public async Task AgvJoinBoundTasks(AgvJoinDto input)
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
            string endpoint = "";
            // 判断用户输入的是否符合逻辑
            var storageGroup = _Storage.GetFirstAsync(x => x.StorageNum == input.EndPoint);
            var selectData = _Storage.AsQueryable()
                 .Where(x => x.StorageGroup == storageGroup.Result.StorageGroup && x.StorageOccupy == 1)
                 .OrderBy(x => x.StorageNum, OrderByType.Asc)
                 .Select(x => x.StorageNum)
                 .ToList();
            if (storageGroup.Result.StorageNum.ToInt() > selectData[0].ToInt())
            {
                throw Oops.Oh("当前选择的这个库位，入库时有库位阻挡，请重新选择库位！");
            }
            else
            {
                endpoint = input.EndPoint;
            }

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

    #region AGV出库（两点位）（出库WMS自动推荐库位）（密集库）（潜伏举升）

    /// <summary>
    /// AGV出库（两点位）（出库WMS自动推荐库位）（密集库）（潜伏举升）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AgvOutBoundTask", Order = 45)]
    public async Task AgvOutBoundTask(AgvBoundDto input)
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
            if (input.StartPoint == null || input.StartPoint == "")
            {
                // 根据策略推荐
                input.StartPoint = BaseService.AGVStrategyReturnRecommendStorageOutBound(input.MaterielNum);
                if (input.StartPoint == "没有合适的库位")
                {
                    // 添加出库暂存任务
                    await InAndOutBoundMessage.NotBoundNotStorageAddStagingTask(input, outboundnum);
                    return;
                }
            }
            string startpoint = "";
            // 判断用户输入的是否符合逻辑
            var storageGroup = _Storage.GetFirstAsync(x => x.StorageNum == input.StartPoint);
            var selectData = _Storage.AsQueryable()
                 .Where(x => x.StorageGroup == storageGroup.Result.StorageGroup && x.StorageOccupy == 1)
                 .OrderBy(x => x.StorageNum, OrderByType.Desc)
                 .Select(x => x.StorageNum)
                 .ToList();
            if (storageGroup.Result.StorageNum.ToInt() > selectData[0].ToInt())
            {
                throw Oops.Oh("当前选择的这个库位，出库时有库位阻挡，请重新选择库位！");
            }
            else
            {
                startpoint = input.StartPoint;
            }

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
                          .Where(x => x.StorageNum == input.StartPoint)
                          .ExecuteCommandAsync();


                #region 生成出库

                // 生成出库单
                EG_WMS_InAndOutBound outbound = new EG_WMS_InAndOutBound
                {
                    // 编号
                    InAndOutBoundNum = outboundnum,
                    // 出入库类型（入库还是出库）
                    InAndOutBoundType = 1,
                    // 时间
                    InAndOutBoundTime = DateTime.Now,
                    // 操作人
                    InAndOutBoundUser = input.AddName,
                    // 备注
                    InAndOutBoundRemake = input.InAndOutBoundRemake,
                    // 创建时间
                    CreateTime = DateTime.Now,
                    // 起始点
                    StartPoint = startpoint,
                    // 目标点
                    EndPoint = endpoint,
                };

                // 查询库位编号所在的区域编号

                var regionnum = InAndOutBoundMessage.GetStorageWhereRegion(startpoint);

                // 通过查询出来的区域得到仓库编号

                var whnum = InAndOutBoundMessage.GetRegionWhereWHNum(regionnum);

                // 生成出库详单

                EG_WMS_InAndOutBoundDetail outbounddetail = new EG_WMS_InAndOutBoundDetail()
                {
                    // 出入库编号
                    InAndOutBoundNum = outboundnum,
                    CreateTime = DateTime.Now,
                    MaterielNum = input.MaterielNum,
                    // 区域编号
                    RegionNum = regionnum,
                    // 策略推荐的库位
                    StorageNum = input.StartPoint,
                };

                await _rep.InsertAsync(outbound);
                await _InAndOutBoundDetail.InsertAsync(outbounddetail);

                // 得到这个库位上的库存信息（先修改临时表里面的信息）

                var tem_InventoryDetails = _InventoryDetailTem.AsQueryable()
                                    .InnerJoin<EG_WMS_Tem_Inventory>((a, b) => a.InventoryNum == b.InventoryNum)
                                    .Where((a, b) => a.StorageNum == startpoint && b.OutboundStatus == 0 && a.IsDelete == false && b.IsDelete == false)
                                    .ToList();

                string wbnum = "";
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

                    // 得到每个料箱编号
                    if (tem_InventoryDetails.Count > 1)
                    {
                        wbnum = tem_InventoryDetails[i].WorkBinNum + "," + wbnum;
                    }
                    else
                    {
                        wbnum = tem_InventoryDetails[i].WorkBinNum;
                    }

                }
                // 修改出库详情表里面的料箱编号和物料编号
                await _InAndOutBoundDetail.AsUpdateable()
                                     .AS("EG_WMS_InAndOutBoundDetail")
                                     .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                                     {
                                         WHNum = whnum,
                                         WorkBinNum = wbnum,
                                         MaterielNum = input.MaterielNum,
                                     })
                                     .Where(u => u.InAndOutBoundNum == outboundnum)
                                     .ExecuteCommandAsync();
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

                #endregion
            }
            else
            {
                throw Oops.Oh("下达agv任务失败");
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
                        await InAndOutBoundMessage.WarehousingOperationTask(input, joinboundnum);
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
                        await InAndOutBoundMessage.UpdateInAndOutBoundTask(wbnum, sumcount, joinboundnum, _whnum, materienum);
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
                        // 通过查询出来的区域得到仓库编号
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

            throw Oops.Oh("错误：" + ex);
        }

    }


    #endregion

    #region AGV堆高车出库（人工选择库位出库）（立库）

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
                        // 通过查询出来的区域得到仓库编号
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

            throw Oops.Oh("错误：" + ex);
        }

    }


    #endregion

    #region 人工入库（已完成）
    /// <summary>
    /// 人工入库
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>

    [HttpPost]
    [ApiDescriptionSettings(Name = "ArtificialJoinBoundAdd", Order = 40)]
    public async Task ArtificialJoinBoundAdd(EGInBoundDto input)
    {
        // 生成当前时间时间戳
        string joinboundnum = _TimeStamp.GetTheCurrentTimeTimeStamp("EGRK");

        try
        {
            // 生成入库单
            EG_WMS_InAndOutBound inandoutbound = new EG_WMS_InAndOutBound
            {
                // 入库编号
                InAndOutBoundNum = joinboundnum,
                // 类型
                InAndOutBoundType = 0,
                // 时间
                InAndOutBoundTime = DateTime.Now,
                // 操作人（修改）
                InAndOutBoundUser = input.InAndOutBoundUser,
                // 入库备注
                InAndOutBoundRemake = input.InAndOutBoundRemake,
                // 入库目标点
                EndPoint = input.EndPoint,

            };

            #region 根据库位查询区域、仓库

            // 查询库位编号所在的区域编号
            var _storagelistdata = _Storage.AsQueryable()
                                   .Where(u => u.StorageNum == input.EndPoint)
                                   .Select(f => new
                                   {
                                       f.RegionNum
                                   })
                                   .ToList();

            if (_storagelistdata.Count == 0 || string.IsNullOrEmpty(_storagelistdata[0].RegionNum))
            {
                throw Oops.Oh("没有查询到这个库位");
            }

            string regionnum = _storagelistdata[0].RegionNum;

            // 通过查询出来的区域得到仓库编号

            var _regionlistdata = _Region.AsQueryable()
                    .Where(u => u.RegionNum == regionnum)
                    .Select(f => new
                    {
                        f.WHNum
                    })
                    .ToList();

            if (_regionlistdata.Count == 0 || string.IsNullOrEmpty(_regionlistdata[0].WHNum))
            {
                throw Oops.Oh("没有查询到这个仓库");
            }
            #endregion

            // 生成入库详单
            EG_WMS_InAndOutBoundDetail inandoutbounddetail = new EG_WMS_InAndOutBoundDetail
            {
                // 入库编号
                InAndOutBoundNum = joinboundnum,
                // 库位编号
                StorageNum = input.EndPoint,
                // 区域编号
                RegionNum = regionnum,
                // 仓库编号
                WHNum = _regionlistdata[0].WHNum,
                // 时间
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
                // 雪花ID
                var idone = SnowFlakeSingle.instance.NextId();
                var idtwo = SnowFlakeSingle.instance.NextId();
                // 库存编号（主表和详细表）
                string inventorynum = $"{i}EGKC" + _TimeStamp.GetTheCurrentTimeTimeStamp();
                // 料箱编号（详细表）
                string workbinnum = item[i].WorkBinNum;
                // 物料编号（主表）
                string materienum = item[i].MaterielNum;
                // 物料的数量（主表）
                int productcount = item[i].ProductCount;
                // 生产日期（料箱表）
                DateTime productiondate = item[i].ProductionDate;
                // 生产批次（详细表）
                string productionlot = item[i].ProductionLot;

                sumcount += productcount;
                // 将得到的数据，保存在库存主表和详细表中

                // 库存主表
                EG_WMS_Inventory addInventory = new EG_WMS_Inventory()
                {
                    // 雪花id
                    Id = idone,
                    // 库存编号
                    InventoryNum = inventorynum,
                    // 物料编号
                    MaterielNum = materienum,
                    // 库存总数
                    ICountAll = productcount,
                    // 创建时间
                    CreateTime = DateTime.Now,
                    // 入库编号
                    InBoundNum = joinboundnum,
                    // 是否删除
                    IsDelete = false,
                    // 是否出库
                    OutboundStatus = 0,
                };
                // 详细表
                EG_WMS_InventoryDetail addInventoryDetail = new EG_WMS_InventoryDetail()
                {
                    // 雪花id
                    Id = idtwo,
                    // 库存编号
                    InventoryNum = inventorynum,
                    // 料箱编号
                    WorkBinNum = workbinnum,
                    // 生产批次
                    ProductionLot = productionlot,
                    // 创建时间
                    CreateTime = DateTime.Now,
                    // 库位编号
                    StorageNum = input.EndPoint,
                    // 区域编号
                    RegionNum = regionnum,
                    // 仓库编号
                    WHNum = _regionlistdata[0].WHNum,
                    // 是否删除
                    IsDelete = false,

                };
                // 料箱表 将料箱内容保存到料箱表中（生成新料箱或修改）
                EG_WMS_WorkBin addWorkBin = new EG_WMS_WorkBin()
                {
                    // 编号
                    WorkBinNum = workbinnum,
                    // 产品数量
                    ProductCount = productcount,
                    // 生产批次
                    ProductionLot = productionlot,
                    CreateTime = DateTime.Now,
                    // 生产日期
                    ProductionDate = productiondate,
                    WorkBinStatus = 0,
                    MaterielNum = materienum,
                    // 库位编号
                    StorageNum = input.EndPoint,
                };
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
                      })
                      .Where(x => x.StorageNum == input.EndPoint)
                      .ExecuteCommandAsync();



        }
        catch (Exception ex)
        {
            throw Oops.Oh(ex.Message);
        }

    }



    #endregion

    #region 人工出库（已完成）（人工扫描库位库位上所有的料箱都出）

    /// <summary>
    /// 人工出库（人工扫描库位库位上所有的料箱都出）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>

    [HttpPost]
    [ApiDescriptionSettings(Name = "ArtificialOutBoundAdd", Order = 35)]
    public async Task ArtificialOutBoundAdd(EGOutBoundDto input)
    {

        // 生成当前时间时间戳
        string outboundnum = _TimeStamp.GetTheCurrentTimeTimeStamp("EGCK");

        try
        {
            // 人拿着pda扫
            // 扫库位，得到这个库位上所有的料箱，所有的料箱全部都出
            // 得到pda扫到的库位上所有的料箱（根据库位得到这个库位上所有的料箱(从库存中得到)）

            #region 通过库位查询得到区域编号、仓库编号

            // 查询库位编号所在的区域编号
            var _storagelistdata = _Storage.AsQueryable()
                                   .Where(u => u.StorageNum == input.StorageNum)
                                   .Select(f => new
                                   {
                                       f.RegionNum
                                   })
                                   .ToList();

            if (_storagelistdata.Count == 0 || string.IsNullOrEmpty(_storagelistdata[0].RegionNum))
            {
                throw Oops.Oh("没有查询到这个库位");
            }

            string regionnum = _storagelistdata[0].RegionNum;

            // 通过查询出来的区域得到仓库编号

            var _regionlistdata = _Region.AsQueryable()
                    .Where(u => u.RegionNum == regionnum)
                    .Select(f => new
                    {
                        f.WHNum
                    })
                    .ToList();

            if (_regionlistdata.Count == 0 || string.IsNullOrEmpty(_regionlistdata[0].WHNum))
            {
                throw Oops.Oh("没有查询到这个仓库");
            }
            #endregion

            // 1.得到这个库位上所有的数据

            List<EG_WMS_InventoryDetail> dataList =
                                        _InventoryDetail.AsQueryable()
                                        .InnerJoin<EG_WMS_Inventory>((a, b) => a.InventoryNum == b.InventoryNum)
                                        .Where((a, b) => a.StorageNum == input.StorageNum && b.OutboundStatus == 0)
                                        .ToList();

            if (dataList.Count == 0)
            {
                throw Oops.Oh("此库位上没有数据");
            }

            // 2.得到这个库位上所有的库存

            // 所有料箱的总数
            int? sumcount = 0;

            // 生成出库单
            EG_WMS_InAndOutBound inandoutbound = new EG_WMS_InAndOutBound
            {
                // 出库编号
                InAndOutBoundNum = outboundnum,
                // 类型
                InAndOutBoundType = 1,
                // 时间
                InAndOutBoundTime = DateTime.Now,
                // 出库备注
                InAndOutBoundRemake = input.Remake,
                CreateTime = DateTime.Now,
            };

            // 生成出库详单
            EG_WMS_InAndOutBoundDetail inandoutbounddetail = new EG_WMS_InAndOutBoundDetail
            {
                // 出库编号
                InAndOutBoundNum = outboundnum,
                // 库位编号
                StorageNum = input.StorageNum,
                // 区域编号
                RegionNum = regionnum,
                // 仓库编号
                WHNum = _regionlistdata[0].WHNum,
                // 时间
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

                // 将库存表中的出库状态改变
                await _Inventory.AsUpdateable()
                           .AS("EG_WMS_Inventory")
                           .SetColumns(u => new EG_WMS_Inventory
                           {
                               OutboundStatus = 1,
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
                     })
                     .Where(x => x.StorageNum == input.StorageNum)
                     .ExecuteCommandAsync();

        }
        catch (Exception ex)
        {
            throw Oops.Oh(ex.Message);
        }

    }


    #endregion

    #region 分页查询出入库信息
    /// <summary>
    /// 分页查询出入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page", Order = 30)]
    public async Task<SqlSugarPagedList<EG_WMS_InAndOutBoundOutput>> Page(EG_WMS_InAndOutBoundInput input)
    {
        var query = _rep.AsQueryable()
                        .InnerJoin<EG_WMS_InAndOutBoundDetail>((a, b) => a.InAndOutBoundNum == b.InAndOutBoundNum)
                        .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), (a, b) => b.MaterielNum.Contains(input.MaterielNum.Trim()))
                        .WhereIF(!string.IsNullOrWhiteSpace(input.InAndOutBoundNum), (a, b) => a.InAndOutBoundNum.Contains(input.InAndOutBoundNum.Trim()))
                        .WhereIF(input.InAndOutBoundType > 0, (a, b) => a.InAndOutBoundType == input.InAndOutBoundType)
                        .WhereIF(input.InAndOutBoundStatus > 0, (a, b) => a.InAndOutBoundStatus == input.InAndOutBoundStatus)
                        .WhereIF(!string.IsNullOrWhiteSpace(input.InAndOutBoundUser), (a, b) => a.InAndOutBoundUser.Contains(input.InAndOutBoundUser.Trim()))
                    // 倒序
                    .Where(a => a.InAndOutBoundType == input.InAndOutBoundType && a.IsDelete == false)
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

    #region 删除出入库信息
    /// <summary>
    /// 删除出入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete", Order = 25)]
    public async Task Delete(DeleteEG_WMS_InAndOutBoundInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
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

    //-------------------------------------//-------------------------------------// 

    #region agv入库（两点位）（没有判断agv是否执行成功）（已完成）

    ///// <summary>
    ///// agv入库（两点位）（没有判断agv是否执行成功）
    ///// </summary>
    ///// <param name="input"></param>
    ///// <returns></returns>
    ///// <exception cref="Exception"></exception>

    //[HttpPost]
    //[ApiDescriptionSettings(Name = "AgvJoinBound")]

    //public async Task AgvJoinBound(AgvJoinDto input)
    //{
    //    // 生成当前时间时间戳
    //    string timesstamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
    //    string joinboundnum = "EGRK" + timesstamp;

    //    #region 判断有无传入任务编号
    //    // 判断有无传入任务编号
    //    //string taskno;
    //    //if (input == null || input.TaskNo == null)
    //    //{
    //    //    var id = SnowFlakeSingle.instance.NextId().ToString();
    //    //    taskno = id;
    //    //}
    //    //else
    //    //{
    //    //    taskno = input.TaskNo;
    //    //}

    //    //var db = new SqlSugarClient(new ConnectionConfig
    //    //{
    //    //    ConnectionString = "server=127.0.0.1;Database=iwms;Uid=root;Pwd=123456;AllowLoadLocalInfile=true;min pool size=1",
    //    //    DbType = DbType.MySql,
    //    //    IsAutoCloseConnection = true,
    //    //});
    //    #endregion

    //    try
    //    {
    //        // 开启一个事务
    //        //db.BeginTran();

    //        // 起始点
    //        string startpoint = input.StartPoint;
    //        if (startpoint == null)
    //        {
    //            throw Oops.Oh("起始点不可为空");
    //        }

    //        // 目标点
    //        if (input.EndPoint == null)
    //        {
    //            // 根据策略推荐（修改）
    //        }

    //        string endpoint = input.EndPoint;

    //        #region 生成入库
    //        // 生成入库单
    //        EG_WMS_InAndOutBound joinbound = new EG_WMS_InAndOutBound
    //        {
    //            // 编号
    //            InAndOutBoundNum = joinboundnum,
    //            // 出入库类型（入库还是出库）
    //            InAndOutBoundType = 0,
    //            // 时间
    //            InAndOutBoundTime = DateTime.Now,
    //            // 操作人
    //            InAndOutBoundUser = input.AddName,
    //            // 备注
    //            InAndOutBoundRemake = input.InAndOutBoundRemake,
    //            // 创建时间
    //            CreateTime = DateTime.Now,
    //            // 起始点
    //            StartPoint = startpoint,
    //            // 目标点
    //            EndPoint = endpoint,
    //        };

    //        // 查询库位编号所在的区域编号
    //        var _storagelistdata = _Storage.AsQueryable()
    //                               .Where(u => u.StorageNum == endpoint)
    //                               .Select(f => new
    //                               {
    //                                   f.RegionNum
    //                               })
    //                               .ToList();

    //        if (_storagelistdata.Count == 0 || string.IsNullOrEmpty(_storagelistdata[0].RegionNum))
    //        {
    //            throw Oops.Oh("没有查询到这个库位");
    //        }

    //        string regionnum = _storagelistdata[0].RegionNum;

    //        // 通过查询出来的区域得到仓库编号

    //        var _regionlistdata = _Region.AsQueryable()
    //                .Where(u => u.RegionNum == regionnum)
    //                .Select(f => new
    //                {
    //                    f.WHNum
    //                })
    //                .ToList();

    //        if (_regionlistdata.Count == 0 || string.IsNullOrEmpty(_regionlistdata[0].WHNum))
    //        {
    //            throw Oops.Oh("没有查询到这个仓库");
    //        }

    //        // 生成入库详单

    //        EG_WMS_InAndOutBoundDetail joinbounddetail = new EG_WMS_InAndOutBoundDetail()
    //        {
    //            // 出入库编号
    //            InAndOutBoundNum = joinboundnum,
    //            CreateTime = DateTime.Now,
    //            // 区域编号
    //            RegionNum = _storagelistdata[0].RegionNum,
    //            // 目标点就是存储的点位即库位编号
    //            StorageNum = input.EndPoint,
    //        };

    //        await _rep.InsertAsync(joinbound);
    //        await _InAndOutBoundDetail.InsertAsync(joinbounddetail);
    //        #endregion

    //        // 任务点集
    //        var positions = startpoint + "," + endpoint;

    //        TaskEntity taskEntity = input.Adapt<TaskEntity>();
    //        taskEntity.TaskPath = positions;

    //        // 下达agv任务

    //        DHMessage item = await taskService.AddAsync(taskEntity);

    //        // 判断agv任务是否成功（修改）



    //        // 判断agv下达任务是否成功
    //        if (item.code == 1000)
    //        {

    //            // 得到入库的数据
    //            List<MaterielWorkBin> list = input.materielWorkBins;
    //            string wbnum = "";
    //            string wlnum = "";
    //            int sumcount = 0;
    //            for (int i = 0; i < list.Count; i++)
    //            {
    //                // 雪花ID
    //                var idone = SnowFlakeSingle.instance.NextId();
    //                var idtwo = SnowFlakeSingle.instance.NextId();
    //                // 库存编号（主表和详细表）
    //                string inventorynum = $"{i}EGKC" + timesstamp;
    //                // 料箱编号（详细表、料箱表）
    //                string workbinnum = list[i].WorkBinNum;
    //                // 物料编号（主表）
    //                string materienum = list[i].MaterielNum;
    //                // 物料的数量（主表、料箱表）
    //                int productcount = list[i].ProductCount;
    //                // 生产日期（料箱表）
    //                DateTime productiondate = list[i].ProductionDate;
    //                // 生产批次（详细表、料箱表）
    //                string productionlot = list[i].ProductionLot;

    //                // 总数
    //                sumcount += productcount;

    //                // 将得到的数据，保存在库存主表和详细表中

    //                // 库存主表
    //                EG_WMS_Inventory addInventory = new EG_WMS_Inventory()
    //                {
    //                    // 雪花id
    //                    Id = idone,
    //                    // 库存编号
    //                    InventoryNum = inventorynum,
    //                    // 物料编号
    //                    MaterielNum = materienum,
    //                    // 库存总数
    //                    ICountAll = productcount,
    //                    // 创建时间
    //                    CreateTime = DateTime.Now,
    //                    // 入库编号
    //                    InAndOutBoundNum = joinboundnum,
    //                    // 是否删除
    //                    IsDelete = false,
    //                    // 是否出库
    //                    OutboundStatus = 0,
    //                };

    //                // 详细表
    //                EG_WMS_InventoryDetail addInventoryDetail = new EG_WMS_InventoryDetail()
    //                {
    //                    // 雪花id
    //                    Id = idtwo,
    //                    // 库存编号
    //                    InventoryNum = inventorynum,
    //                    // 料箱编号
    //                    WorkBinNum = workbinnum,
    //                    // 生产批次
    //                    ProductionLot = productionlot,
    //                    // 创建时间
    //                    CreateTime = DateTime.Now,
    //                    // 库位编号
    //                    StorageNum = input.EndPoint,
    //                    // 区域编号
    //                    RegionNum = _storagelistdata[0].RegionNum,
    //                    // 仓库编号
    //                    WHNum = _regionlistdata[0].WHNum,
    //                    // 是否删除
    //                    IsDelete = false,

    //                };

    //                // 料箱表 将料箱内容保存到料箱表中
    //                EG_WMS_WorkBin addWorkBin = new EG_WMS_WorkBin()
    //                {
    //                    // 编号
    //                    WorkBinNum = workbinnum,
    //                    // 产品数量
    //                    ProductCount = productcount,
    //                    // 生产批次
    //                    ProductionLot = productionlot,
    //                    CreateTime = DateTime.Now,
    //                    // 生产日期
    //                    ProductionDate = productiondate,
    //                    WorkBinStatus = 0,
    //                    MaterielNum = materienum,
    //                    // 库位编号
    //                    StorageNum = input.EndPoint,
    //                };

    //                await _Inventory.InsertAsync(addInventory);
    //                await _InventoryDetail.InsertAsync(addInventoryDetail);
    //                await _workbin.InsertOrUpdateAsync(addWorkBin);

    //                // 得到每个料箱编号
    //                if (list.Count > 1)
    //                {
    //                    wbnum = workbinnum + "," + wbnum;
    //                    wlnum = materienum + "," + wlnum;
    //                }
    //                else
    //                {
    //                    wbnum = workbinnum;
    //                    wlnum = materienum;
    //                }

    //                // 提交事务
    //                //db.CommitTran();
    //            }

    //            // 修改入库详情表里面的料箱编号和物料编号

    //            await _InAndOutBoundDetail.AsUpdateable()
    //                                 .AS("EG_WMS_InAndOutBoundDetail")
    //                                 .SetColumns(it => new EG_WMS_InAndOutBoundDetail
    //                                 {
    //                                     WHNum = _regionlistdata[0].WHNum,
    //                                     WorkBinNum = wbnum,
    //                                     MaterielNum = wlnum,
    //                                 })
    //                                 .Where(u => u.InAndOutBoundNum == joinboundnum)
    //                                 .ExecuteCommandAsync();

    //            // 改变入库状态
    //            await _rep.AsUpdateable()
    //                 .AS("EG_WMS_InAndOutBound")
    //                 .SetColumns(it => new EG_WMS_InAndOutBound
    //                 {
    //                     InAndOutBoundCount = sumcount,
    //                     InAndOutBoundStatus = 1,
    //                     SuccessOrNot = 0,
    //                 })
    //                 .Where(u => u.InAndOutBoundNum == joinboundnum)
    //                 .ExecuteCommandAsync();

    //        }
    //        else
    //        {
    //            // 失败时，将生成的入库单修改为失败
    //            await _rep.AsUpdateable()
    //                 .AS("EG_WMS_InAndOutBound")
    //                 .SetColumns(it => new EG_WMS_InAndOutBound
    //                 {
    //                     // 入库失败
    //                     SuccessOrNot = 1,
    //                     // 未入库
    //                     InAndOutBoundStatus = 0,
    //                 })
    //                 .Where(u => u.InAndOutBoundNum == joinboundnum)
    //                 .ExecuteCommandAsync();

    //            throw new Exception("agv任务执行失败");
    //        }

    //    }
    //    catch (Exception ex)
    //    {
    //        // 回滚事务
    //        //db.RollbackTran();
    //        throw new Exception(ex.Message);
    //    }
    //}

    #endregion

    #region agv出库（两点位）（没有判断agv是否执行成功）（已完成）

    ///// <summary>
    ///// agv出库（没有判断agv是否执行成功）（两点位）
    ///// </summary>
    ///// <param name="input"></param>
    ///// <returns></returns>
    ///// <exception cref="Exception"></exception>

    //[HttpPost]
    //[ApiDescriptionSettings(Name = "AgvOutBound")]

    //public async Task AgvOutBound(AgvJoinDto input)
    //{
    //    // 生成当前时间时间戳
    //    string timesstamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
    //    string outboundnum = "EGCK" + timesstamp;

    //    try
    //    {
    //        // 开启一个事务
    //        //db.BeginTran();

    //        // 起始点
    //        string startpoint = input.StartPoint;
    //        if (startpoint == null)
    //        {
    //            throw Oops.Oh("起始点不可为空");
    //        }

    //        // 目标点
    //        string endpoint = input.EndPoint;
    //        if (endpoint == null)
    //        {
    //            // 根据策略推荐
    //        }

    //        #region 生成出库

    //        // 生成出库单
    //        EG_WMS_InAndOutBound outbound = new EG_WMS_InAndOutBound
    //        {
    //            // 编号
    //            InAndOutBoundNum = outboundnum,
    //            // 出入库类型（入库还是出库）
    //            InAndOutBoundType = 1,
    //            // 时间
    //            InAndOutBoundTime = DateTime.Now,
    //            // 操作人
    //            InAndOutBoundUser = input.AddName,
    //            // 备注
    //            InAndOutBoundRemake = input.InAndOutBoundRemake,
    //            // 创建时间
    //            CreateTime = DateTime.Now,
    //            // 起始点
    //            StartPoint = startpoint,
    //            // 目标点
    //            EndPoint = endpoint,
    //        };

    //        // 查询库位编号所在的区域编号
    //        var _storagelistdata = _Storage.AsQueryable()
    //                               .Where(u => u.StorageNum == endpoint)
    //                               .Select(f => new
    //                               {
    //                                   f.RegionNum
    //                               })
    //                               .ToList();

    //        if (_storagelistdata.Count == 0 || string.IsNullOrEmpty(_storagelistdata[0].RegionNum))
    //        {
    //            throw Oops.Oh("没有查询到这个库位");
    //        }

    //        string regionnum = _storagelistdata[0].RegionNum;

    //        // 通过查询出来的区域得到仓库编号

    //        var _regionlistdata = _Region.AsQueryable()
    //                .Where(u => u.RegionNum == regionnum)
    //                .Select(f => new
    //                {
    //                    f.WHNum
    //                })
    //                .ToList();

    //        if (_regionlistdata.Count == 0 || string.IsNullOrEmpty(_regionlistdata[0].WHNum))
    //        {
    //            throw Oops.Oh("没有查询到这个仓库");
    //        }

    //        // 生成出库详单

    //        EG_WMS_InAndOutBoundDetail outbounddetail = new EG_WMS_InAndOutBoundDetail()
    //        {
    //            // 出入库编号
    //            InAndOutBoundNum = outboundnum,
    //            CreateTime = DateTime.Now,
    //            // 区域编号
    //            RegionNum = _storagelistdata[0].RegionNum,
    //            // 目标点就是存储的点位即库位编号
    //            StorageNum = input.EndPoint,
    //        };

    //        await _rep.InsertAsync(outbound);
    //        await _InAndOutBoundDetail.InsertAsync(outbounddetail);
    //        #endregion


    //        // 任务点集
    //        var positions = startpoint + "," + endpoint;



    //        TaskEntity taskEntity = input.Adapt<TaskEntity>();
    //        taskEntity.TaskPath = positions;

    //        // 下达agv任务

    //        DHMessage item = await taskService.AddAsync(taskEntity);

    //        // 判断agv入库是否成功

    //        if (item.code == 1000)
    //        {

    //            // 得到出库的数据
    //            List<MaterielWorkBin> list = input.materielWorkBins;
    //            string wbnum = "";
    //            string wlnum = "";
    //            int sumcount = 0;
    //            for (int i = 0; i < list.Count; i++)
    //            {
    //                // 查询出出库数据的料箱编号
    //                var inum = _InventoryDetail.AsQueryable()
    //                                 .Where(u => u.WorkBinNum == list[i].WorkBinNum)
    //                                 .Select(f => new
    //                                 {
    //                                     f.InventoryNum
    //                                 })
    //                                 .ToList();


    //                // 库存编号（主表和详细表）
    //                string inventorynum = inum[0].InventoryNum;
    //                // 料箱编号（详细表）
    //                string workbinnum = list[i].WorkBinNum;
    //                // 物料编号（主表）
    //                string materienum = list[i].MaterielNum;
    //                // 物料的数量（主表）
    //                int productcount = list[i].ProductCount;
    //                // 生产日期（）
    //                DateTime productiondate = list[i].ProductionDate;
    //                // 生产批次（详细表）
    //                string productionlot = list[i].ProductionLot;

    //                // 总数
    //                sumcount += productcount;

    //                // 将得到的数据，修改在库存主表和详细表中
    //                // 库存主表

    //                await _Inventory.AsUpdateable()
    //                 .AS("EG_WMS_Inventory")
    //                 .SetColumns(it => new EG_WMS_Inventory
    //                 {
    //                     // 改变出库状态
    //                     OutboundStatus = 1,

    //                 })
    //                 // 库存编号相同
    //                 .Where(u => u.InventoryNum == inventorynum)
    //                 .ExecuteCommandAsync();

    //                // 得到每个料箱编号
    //                if (list.Count > 1)
    //                {
    //                    wbnum = workbinnum + "," + wbnum;
    //                    wlnum = materienum + "," + wlnum;
    //                }
    //                else
    //                {
    //                    wbnum = workbinnum;
    //                    wlnum = materienum;
    //                }
    //                // 提交事务
    //                //db.CommitTran();
    //            }

    //            // 修改出库详情表里面的料箱编号和物料编号

    //            await _InAndOutBoundDetail.AsUpdateable()
    //                                 .AS("EG_WMS_InAndOutBoundDetail")
    //                                 .SetColumns(it => new EG_WMS_InAndOutBoundDetail
    //                                 {
    //                                     WHNum = _regionlistdata[0].WHNum,
    //                                     WorkBinNum = wbnum,
    //                                     MaterielNum = wlnum,
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
    //                     InAndOutBoundStatus = 3,
    //                     SuccessOrNot = 0,
    //                 })
    //                 .Where(u => u.InAndOutBoundNum == outboundnum)
    //                 .ExecuteCommandAsync();

    //        }
    //        else
    //        {

    //            // 失败时，将生成的出库单修改为失败
    //            await _rep.AsUpdateable()
    //                 .AS("EG_WMS_InAndOutBound")
    //                 .SetColumns(it => new EG_WMS_InAndOutBound
    //                 {
    //                     // 出库失败
    //                     SuccessOrNot = 1,
    //                     // 未出库
    //                     InAndOutBoundStatus = 2,
    //                 })
    //                 .Where(u => u.InAndOutBoundNum == outboundnum)
    //                 .ExecuteCommandAsync();

    //            throw new Exception("agv任务执行失败");
    //        }

    //    }
    //    catch (Exception ex)
    //    {
    //        // 回滚事务
    //        //db.RollbackTran();
    //        throw new Exception(ex.Message);
    //    }
    //}
    #endregion

    #region agv出库（完整）（已完成）

    ///// <summary>
    ///// agv出库（两点位）（完整）
    ///// </summary>
    ///// <returns></returns>
    //public async Task AgvOutBoundTask(AgvJoinDto input)
    //{
    //    // 生成当前时间时间戳
    //    string timesstamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
    //    string outboundnum = "EGCK" + timesstamp;

    //    try
    //    {
    //        // 起始点
    //        string startpoint = input.StartPoint;
    //        if (startpoint == null)
    //        {
    //            throw Oops.Oh("起始点不可为空");
    //        }

    //        // 目标点
    //        string endpoint = input.EndPoint;
    //        if (endpoint == null || input.EndPoint == "")
    //        {
    //            // 根据策略推荐
    //            input.EndPoint = BaseService.StrategyReturnRecommendStorageOutBound(input.materielWorkBins[0].MaterielNum);
    //        }

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
    //                      })
    //                      .Where(x => x.StorageNum == input.EndPoint)
    //                      .ExecuteCommandAsync();


    //            #region 生成出库

    //            // 生成出库单
    //            EG_WMS_InAndOutBound outbound = new EG_WMS_InAndOutBound
    //            {
    //                // 编号
    //                InAndOutBoundNum = outboundnum,
    //                // 出入库类型（入库还是出库）
    //                InAndOutBoundType = 1,
    //                // 时间
    //                InAndOutBoundTime = DateTime.Now,
    //                // 操作人
    //                InAndOutBoundUser = input.AddName,
    //                // 备注
    //                InAndOutBoundRemake = input.InAndOutBoundRemake,
    //                // 创建时间
    //                CreateTime = DateTime.Now,
    //                // 起始点
    //                StartPoint = startpoint,
    //                // 目标点
    //                EndPoint = endpoint,
    //            };

    //            // 查询库位编号所在的区域编号
    //            var _storagelistdata = await _Storage.GetFirstAsync(u => u.StorageNum == input.EndPoint);


    //            if (_storagelistdata == null || string.IsNullOrEmpty(_storagelistdata.RegionNum))
    //            {
    //                throw Oops.Oh("没有查询到这个库位");
    //            }

    //            string regionnum = _storagelistdata.RegionNum;

    //            // 通过查询出来的区域得到仓库编号

    //            var _regionlistdata = await _Region.GetFirstAsync(x => x.RegionNum == regionnum);


    //            if (_regionlistdata == null || string.IsNullOrEmpty(_regionlistdata.WHNum))
    //            {
    //                throw Oops.Oh("没有查询到这个仓库");
    //            }

    //            // 生成出库详单

    //            EG_WMS_InAndOutBoundDetail outbounddetail = new EG_WMS_InAndOutBoundDetail()
    //            {
    //                // 出入库编号
    //                InAndOutBoundNum = outboundnum,
    //                CreateTime = DateTime.Now,
    //                // 区域编号
    //                RegionNum = _storagelistdata.RegionNum,
    //                // 目标点就是存储的点位即库位编号
    //                StorageNum = input.EndPoint,
    //            };

    //            await _rep.InsertAsync(outbound);
    //            await _InAndOutBoundDetail.InsertAsync(outbounddetail);
    //            #endregion

    //            // 得到出库的数据
    //            List<MaterielWorkBin> list = input.materielWorkBins;
    //            string wbnum = "";
    //            string wlnum = "";
    //            int sumcount = 0;
    //            for (int i = 0; i < list.Count; i++)
    //            {
    //                // 查询出出库数据的料箱编号
    //                var inum = _InventoryDetail.AsQueryable()
    //                                 .Where(u => u.WorkBinNum == list[i].WorkBinNum)
    //                                 .Select(f => new
    //                                 {
    //                                     f.InventoryNum
    //                                 })
    //                                 .ToList();

    //                // 库存编号（主表和详细表）
    //                string inventorynum = inum[0].InventoryNum;
    //                // 料箱编号（详细表）
    //                string workbinnum = list[i].WorkBinNum;
    //                // 物料编号（主表）
    //                string materienum = list[i].MaterielNum;
    //                // 物料的数量（主表）
    //                int productcount = list[i].ProductCount;
    //                // 生产日期（）
    //                DateTime productiondate = list[i].ProductionDate;
    //                // 生产批次（详细表）
    //                string productionlot = list[i].ProductionLot;

    //                // 总数
    //                sumcount += productcount;

    //                // 将得到的数据，修改先修改到临时库存主表和详细表中
    //                // 临时库存主表

    //                await _InventoryTem.AsUpdateable()
    //                 .AS("EG_WMS_Tem_Inventory")
    //                 .SetColumns(it => new EG_WMS_Tem_Inventory
    //                 {
    //                     InAndOutBoundNum = outboundnum,
    //                     // 改变出库状态
    //                     OutboundStatus = 1,

    //                     UpdateTime = DateTime.Now,
    //                 })
    //                 // 库存编号相同
    //                 .Where(u => u.InventoryNum == inventorynum)
    //                 .ExecuteCommandAsync();

    //                // 得到每个料箱编号
    //                if (list.Count > 1)
    //                {
    //                    wbnum = workbinnum + "," + wbnum;
    //                    wlnum = materienum + "," + wlnum;
    //                }
    //                else
    //                {
    //                    wbnum = workbinnum;
    //                    wlnum = materienum;
    //                }
    //            }

    //            // 修改出库详情表里面的料箱编号和物料编号
    //            await _InAndOutBoundDetail.AsUpdateable()
    //                                 .AS("EG_WMS_InAndOutBoundDetail")
    //                                 .SetColumns(it => new EG_WMS_InAndOutBoundDetail
    //                                 {
    //                                     WHNum = _regionlistdata.WHNum,
    //                                     WorkBinNum = wbnum,
    //                                     MaterielNum = wlnum,
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

    //        }
    //        else
    //        {
    //            throw new Exception("下达agv任务失败");
    //        }

    //    }
    //    catch (Exception ex)
    //    {

    //        throw new Exception(ex.Message);
    //    }

    //}

    #endregion

}
