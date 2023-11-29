namespace Admin.NET.Application.Service.EG_WMS_InAndOutBound;

/// <summary>
/// AGV出入库业务逻辑
/// </summary>
public class EG_WMS_InAndOutBoundMessage
{
    #region Tool
    private static readonly ToolTheCurrentTime _TimeStamp = new ToolTheCurrentTime();
    #endregion

    #region 关系注入

    private readonly SqlSugarRepository<Entity.EG_WMS_InAndOutBound> _rep = App.GetService<SqlSugarRepository<Entity.EG_WMS_InAndOutBound>>();
    private readonly SqlSugarRepository<EG_WMS_InAndOutBoundDetail> _InAndOutBoundDetail = App.GetService<SqlSugarRepository<EG_WMS_InAndOutBoundDetail>>();
    private readonly SqlSugarRepository<EG_WMS_Inventory> _Inventory = App.GetService<SqlSugarRepository<EG_WMS_Inventory>>();
    private readonly SqlSugarRepository<EG_WMS_InventoryDetail> _InventoryDetail = App.GetService<SqlSugarRepository<EG_WMS_InventoryDetail>>();
    private readonly SqlSugarRepository<Entity.EG_WMS_Storage> _Storage = App.GetService<SqlSugarRepository<Entity.EG_WMS_Storage>>();
    private readonly SqlSugarRepository<Entity.EG_WMS_Region> _Region = App.GetService<SqlSugarRepository<Entity.EG_WMS_Region>>();
    private readonly SqlSugarRepository<EG_WMS_WorkBin> _workbin = App.GetService<SqlSugarRepository<EG_WMS_WorkBin>>();
    private readonly SqlSugarRepository<EG_WMS_Tem_Inventory> _InventoryTem = App.GetService<SqlSugarRepository<EG_WMS_Tem_Inventory>>();
    private readonly SqlSugarRepository<EG_WMS_Tem_InventoryDetail> _InventoryDetailTem = App.GetService<SqlSugarRepository<EG_WMS_Tem_InventoryDetail>>();
    private readonly SqlSugarRepository<TaskEntity> _TaskEntity = App.GetService<SqlSugarRepository<TaskEntity>>();
    private readonly SqlSugarRepository<TemLogicEntity> _TemLogicEntity = App.GetService<SqlSugarRepository<TemLogicEntity>>();
    private readonly SqlSugarRepository<TaskStagingEntity> _TaskStagingEntity = App.GetService<SqlSugarRepository<TaskStagingEntity>>();
    #endregion

