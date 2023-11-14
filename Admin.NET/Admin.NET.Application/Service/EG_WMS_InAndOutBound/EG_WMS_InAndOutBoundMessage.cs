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
                throw new Exception(ex.Message);
            }
        }

    }


    /// <summary>
    /// 修改库位表占用
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
    /// 入库操作
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
                InAndOutBoundNum = joinboundnum,
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
    /// <param name="regionnum"></param>
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