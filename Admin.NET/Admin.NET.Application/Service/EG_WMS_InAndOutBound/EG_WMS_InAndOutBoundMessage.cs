
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
    private readonly SqlSugarRepository<Entity.EG_WMS_Inventory> _Inventory = App.GetService<SqlSugarRepository<Entity.EG_WMS_Inventory>>();
    private readonly SqlSugarRepository<EG_WMS_InventoryDetail> _InventoryDetail = App.GetService<SqlSugarRepository<EG_WMS_InventoryDetail>>();
    private readonly SqlSugarRepository<Entity.EG_WMS_Storage> _Storage = App.GetService<SqlSugarRepository<Entity.EG_WMS_Storage>>();
    private readonly SqlSugarRepository<Entity.EG_WMS_Region> _Region = App.GetService<SqlSugarRepository<Entity.EG_WMS_Region>>();
    private readonly SqlSugarRepository<Entity.EG_WMS_WorkBin> _workbin = App.GetService<SqlSugarRepository<Entity.EG_WMS_WorkBin>>();
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
    public async Task ProcessInbound(AgvJoinBoundNewDto input, string joinboundnum)
    {
        using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {

            try
            {
                // 产品计算总数
                int SumCount = 0;
                for (int i = 0; i < input.materielWorkBins.Count; i++)
                {
                    SumCount += input.materielWorkBins[i].ProductCount;
                }
                var WHReceipt = input.Adapt<WareHouseReceiptDto>();
                WHReceipt.InAndOutBoundCount = SumCount;
                WHReceipt.MaterielNum = input.materielWorkBins[0].MaterielNum;

                // 修改库位表
                await ModifyInventoryLocationOccupancy(input.materielWorkBins, input.EndPoint, joinboundnum);

                // 入库操作
                await WarehousingOperationTask(WHReceipt, joinboundnum);

                // 临时库存表操作
                await TemporaryInventoryStatementTask(input.materielWorkBins, input.EndPoint, joinboundnum);

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
    /// <param name="materielProduct">料箱产品</param>
    /// <param name="endPoint">目标点</param>
    /// <param name="joinBoundNum">入库编号</param>
    /// <returns></returns>
    public async Task ModifyInventoryLocationOccupancy(List<MaterielProductDto> materielProduct, string endPoint, string joinBoundNum)
    {
        List<MaterielProductDto> list = materielProduct;
        var datatask = await _TaskEntity.GetFirstAsync(x => x.InAndOutBoundNum == joinBoundNum);
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
                      UpdateTime = DateTime.Now,
                  })
                  .Where(x => x.StorageNum == endPoint)
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
        List<Entity.EG_WMS_WorkBin> list = _workbin.AsQueryable()
                 .Where(x => x.InAndOutBoundNum == inandoutboundnum)
                 .ToList();
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
                      UpdateTime = DateTime.Now,
                  })
                  .Where(x => x.StorageNum == EndPoint)
                  .ExecuteCommandAsync();


    }


    /// <summary>
    /// 入库操作（携带目标库位）
    /// </summary>
    /// <returns></returns>
    public async Task WarehousingOperationTask(WareHouseReceiptDto input, string joinboundnum)
    {
        #region 入库操作

        // 生成入库单
        Entity.EG_WMS_InAndOutBound joinbound = new Entity.EG_WMS_InAndOutBound
        {
            InAndOutBoundNum = joinboundnum,
            InAndOutBoundType = 0,
            InAndOutBoundStatus = 4,
            InAndOutBoundCount = input.InAndOutBoundCount,
            InAndOutBoundTime = DateTime.Now,
            InAndOutBoundUser = input.AddName,
            InAndOutBoundRemake = input.InAndOutBoundRemake,
            CreateTime = DateTime.Now,
            StartPoint = input.StartPoint,
            EndPoint = input.EndPoint,
        };

        if (input.EndPoint != null)
        {
            string _storageRegionNum = GetStorageWhereRegion(input.EndPoint);
            string _regionNumWHnum = GetRegionWhereWHNum(_storageRegionNum);
            EG_WMS_InAndOutBoundDetail joindetail = new EG_WMS_InAndOutBoundDetail()
            {
                InAndOutBoundNum = joinboundnum,
                CreateTime = DateTime.Now,
                RegionNum = _storageRegionNum,
                StorageNum = input.EndPoint,
                WHNum = _regionNumWHnum,
                MaterielNum = input.MaterielNum
            };
            await _rep.InsertAsync(joinbound);
            await _InAndOutBoundDetail.InsertAsync(joindetail);
            return;
        }
        // 生成入库详单
        EG_WMS_InAndOutBoundDetail joinbounddetail = new EG_WMS_InAndOutBoundDetail()
        {
            InAndOutBoundNum = joinboundnum,
            CreateTime = DateTime.Now,
            RegionNum = null,
            StorageNum = null,
            WHNum = null,
            MaterielNum = input.MaterielNum
        };
        #endregion

        await _rep.InsertAsync(joinbound);
        await _InAndOutBoundDetail.InsertAsync(joinbounddetail);

    }

    /// <summary>
    /// 临时库存表操作
    /// </summary>
    /// <param name="MaterielProduct">宇翔料箱产品</param>
    /// <param name="StorageNum">目标点库位编号</param>
    /// <param name="joinboundnum">入库编号</param>
    /// <returns></returns>
    public async Task TemporaryInventoryStatementTask(List<MaterielProductDto> MaterielProduct, string StorageNum, string joinboundnum)
    {
        // 根据库位编号得到仓库编号和区域编号
        string _regionnum = GetStorageWhereRegion(StorageNum);
        string _whnum = GetRegionWhereWHNum(_regionnum);

        int sumcount = 0;
        for (int i = 0; i < MaterielProduct.Count; i++)
        {
            string inventorynum = $"{i}EGKC" + _TimeStamp.GetTheCurrentTimeTimeStamp();
            // 没有料箱编号
            //string workbinnum = MaterielProduct[i].WorkBinNum;
            string materienum = MaterielProduct[i].MaterielNum;
            int productcount = MaterielProduct[i].ProductCount;
            DateTime productiondate = MaterielProduct[i].ProductionDate;
            string productionlot = MaterielProduct[i].ProductionLot;
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
                ProductionLot = productionlot,
                CreateTime = DateTime.Now,
                StorageNum = StorageNum,
                RegionNum = _regionnum,
                WHNum = _whnum,
                IsDelete = false,
            };

            #region 宇翔项目没有料箱编号
            // 料箱表 将料箱内容保存到料箱表中

            //Entity.EG_WMS_WorkBin addWorkBin = new Entity.EG_WMS_WorkBin()
            //{
            //    WorkBinNum = workbinnum,
            //    ProductCount = productcount,
            //    ProductionLot = productionlot,
            //    CreateTime = DateTime.Now,
            //    ProductionDate = productiondate,
            //    WorkBinStatus = 0,
            //    MaterielNum = materienum,
            //    StorageNum = input.EndPoint,
            //    InAndOutBoundNum = joinboundnum,
            //};
            #endregion

            // 将数据保存到临时表中
            await _InventoryTem.InsertAsync(addInventory);
            await _InventoryDetailTem.InsertAsync(addInventoryDetail);

            // 得到每个料箱编号
            //if (list.Count > 1)
            //{
            //    wbnum = workbinnum + "," + wbnum;
            //}
            //else
            //{
            //    wbnum = workbinnum;
            //}
            //NeedWorkBinAllData.WorkBinNum = wbnum;
        }
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
    public async Task NotStorageAddStagingTask(AgvJoinBoundNewDto input, string boundNum)
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
                    InAndOutBoundNum = boundNum,
                    InAndOutBoundType = 0,
                    InAndOutBoundTime = DateTime.Now,
                    InAndOutBoundUser = input.AddName,
                    InAndOutBoundRemake = input.InAndOutBoundRemake,
                    CreateTime = DateTime.Now,
                    StartPoint = input.StartPoint,
                    EndPoint = null,
                };

                // 生成入库详单

                EG_WMS_InAndOutBoundDetail joinbounddetail = new EG_WMS_InAndOutBoundDetail()
                {
                    InAndOutBoundNum = boundNum,
                    CreateTime = DateTime.Now,
                    RegionNum = null,
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
    public async Task AddWorkBinDataList(List<MaterielProductDto> input, string boundNum)
    {

        string wbnum = "";
        int sumcount = 0;
        for (int i = 0; i < input.Count; i++)
        {
            // 库存编号（主表和详细表）
            string inventorynum = $"{i}EGKC" + _TimeStamp.GetTheCurrentTimeTimeStamp();
            // 物料编号（主表）
            string materienum = input[i].MaterielNum;
            // 物料的数量（主表、料箱表）
            int productcount = input[i].ProductCount;
            // 生产日期（料箱表）
            DateTime productiondate = input[i].ProductionDate;
            // 生产批次（详细表、料箱表）
            string productionlot = input[i].ProductionLot;
            // 总数
            sumcount += productcount;

            // 临时库存主表
            EG_WMS_Tem_Inventory addInventory = new EG_WMS_Tem_Inventory()
            {
                InventoryNum = inventorynum,
                MaterielNum = materienum,
                ICountAll = productcount,
                CreateTime = DateTime.Now,
                InBoundNum = boundNum,
                IsDelete = false,
                OutboundStatus = 0,
                ProductionDate = productiondate,
            };

            // 临时详细表
            EG_WMS_Tem_InventoryDetail addInventoryDetail = new EG_WMS_Tem_InventoryDetail()
            {
                InventoryNum = inventorynum,
                ProductionLot = productionlot,
                CreateTime = DateTime.Now,
                StorageNum = null,
                RegionNum = null,
                WHNum = null,
                IsDelete = false,
            };

            // 将数据保存到临时表中
            await _InventoryTem.InsertAsync(addInventory);
            await _InventoryDetailTem.InsertAsync(addInventoryDetail);
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
                                 MaterielNum = input[0].MaterielNum,

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