    /// <summary>
    /// Agv入库封装方法
    /// </summary>
    /// <param name="input"></param>
    /// <param name="joinboundnum"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task ProcessInbound(AgvJoinDto input, string joinboundnum)
    {

        using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {

            try
            {

                // 修改库位表
                await ModifyInventoryLocationOccupancy(input, joinboundnum);

                // 入库操作
                await WarehousingOperationTask(input, joinboundnum);

                // 临时库存表操作
                var _temdata = await TemporaryInventoryStatementTask(input, joinboundnum);

                // 修改出入库表、详情表
                await UpdateInAndOutBoundTask(_temdata.WorkBinNum, _temdata.CountSum, joinboundnum, _temdata.WHNum, _temdata.MaterieNum);

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

    /// <summary>
    /// 修改库位表占用（参数传递）
    /// </summary>
    /// <param name="input"></param>
    /// <param name="joinboundnum"></param>
    /// <returns></returns>
    public async Task ModifyInventoryLocationOccupancy(AgvJoinDto input, string joinboundnum)
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
                  .SetColumns(it => new Entity.EG_WMS_Storage
                  {
                      // 预占用
                      StorageOccupy = 2,
                      TaskNo = datatask.TaskNo,
                      // 得到日期最大的生产日期
                      StorageProductionDate = datetime.Max(),
                  })
                  .Where(x => x.StorageNum == input.EndPoint)
                  .ExecuteCommandAsync();
    }

    /// <summary>
    /// 修改库位表占用（列表查询）
    /// </summary>
    /// <param name="inandoutboundnum">出入库编号</param>
    /// <param name="EndPoint">目标点</param>
    /// <returns></returns>
    public async Task ModifyInventoryLocationOccupancy(string inandoutboundnum, string EndPoint)
    {
        // 得到入库的数据
        List<EG_WMS_WorkBin> list = _workbin.AsQueryable()
                 .Where(x => x.InAndOutBoundNum == inandoutboundnum)
                 .ToList();

        // 根据入库编号查询任务编号
        var datatask = await _TaskEntity.GetFirstAsync(x => x.InAndOutBoundNum == inandoutboundnum);

        List<DateTime?> datetime = new List<DateTime?>();
        // 将料箱的生产日期保存
        for (int i = 0; i < list.Count; i++)
        {
            datetime.Add(list[i].ProductionDate);
        }

        // 修改库位表中的状态为占用
        await _Storage.AsUpdateable()
                  .AS("EG_WMS_Storage")
                  .SetColumns(it => new Entity.EG_WMS_Storage
                  {
                      // 预占用
                      StorageOccupy = 2,
                      TaskNo = datatask.TaskNo,
                      // 得到日期最大的生产日期
                      StorageProductionDate = datetime.Max(),
                  })
                  .Where(x => x.StorageNum == EndPoint)
                  .ExecuteCommandAsync();


    }


    /// <summary>
    /// 入库操作（携带目标库位）
    /// </summary>
    /// <returns></returns>
    public async Task WarehousingOperationTask(AgvJoinDto input, string joinboundnum)
    {
        #region 入库操作

        // 生成入库单
        Entity.EG_WMS_InAndOutBound joinbound = new Entity.EG_WMS_InAndOutBound
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

        // 得到区域编号
        var _storageRegionNum = GetStorageWhereRegion(input.EndPoint);

        // 生成入库详单

        EG_WMS_InAndOutBoundDetail joinbounddetail = new EG_WMS_InAndOutBoundDetail()
        {
            // 出入库编号
            InAndOutBoundNum = joinboundnum,
            CreateTime = DateTime.Now,
            // 区域编号
            RegionNum = _storageRegionNum,
            // 目标点就是存储的点位即库位编号
            StorageNum = input.EndPoint,
        };
        #endregion

        await _rep.InsertAsync(joinbound);
        await _InAndOutBoundDetail.InsertAsync(joinbounddetail);

    }

    /// <summary>
    /// 修改出入库表、详情表
    /// </summary>
    /// <returns></returns>
    public async Task UpdateInAndOutBoundTask(string workbin, int sumcount, string joinboundnum, string whnum, string materienum)
    {

        // 修改入库详情表里面的料箱编号和物料编号

        await _InAndOutBoundDetail.AsUpdateable()
                             .AS("EG_WMS_InAndOutBoundDetail")
                             .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                             {
                                 WHNum = whnum,
                                 WorkBinNum = workbin,
                                 // 只会有一种物料
                                 MaterielNum = materienum,
                             })
                             .Where(u => u.InAndOutBoundNum == joinboundnum)
                             .ExecuteCommandAsync();

        // 改变入库状态
        await _rep.AsUpdateable()
             .AS("EG_WMS_InAndOutBound")
             .SetColumns(it => new Entity.EG_WMS_InAndOutBound
             {
                 InAndOutBoundCount = sumcount,
                 // 入库中
                 InAndOutBoundStatus = 4,
             })
             .Where(u => u.InAndOutBoundNum == joinboundnum)
             .ExecuteCommandAsync();

    }

    /// <summary>
    /// 临时库存表操作
    /// </summary>
    /// <returns></returns>
    public async Task<NeedWorkBinAllDataDto> TemporaryInventoryStatementTask(AgvJoinDto input, string joinboundnum)
    {
        NeedWorkBinAllDataDto NeedWorkBinAllData = new NeedWorkBinAllDataDto();
        List<MaterielWorkBin> list = input.materielWorkBins;

        // 根据库位编号得到仓库编号和区域编号
        string _regionnum = GetStorageWhereRegion(input.EndPoint);
        string _whnum = GetRegionWhereWHNum(_regionnum);

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
                StorageNum = input.EndPoint,
                // 区域编号
                RegionNum = _regionnum,
                // 仓库编号
                WHNum = _whnum,
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
            NeedWorkBinAllData.WorkBinNum = wbnum;
        }
        NeedWorkBinAllData.CountSum = sumcount;
        NeedWorkBinAllData.WHNum = _whnum;
        NeedWorkBinAllData.MaterieNum = list[0].MaterielNum;

        return NeedWorkBinAllData;
    }

    /// <summary>
    /// 通过区域编号得到仓库编号
    /// </summary>
    /// <param name="regionnum">区域编号</param>
    /// <returns></returns>
    public string GetRegionWhereWHNum(string regionnum)
    {
        // 通过查询出来的区域得到仓库编号

        var _regionlistdata = _Region.GetFirst(x => x.RegionNum == regionnum);

        if (_regionlistdata == null || string.IsNullOrEmpty(_regionlistdata.WHNum))
        {
            throw Oops.Oh("没有查询到这个仓库");
        }

        return _regionlistdata.WHNum;
    }

    /// <summary>
    /// 通过库位得到区域编号
    /// </summary>
    /// <param name="storagenum">库位编号</param>
    /// <returns></returns>
    public string GetStorageWhereRegion(string storagenum)
    {
        // 查询库位编号所在的区域编号

        var _storagelistdata = _Storage.GetFirst(u => u.StorageNum == storagenum);

        if (_storagelistdata == null || string.IsNullOrEmpty(_storagelistdata.RegionNum))
        {
            throw Oops.Oh("没有查询到这个库位编号所在的区域编号");
        }

        return _storagelistdata.RegionNum;

    }

    /// <summary>
    /// 添加入库暂存任务（没有目标点）
    /// </summary>
    /// <returns></returns>
    public async Task NotStorageAddStagingTask(AgvJoinDto input, string boundNum)
    {
        using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                var agvboundpara = input.Adapt<AgvBoundBO>();
                // 添加暂存Agv任务
                await AddStagingTask(agvboundpara, boundNum);
                // 添加入库单 
                #region 添加入库单
                Entity.EG_WMS_InAndOutBound joinbound = new Entity.EG_WMS_InAndOutBound
                {
                    // 编号
                    InAndOutBoundNum = boundNum,
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
                    EndPoint = null,
                };

                // 生成入库详单

                EG_WMS_InAndOutBoundDetail joinbounddetail = new EG_WMS_InAndOutBoundDetail()
                {
                    // 出入库编号
                    InAndOutBoundNum = boundNum,
                    CreateTime = DateTime.Now,
                    // 区域编号
                    RegionNum = null,
                    // 目标点就是存储的点位即库位编号
                    StorageNum = null,
                };

                await _rep.InsertAsync(joinbound);
                await _InAndOutBoundDetail.InsertAsync(joinbounddetail);
                #endregion
                // 保存临时数据
                await AddWorkBinDataList(input.materielWorkBins, boundNum);
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

    /// <summary>
    /// 添加出库暂存任务
    /// </summary>
    /// <returns></returns>
    public async Task NotBoundNotStorageAddStagingTask(AgvBoundDto input, string boundNum)
    {
        using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                var agvboundbo = input.Adapt<AgvBoundBO>();
                // 添加agv暂存任务
                await AddStagingTask(agvboundbo, boundNum);

                #region 出库单
                Entity.EG_WMS_InAndOutBound data = new Entity.EG_WMS_InAndOutBound()
                {
                    InAndOutBoundNum = boundNum,
                    InAndOutBoundType = 1,
                    InAndOutBoundTime = DateTime.Now,
                    InAndOutBoundUser = input.AddName,
                    InAndOutBoundStatus = 2,
                    InAndOutBoundRemake = input.InAndOutBoundRemake,
                    CreateTime = DateTime.Now,
                    StartPoint = null,
                    EndPoint = input.EndPoint,
                };

                EG_WMS_InAndOutBoundDetail datadetail = new EG_WMS_InAndOutBoundDetail()
                {
                    InAndOutBoundNum = boundNum,
                    MaterielNum = input.MaterielNum,
                    CreateTime = DateTime.Now,
                    WHNum = null,
                    RegionNum = null,
                    StorageNum = null,
                };

                await _rep.InsertAsync(data);
                await _InAndOutBoundDetail.InsertAsync(datadetail);
                #endregion

                scope.Complete();
            }
            catch (Exception ex)
            {
                scope.Dispose();
                throw Oops.Oh("错误" + ex.Message);

            }
        }
    }

    /// <summary>
    /// 添加Agv暂存任务
    /// </summary>
    /// <returns></returns>
    public async Task AddStagingTask(AgvBoundBO agvbound, string boundNum)
    {
        if (agvbound == null)
        {
            throw Oops.Oh("未携带AGV相关参数！");
        }

        // 查询任务名称
        var logictem = _TemLogicEntity.GetFirst(x => x.TemLogicNo == agvbound.ModelNo);
        if (logictem == null)
        {
            throw Oops.Oh("没有找到该模板！");
        }
        TaskStagingEntity taskstagingentity = agvbound.Adapt<TaskStagingEntity>();
        if (taskstagingentity.Id == 0) taskstagingentity.Id = SnowFlakeSingle.Instance.NextId();
        taskstagingentity.TaskNo = taskstagingentity.TaskNo ?? taskstagingentity.Id.ToString();
        taskstagingentity.TaskName = taskstagingentity.TaskName ?? logictem.TemLogicName;
        if (agvbound.StartPoint != "没有合适的库位")
        {
            taskstagingentity.TaskPath = agvbound.StartPoint + ",";

        }
        else
        {
            taskstagingentity.TaskPath = "," + agvbound.EndPoint;
        }
        taskstagingentity.CreateTime = DateTime.Now;
        taskstagingentity.Source = taskstagingentity.Source ?? "手工下发";
        taskstagingentity.TaskState = null;
        // 任务暂存状态
        taskstagingentity.StagingStatus = 0;
        if (taskstagingentity.Priority == 0) taskstagingentity.Priority = 6;
        taskstagingentity.InAndOutBoundNum = boundNum;

        // 保存任务
        await _TaskStagingEntity.InsertAsync(taskstagingentity);

    }

    /// <summary>
    /// 保存料箱数据
    /// </summary>
    public async Task AddWorkBinDataList(List<MaterielWorkBin> input, string boundNum)
    {
        List<MaterielWorkBin> list = input;

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
                InBoundNum = boundNum,
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
                StorageNum = null,
                // 区域编号
                RegionNum = null,
                // 仓库编号
                WHNum = null,
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
                StorageNum = null,
                // 出入库编号
                InAndOutBoundNum = boundNum,
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

        // 修改入库单

        #region 修改入库单（主要保存里面的物料数据）

        await _rep.AsUpdateable()
                  .SetColumns(it => new Entity.EG_WMS_InAndOutBound
                  {
                      InAndOutBoundCount = sumcount,
                      // 任务还没有下发（保存入库单）（未入库）
                      InAndOutBoundStatus = 0,

                  })
                  .Where(x => x.InAndOutBoundNum == boundNum)
                  .ExecuteCommandAsync();

        await _InAndOutBoundDetail.AsUpdateable()
                             .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                             {
                                 WorkBinNum = wbnum,
                                 // 只会有一种物料
                                 MaterielNum = list[0].MaterielNum,

                             })
                             .Where(x => x.InAndOutBoundNum == boundNum)
                             .ExecuteCommandAsync();

        #endregion

    }

    /// <summary>
    /// 传递参数Dto
    /// </summary>
    public class NeedWorkBinAllDataDto
    {
        public string WorkBinNum { get; set; }
        public int CountSum { get; set; }
        public string WHNum { get; set; }
        public string MaterieNum { get; set; }
    }
}