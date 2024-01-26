using Microsoft.AspNetCore.Authorization;

namespace Admin.NET.Application.Service.EG_WMS_BaseServer;

/// <summary>
/// 基础实用接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class BaseService : IDynamicApiController, ITransient
{
    // TODO：
    // 修改接口，完善代码结构，以及项目结构

    #region 关系注入
    EG_WMS_InAndOutBoundMessage boundMessage = new EG_WMS_InAndOutBoundMessage();
    private readonly SqlSugarRepository<TaskEntity> _TaskEntity = App.GetService<SqlSugarRepository<TaskEntity>>();
    private readonly SqlSugarRepository<TaskDetailEntity> _TaskDetailEntity = App.GetService<SqlSugarRepository<TaskDetailEntity>>();
    private readonly SqlSugarRepository<Entity.EG_WMS_InAndOutBound> _InAndOutBound = App.GetService<SqlSugarRepository<Entity.EG_WMS_InAndOutBound>>();
    private readonly SqlSugarRepository<EG_WMS_Tem_Inventory> _TemInventory = App.GetService<SqlSugarRepository<EG_WMS_Tem_Inventory>>();
    private readonly SqlSugarRepository<EG_WMS_Tem_InventoryDetail> _TemInventoryDetail = App.GetService<SqlSugarRepository<EG_WMS_Tem_InventoryDetail>>();
    private readonly SqlSugarRepository<Entity.EG_WMS_Inventory> _Inventory = App.GetService<SqlSugarRepository<Entity.EG_WMS_Inventory>>();
    private readonly SqlSugarRepository<EG_WMS_InAndOutBoundDetail> _InAndOutBoundDetail = App.GetService<SqlSugarRepository<EG_WMS_InAndOutBoundDetail>>();
    private readonly SqlSugarRepository<Entity.EG_WMS_Region> _Region = App.GetService<SqlSugarRepository<Entity.EG_WMS_Region>>();
    private readonly SqlSugarRepository<Entity.EG_WMS_Storage> _Storage = App.GetService<SqlSugarRepository<Entity.EG_WMS_Storage>>();
    private readonly SqlSugarRepository<Entity.EG_WMS_WorkBin> _WorkBin = App.GetService<SqlSugarRepository<Entity.EG_WMS_WorkBin>>();

    #endregion

    #region 构造函数
    public BaseService()
    {

    }

    #endregion

    #region 得到所有的库存信息以及关联关系

    /// <summary>
    /// 得到所有的库存信息以及关联关系（分页查询）
    /// 修改
    /// </summary>
    /// <param name="page">分页页数</param>
    /// <param name="pageSize">每页容量</param>
    /// <returns></returns>

    [HttpPost]
    [ApiDescriptionSettings(Name = "GetAllInventoryMessage")]
    public async Task<SqlSugarPagedList<GetAllInventoryData>> GetAllInventoryMessage(int page, int pageSize)
    {
        var data = _Inventory.AsQueryable()
                    .InnerJoin<EG_WMS_InventoryDetail>((o, cus) => o.InventoryNum == cus.InventoryNum)
                    .InnerJoin<Entity.EG_WMS_Materiel>((o, cus, ml) => ml.MaterielNum == o.MaterielNum)
                    .InnerJoin<Entity.EG_WMS_Storage>((o, cus, ml, age) => cus.StorageNum == age.StorageNum)
                    .InnerJoin<Entity.EG_WMS_Region>((o, cus, ml, age, ion) => age.RegionNum == ion.RegionNum)
                    .InnerJoin<Entity.EG_WMS_WareHouse>((o, cus, ml, age, ion, wh) => ion.WHNum == wh.WHNum)
                    .InnerJoin<Entity.EG_WMS_WorkBin>((o, cus, ml, age, ion, wh, wb) => cus.WorkBinNum == wb.WorkBinNum)
                    .OrderBy(o => o.Id)
                    .Select((o, cus, ml, age, ion, wh, wb) => new GetAllInventoryData
                    {
                        InventoryNum = o.InventoryNum,
                        WHName = wh.WHName,
                        MaterielNum = o.MaterielNum,
                        MaterielName = ml.MaterielName,
                        MaterielSpecs = ml.MaterielSpecs,
                        MaterielType = ml.MaterielType,
                        ICountAll = o.ICountAll,
                        IUsable = o.IUsable,
                        IWaitingCount = o.IWaitingCount,
                        IFrostCount = o.IFrostCount,
                        StorageName = age.StorageName,
                        WorkBinName = wb.WorkBinName,
                        RegionName = ion.RegionName,
                        OutboundStatus = o.OutboundStatus
                    });

        return await data.ToPagedListAsync(page, pageSize);

    }

    #endregion

    #region 查询得到出入库详情表

    /// <summary>
    /// 查询得到出入库详情表
    /// </summary>
    /// <param name="inandoutbound">出入库编号</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "GetAllInAndBoundDetailMessage")]
    public List<GetAllInAndBoundDetailData> GetAllInAndBoundDetailMessage(string inandoutbound)
    {

        List<GetAllInAndBoundDetailData> data = _InAndOutBound.AsQueryable()
                      .InnerJoin<EG_WMS_InAndOutBoundDetail>((a, b) => a.InAndOutBoundNum == b.InAndOutBoundNum)
                      .InnerJoin<EG_WMS_Tem_Inventory>((a, b, c) => c.InBoundNum == b.InAndOutBoundNum)
                      .InnerJoin<Entity.EG_WMS_Materiel>((a, b, c, d) => d.MaterielNum == c.MaterielNum)
                      .InnerJoin<EG_WMS_Tem_InventoryDetail>((a, b, c, d, e) => e.InventoryNum == c.InventoryNum)
                      .InnerJoin<Entity.EG_WMS_WorkBin>((a, b, c, d, e, f) => e.WorkBinNum == f.WorkBinNum)
                      .InnerJoin<Entity.EG_WMS_Region>((a, b, c, d, e, f, g) => g.RegionNum == e.RegionNum)
                      .InnerJoin<Entity.EG_WMS_WareHouse>((a, b, c, d, e, f, g, h) => h.WHNum == e.WHNum)
                      .InnerJoin<Entity.EG_WMS_Storage>((a, b, c, d, e, f, g, h, i) => i.StorageNum == e.StorageNum)
                      .WhereIF(inandoutbound.Substring(0, 4) == "EGRK", (a, b, c, d, e, f, g, h, i) => c.InBoundNum == inandoutbound)
                      .WhereIF(inandoutbound.Substring(0, 4) == "EGCK", (a, b, c, d, e, f, g, h, i) => c.OutBoundNum == inandoutbound)
                      .Select((a, b, c, d, e, f, g, h, i) => new GetAllInAndBoundDetailData
                      {
                          MaterielNum = d.MaterielNum,
                          MaterielName = d.MaterielName,
                          MaterieSpecs = d.MaterielSpecs,
                          ICountAll = (int)c.ICountAll,
                          WorkBinNum = f.WorkBinNum,
                          WorkBinName = f.WorkBinName,
                          WHName = h.WHName,
                          RegionName = g.RegionName,
                          StorageName = i.StorageName,
                          InAndOutBoundUser = a.InAndOutBoundUser,
                      })
                      .Distinct()
                      .ToList();


        return data;

    }

    #endregion

    #region 根据物料筛选条件查询物料的总数

    /// <summary>
    /// 根据物料筛选条件查询物料的总数
    /// </summary>
    /// <param name="input">查询条件</param>
    /// <returns></returns>

    [HttpPost]
    [ApiDescriptionSettings(Name = "MaterialAccorDingSumCount")]
    public async Task<SqlSugarPagedList<MaterielDataSumDto>> MaterialAccorDingSumCount(MaterialSelectSumCountBO input)
    {
        var data = _Inventory.AsQueryable()
                   .InnerJoin<Entity.EG_WMS_Materiel>((inv, mat) => inv.MaterielNum == mat.MaterielNum)
                   .WhereIF(!string.IsNullOrEmpty(input.materielNum), (inv, mat) => mat.MaterielNum == input.materielNum)
                   .WhereIF(!string.IsNullOrEmpty(input.materielName), (inv, mat) => mat.MaterielName.Contains(input.materielName.Trim()))
                   .WhereIF(!string.IsNullOrEmpty(input.materielType), (inv, mat) => mat.MaterielType.Contains(input.materielType.Trim()))
                   .WhereIF(!string.IsNullOrEmpty(input.materielSpecs), (inv, mat) => mat.MaterielSpecs.Contains(input.materielSpecs.Trim()))
                   .Where((inv, mat) => inv.OutboundStatus == 0 && inv.IsDelete == false)
                   .GroupBy((inv, mat) => inv.MaterielNum)
                   .Select((inv, mat) => new MaterielDataSumDto
                   {
                       MaterielNum = inv.MaterielNum,
                       MaterielName = mat.MaterielName,
                       MaterielType = mat.MaterielType,
                       MaterielSpecs = mat.MaterielSpecs,
                       MaterielMainUnit = mat.MaterielMainUnit,
                       MaterielAssistUnit = mat.MaterielAssistUnit,
                       SumCount = (int)SqlFunc.AggregateSum(inv.ICountAll)
                   });

        return await data.ToPagedListAsync(input.page, input.pageSize);

    }
    #endregion 

    #region 根据物料编号得到这条物料编号所有的库存记录

    /// <summary>
    /// 根据物料编号得到这条物料编号所有的库存记录
    /// </summary>
    /// <param name="MaterielNum">物料编号</param>
    /// <param name="page">页数</param>
    /// <param name="pageSize">每页容量</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "GetMaterileNumAllInventoryRecords")]
    public async Task<SqlSugarPagedList<GetMaterielNumDataList>> GetMaterileNumAllInventoryRecords(string MaterielNum, int page, int pageSize)
    {
        var data = _Inventory.AsQueryable()
                   .InnerJoin<EG_WMS_InventoryDetail>((inv, invd) => inv.InventoryNum == invd.InventoryNum)
                   .InnerJoin<Entity.EG_WMS_Materiel>((inv, invd, ma) => inv.MaterielNum == ma.MaterielNum)
                   .InnerJoin<Entity.EG_WMS_Region>((inv, invd, ma, re) => invd.RegionNum == re.RegionNum)
                   .InnerJoin<Entity.EG_WMS_WareHouse>((inv, invd, ma, re, wh) => invd.WHNum == wh.WHNum)
                   .Where((inv, invd) => inv.MaterielNum == MaterielNum && inv.OutboundStatus == 0 && inv.IsDelete == false)
                   .Select<GetMaterielNumDataList>();

        return await data.ToPagedListAsync(page, pageSize);

    }



    #endregion

    #region 展示有库存的库位信息

    /// <summary>
    /// 展示有库存的库位信息
    /// </summary>
    /// <param name="page">页数</param>
    /// <param name="pageSize">每页容量</param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "StorageExistInventory"), HttpPost]
    public async Task<SqlSugarPagedList<SelectExistInventoryDto>> StorageExistInventory(int page, int pageSize)
    {
        var data = _Storage.AsQueryable()
                                .Where(x => x.StorageOccupy == 1 && x.StorageStatus == 0)
                                .Select(x => new SelectExistInventoryDto
                                {
                                    StorageNum = x.StorageNum,
                                    StorageName = x.StorageName,
                                    ShelfNum = (int)x.ShelfNum,
                                    RoadwayNum = (int)x.RoadwayNum,
                                    FloorNumber = (int)x.FloorNumber,
                                });

        return await data.ToPagedListAsync(page, pageSize);

    }

    #endregion

    #region 根据库存信息，物料数据在哪几个立库库位上

    /// <summary>
    /// 根据库存信息，物料数据在哪几个立库库位上
    /// </summary>
    /// <param name="materielNum">物料编号</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页容量</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AccordingTheInventorySetUpStorageNum")]
    public async Task<SqlSugarPagedList<AccorDingSetUpStorageDto>> AccordingTheInventorySetUpStorageNum(string materielNum, int page, int pageSize)
    {
        var data = await _Inventory.AsQueryable()
                   .InnerJoin<EG_WMS_InventoryDetail>((a, b) => a.InventoryNum == b.InventoryNum)
                   .InnerJoin<Entity.EG_WMS_Storage>((a, b, c) => b.StorageNum == c.StorageNum)
                   .Where((a, b, c) => a.MaterielNum == materielNum && a.OutboundStatus == 0 && c.StorageType == 1)
                   .Select((a, b, c) => new AccorDingSetUpStorageDto
                   {
                       StorageNum = b.StorageNum,
                       StorageName = c.StorageName,
                       MaterielNum = a.MaterielNum,
                       CountSum = (int)a.ICountAll,

                   })
                   .ToListAsync();

        return data.ToPagedListAsync(page, pageSize);

    }


    #endregion

    #region 每月的入库数量

    /// <summary>
    /// 每月的入库数量
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "MonthlyInventoryQuantity")]
    public List<MonthlyInventoryQuantityDto> MonthlyInventoryQuantity()
    {
        //SELECT DATE_FORMAT(InAndOutBoundTime,'%Y-%m') AS 月份,SUM(InAndOutBoundCount) AS 总数 FROM eg_wms_inandoutbound
        //WHERE InAndOutBoundType = 0 AND InAndOutBoundStatus = 1
        //GROUP BY DATE_FORMAT(InAndOutBoundTime, '%Y-%m')

        return _InAndOutBound.AsQueryable()
                     .Where(x => x.InAndOutBoundStatus == 1 && x.InAndOutBoundType == 0 && x.SuccessOrNot == 0)
                     .GroupBy(x => SqlFunc.MappingColumn(x.InAndOutBoundTime.Value.ToString(), "DATE_FORMAT(InAndOutBoundTime, '%Y-%m')"))
                     .Select(x => new MonthlyInventoryQuantityDto
                     {
                         Mouth = SqlFunc.MappingColumn(x.InAndOutBoundTime.Value.ToString(), "DATE_FORMAT(InAndOutBoundTime, '%Y-%m')"),
                         CountSum = (int)SqlFunc.AggregateSum(x.InAndOutBoundCount)
                     })
                     .ToList();

    }


    #endregion

    #region 每月的出库数量

    /// <summary>
    /// 每月的出库数量
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "MonthlyOutboundQuantity")]
    public List<MonthlyInventoryQuantityDto> MonthlyOutboundQuantity()
    {

        return _InAndOutBound.AsQueryable()
             .Where(x => x.InAndOutBoundStatus == 3 && x.InAndOutBoundType == 1 && x.SuccessOrNot == 0)
             .GroupBy(x => SqlFunc.MappingColumn(x.InAndOutBoundTime.Value.ToString(), "DATE_FORMAT(InAndOutBoundTime, '%Y-%m')"))
             .Select(x => new MonthlyInventoryQuantityDto
             {
                 Mouth = SqlFunc.MappingColumn(x.InAndOutBoundTime.Value.ToString(), "DATE_FORMAT(InAndOutBoundTime, '%Y-%m')"),
                 CountSum = (int)SqlFunc.AggregateSum(x.InAndOutBoundCount)
             })
             .ToList();

    }


    #endregion

    #region 所有已占用库位数量

    /// <summary>
    /// 所有已占用库位数量
    /// </summary>
    /// <returns></returns>
    public int GetAllOccupiedStorage()
    {
        return _Storage.AsQueryable()
                       .Where(x => x.StorageOccupy == 1 && x.IsDelete == false && x.StorageStatus == 0)
                       .Count();
    }

    #endregion

    #region 所有未占用库位数量

    /// <summary>
    /// 所有未占用库位数量
    /// </summary>
    /// <returns></returns>
    public int GetAllUnoccupiedStorage()
    {
        return _Storage.AsQueryable()
                       .Where(x => x.StorageOccupy == 0 && x.IsDelete == false && x.StorageStatus == 0)
                       .Count();
    }

    #endregion

    #region 根据在库库存查询物料

    /// <summary>
    /// 根据在库库存查询物料
    /// </summary>
    /// <returns></returns>

    [HttpGet]
    [ApiDescriptionSettings(Name = "QueryMaterialsAccordingInventory")]
    public List<AccordingInventoryDto> QueryMaterialsAccordingInventory()
    {
        return _Inventory.AsQueryable()
           .InnerJoin<Entity.EG_WMS_Materiel>((a, b) => a.MaterielNum == b.MaterielNum)
           .Where(a => a.OutboundStatus == 0 && a.IsDelete == false)
           .Distinct()
           .Select((a, b) => new AccordingInventoryDto
           {
               MaterielNum = a.MaterielNum,
               MaterielName = b.MaterielName,
               MaterielType = b.MaterielType,
               MaterielMainUnit = b.MaterielMainUnit,
               MaterielSpecs = b.MaterielSpecs,
           })
           .ToList();

    }

    #endregion

    //-------------------------------------/策略/-------------------------------------//

    #region （策略）（密集库）AGV入库WMS自动推荐的库位（优先最靠里的库位）

    /// <summary>
    /// （策略）（密集库）AGV入库WMS自动推荐的库位
    /// </summary>
    /// <param name="materielNum">物料编号</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AGVStrategyReturnRecommEndStorage", Order = 1000)]
    public string AGVStrategyReturnRecommEndStorage(string materielNum)
    {
        // 根据物料编号，得到这个物料属于那个区域
        var dataRegion = _Region.AsQueryable().Where(x => x.RegionMaterielNum == materielNum).ToList();

        if (dataRegion == null || dataRegion.Count == 0)
        {
            throw Oops.Oh("区域未绑定物料");
        }

        for (int k = 0; k < dataRegion.Count; k++)
        {

            #region 用于新区域时，第一次入库推荐使用（可能需要修改）

            // 一开始初始化数据，第一个开始
            var data = _Storage.AsQueryable()
                     .Where(x => x.RegionNum == dataRegion[k].RegionNum && x.StorageOccupy == 0 && x.StorageStatus == 0)
                     .OrderBy(x => x.StorageNum, OrderByType.Desc)
                     .Select(x => new
                     {
                         x.StorageNum
                     })
                     .ToList();

            // 区域库位总数
            int datacount = _Storage.AsQueryable()
                        .Where(x => x.RegionNum == dataRegion[k].RegionNum)
                        .Count();

            if (data.Count == datacount)
            {
                return data[0].StorageNum.ToString();
            }

            #endregion


            // 查询是否有正在进行中的任务库位的组别

            var dataStorageGroup = _Storage.AsQueryable()
                       .Where(a => a.TaskNo != null && a.RegionNum == dataRegion[k].RegionNum)
                       .Distinct()
                       .Select(a => new
                       {
                           a.StorageGroup,
                       })
                       .ToList();

            // 将有任务的组别保存
            string[] strings = new string[dataStorageGroup.Count];

            for (int i = 0; i < dataStorageGroup.Count; i++)
            {
                strings[i] = dataStorageGroup[i].StorageGroup;
            }

            // 查询库位并且排除不符合条件的组别和库位

            var getStorage = _Storage.AsQueryable()
                     .Where(a => a.StorageStatus == 0 && a.StorageGroup != null
                     && a.StorageOccupy == 0 && a.RegionNum == dataRegion[k].RegionNum && !strings.Contains(a.StorageGroup))
                     .OrderBy(a => a.StorageNum, OrderByType.Desc)
                     .Select(a => new
                     {
                         a.StorageNum,
                         a.StorageGroup,
                     })
                     .ToList();


            // 得到组别
            var getStorageGroup = _Storage.AsQueryable()
                                 .Where(a => a.StorageStatus == 0 && a.StorageGroup != null
                                 && a.StorageOccupy == 0 && a.RegionNum == dataRegion[k].RegionNum && !strings.Contains(a.StorageGroup))
                                 .OrderBy(a => a.StorageNum, OrderByType.Desc)
                                 .Distinct()
                                 .Select(a => new
                                 {
                                     a.StorageGroup,
                                 })
                                 .ToList();


            for (int i = 0; i < getStorageGroup.Count; i++)
            {
                // 查询得到当前组已经占用的库位

                var AlreadyOccupied = _Storage.AsQueryable()
                         .Where(x => x.StorageGroup == getStorageGroup[i].StorageGroup && x.StorageOccupy == 1)
                         .Select(it => new
                         {
                             it.StorageNum,
                         })
                         .ToList();

                // 如果当前组没有占用的库位

                if (AlreadyOccupied.Count == 0)
                {
                    var datalist = _Storage.AsQueryable()
                         .Where(x => x.StorageGroup == getStorageGroup[i].StorageGroup && x.StorageStatus == 0)
                         // TODO、没有验证
                         .OrderBy(x => x.StorageNum, OrderByType.Desc)
                         .Select(it => new
                         {
                             it.StorageNum,
                         })
                         .ToList();

                    return datalist[0].StorageNum;
                }

                // 如果在确实有占用的库位
                if (AlreadyOccupied.Count != 0)
                {
                    // 查询得到当前组别下最末尾一个有占用的库位
                    var allStorageOccupy = _Storage.AsQueryable()
                             .Where(x => x.StorageOccupy == 1 && x.StorageGroup == getStorageGroup[i].StorageGroup)
                             .OrderBy(a => a.StorageNum, OrderByType.Desc)
                             .Select(a => new
                             {
                                 a.StorageNum,
                                 a.StorageGroup,
                             })
                             .ToList()
                             .Last();

                    // 得到这个组别下所有的未占用库位
                    var GetGroupOccupyNot = _Storage.AsQueryable()
                                  .Where(x => x.StorageOccupy == 0 && x.StorageStatus == 0 &&
                                         x.StorageGroup == allStorageOccupy.StorageGroup)
                                  .OrderBy(x => x.StorageNum, OrderByType.Desc)
                                  .Select(it => new
                                  {
                                      it.StorageNum,
                                  })
                                  .ToList();

                    // 依次判断符合条件的数据
                    // 当前组别最后一个被占用的库位编号
                    int lastOccupyNum = allStorageOccupy.StorageNum.ToInt();

                    for (int j = 0; j < GetGroupOccupyNot.Count; j++)
                    {
                        if (GetGroupOccupyNot[j].StorageNum.ToInt() < lastOccupyNum)
                        {
                            return GetGroupOccupyNot[j].StorageNum;
                        }
                    }
                }
                else
                {
                    //throw Oops.Oh("没有合适的库位");
                    return "没有合适的库位";

                }
            }
        }

        // 如果没有则返回错误
        return "没有合适的库位";

        //throw Oops.Oh("没有合适的库位");
    }

    #endregion

    #region （策略）（密集库）AGV出库WMS自动推荐的库位（判断生产日期）

    /// <summary>
    /// （策略）（密集库）AGV出库WMS自动推荐的库位（判断生产日期）
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AGVStrategyReturnRecommendStorageOutBoundJudgeTime", Order = 999)]
    public string AGVStrategyReturnRecommendStorageOutBoundJudgeTime(string materielNum)
    {

        // 根据物料编号，得到这个物料属于那个区域
        var dataRegion = _Region.AsQueryable().Where(x => x.RegionMaterielNum == materielNum).ToList();
        // 用于保存每个区域里面的数据
        List<string> datastring = new List<string>();
        if (dataRegion == null || dataRegion.Count == 0)
        {
            throw Oops.Oh("区域未绑定物料");
        }
        for (int k = 0; k < dataRegion.Count; k++)
        {
            // 查询是否有正在进行中的任务库位的组别

            var dataStorageGroup = _Storage.AsQueryable()
                       .Where(a => a.TaskNo != null && a.RegionNum == dataRegion[k].RegionNum)
                       .Distinct()
                       .Select(a => new
                       {
                           a.StorageGroup,
                       })
                       .ToList();

            // 将有任务的组别保存
            string[] strings = new string[dataStorageGroup.Count];
            for (int i = 0; i < dataStorageGroup.Count; i++)
            {
                strings[i] = dataStorageGroup[i].StorageGroup;
            }

            // 查询所有的组别（排除不符合条件的组别）
            var getGroup = _Storage.AsQueryable()
                            .Where(x => !strings.Contains(x.StorageGroup) && x.StorageType == 0 &&
                                   x.StorageGroup != null && x.RegionNum == dataRegion[k].RegionNum)
                            .Distinct()
                            .Select(x => x.StorageGroup)
                            .ToList();

            // 如果这一组的最后一个的时间还没有达到，则这一组都用不了 
            string[] stringss = new string[getGroup.Count];
            var storagenum = new Entity.EG_WMS_Storage();
            for (int i = 0; i < getGroup.Count; i++)
            {
                var notStorageGroup = _Storage.AsQueryable()
                                     .Where(x => x.StorageGroup == getGroup[i] && x.StorageOccupy == 1)
                                     .OrderBy(x => x.StorageNum, OrderByType.Desc)
                                     .ToList();

                if (notStorageGroup.Count != 0)
                {
                    storagenum = notStorageGroup.Last();
                }

                // 这个组的最后一条数据不符合条件，把这个组别保存下来
                if (storagenum.StorageProductionDate.ToDateTime().AddHours(48) > DateTime.Now)
                {
                    stringss[i] = storagenum.StorageGroup.ToString();
                }

            }

            // 查询库位并且排除不符合条件的组别和库位

            var getStorage = _Storage.AsQueryable()
                     .Where(a => a.StorageStatus == 0 && a.StorageGroup != null
                     && a.StorageOccupy == 1 && a.RegionNum == dataRegion[k].RegionNum &&
                     !strings.Contains(a.StorageGroup) && !stringss.Contains(a.StorageGroup) &&
                     a.StorageProductionDate.ToDateTime().AddHours(48) < DateTime.Now)
                     .OrderBy(a => a.StorageNum, OrderByType.Asc)
                     .Select(a => new
                     {
                         a.StorageNum,
                         a.StorageGroup,
                     })
                     .ToList();

            // 将每个区域里面符合条件的库位保存
            foreach (var item in getStorage)
            {
                datastring.Add(item.StorageNum);
            }

        }

        // 将得到的库位重新进行排序，让最小编号的库位在前面
        List<int> dataInt = new List<int>();
        foreach (string s in datastring)
        {
            dataInt.Add(int.Parse(s));
        }
        dataInt.Sort();
        if (dataInt == null || dataInt.Count == 0)
        {
            //throw Oops.Oh("没有合适的库位");
            return "没有合适的库位";
        }

        return dataInt[0].ToString();

    }


    #endregion

    #region （策略）（密集库）AGV出库WMS自动推荐的库位（不判断生产日期）

    /// <summary>
    /// （策略）（密集库）AGV出库WMS自动推荐的库位（不判断生产日期）
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AGVStrategyReturnRecommendStorageOutBound", Order = 998)]
    public string AGVStrategyReturnRecommendStorageOutBound(string materielNum)
    {
        // 根据物料编号，得到这个物料属于那个区域
        var dataRegion = _Region.AsQueryable().Where(x => x.RegionMaterielNum == materielNum).ToList();
        // 用于保存每个区域里面的数据
        List<string> datastring = new List<string>();

        if (dataRegion == null || dataRegion.Count == 0)
        {
            throw Oops.Oh("区域未绑定物料");
        }

        for (int k = 0; k < dataRegion.Count; k++)
        {
            // 查询是否有正在进行中的任务库位的组别

            var dataStorageGroup = _Storage.AsQueryable()
                       .Where(a => a.TaskNo != null && a.RegionNum == dataRegion[k].RegionNum)
                       .Distinct()
                       .Select(a => new
                       {
                           a.StorageGroup,
                       })
                       .ToList();

            // 将有任务的组别保存
            string[] strings = new string[dataStorageGroup.Count];
            for (int i = 0; i < dataStorageGroup.Count; i++)
            {
                strings[i] = dataStorageGroup[i].StorageGroup;
            }

            // 查询库位并且排除不符合条件的组别和库位

            var getStorage = _Storage.AsQueryable()
                     .Where(a => a.StorageStatus == 0 && a.StorageGroup != null
                     && a.StorageOccupy == 1 && a.RegionNum == dataRegion[k].RegionNum && !strings.Contains(a.StorageGroup))
                     .OrderBy(a => a.StorageNum, OrderByType.Asc)
                     .Select(a => new
                     {
                         a.StorageNum,
                     })
                     .ToList();

            // 将每个区域里面符合条件的库位保存

            foreach (var item in getStorage)
            {
                datastring.Add(item.StorageNum);
            }
        }
        // 将得到的库位重新进行排序，让最小编号的库位在前面

        List<int> dataInt = new List<int>();
        foreach (string s in datastring)
        {
            dataInt.Add(int.Parse(s));
        }
        dataInt.Sort();

        if (dataInt == null || dataInt.Count == 0)
        {
            return "没有合适的库位";
        }
        return dataInt[0].ToString();
    }


    #endregion

    #region （策略一）（立库）堆高车入库WMS自动推荐的库位（按照库位编号大小：从小到大依次开始推荐，不需要根据物料产品）

    /// <summary>
    /// （策略一）（立库）堆高车入库WMS自动推荐的库位（按照库位编号大小：从小到大依次开始推荐，不需要根据物料产品）
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AGVStacKingHighCarsIntoReturnStorage", Order = 997)]
    public string AGVStacKingHighCarsIntoReturnStorage()
    {

        var storagenum = _Storage.AsQueryable()
                 .Where(x => x.StorageType == 1 && x.StorageOccupy == 0 && x.StorageStatus == 0)
                 .OrderBy(x => x.StorageNum, OrderByType.Asc)
                 .ToList()
                 .First();

        if (storagenum == null)
        {
            return "当前没有合适的库位！";
        }
        return storagenum.StorageNum.ToString();

    }


    #endregion

    #region （策略二）（立库）堆高车入库WMS自动推荐的库位（按照库位编号大小：从小到大依次开始推荐，不需要根据物料产品）

    #endregion

    #region （策略）（立库）堆高车出库WMS自动推荐的库位（根据先入先出原则以及出库总数）

    /// <summary>
    /// （策略）（立库）堆高车出库WMS自动推荐的库位（根据先入先出原则以及出库总数）
    /// </summary>
    /// <param name="materielNum">物料编号</param>
    /// <param name="quantity">出库总数</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AGVStackingHighCarStorageOutBound", Order = 996)]
    public List<string> AGVStackingHighCarStorageOutBound(string materielNum, int quantity)
    {

        // 找到这个物料在库存中存在的
        var invdata = _Inventory.AsQueryable()
                                .InnerJoin<EG_WMS_InventoryDetail>((a, b) => a.InventoryNum == b.InventoryNum)
                                .InnerJoin<Entity.EG_WMS_Storage>((a, b, c) => b.StorageNum == c.StorageNum)
                                .Where((a, b, c) => a.MaterielNum == materielNum && a.OutboundStatus == 0 && a.IsDelete == false && c.StorageType == 1)
                                .OrderBy((a, b) => a.CreateTime, OrderByType.Asc) // 从小到大排序
                                .Select((a, b) => new { a.ICountAll, b.StorageNum })
                                .ToList();

        if (invdata.Count == 0 || invdata == null)
        {
            throw Oops.Oh("当前库存中没有存在该物料的库存信息！");
        }

        List<string> result = new List<string>();
        int? sumcount = 0;
        for (int i = 0; i < invdata.Count; i++)
        {
            // 第一种情况（输入的数量恰好等于或大于第一个库位上的数量）
            if (invdata[i].ICountAll >= quantity)
            {
                result.Clear();
                result.Add(invdata[i].StorageNum.ToString());
                break;
            }
            else if (invdata[i].ICountAll < quantity)
            {
                if (sumcount >= quantity)
                {
                    break;
                }
                sumcount += invdata[i].ICountAll;
                result.Add(invdata[i].StorageNum.ToString());
            }
            else if (i == invdata.Count - 1)
            {
                if (sumcount < quantity)
                {
                    throw Oops.Oh("当前在库中该物料数量不足出库数量！");
                }
            }
        }
        return result;
    }


    #endregion

    #region （策略）AGV请求任务点
    /// <summary>
    /// （策略）（密集库）AGV请求任务点（到达等待点获取）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    [UnifyProvider("easygreat")]
    [ApiDescriptionSettings(Name = "AGVRequestsTaskPoint", Order = 995)]
    public object AGVRequestsTaskPoint(GetPointDto input)
    {

        // 找到这个任务编号相同和模板编号相同的这条数据
        var taskData = _TaskEntity.GetFirst(x => x.TaskNo == input.orderId && x.ModelNo == input.modelProcessCode);
        if (taskData == null)
        {
            throw Oops.Oh("没有找到相同的任务编号或模板编号");
        }

        // 根据这个任务编号得到入库编号

        var inand = _InAndOutBoundDetail.GetFirst(x => x.InAndOutBoundNum == taskData.InAndOutBoundNum);

        // 得到这次任务的物料编号

        if (inand == null)
        {
            throw Oops.Oh("没有找到这个任务执行时的出入库编号");
        }

        string malnum = AGVStrategyReturnRecommEndStorage(inand.MaterielNum);

        if (malnum.Length.ToInt() != 8)
        {
            throw Oops.Oh("没有合适的库位");
        }

        string regionnum = boundMessage.GetStorageWhereRegion(malnum);
        string whnum = boundMessage.GetRegionWhereWHNum(regionnum);

        #region 根据得到的库位编号去修改入库的库位

        using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                // 修改agv任务表里面的库位点
                var _taskdata = _TaskEntity.GetFirst(x => x.TaskNo == input.orderId);

                string path = _taskdata.TaskPath;
                string[] result = path.Split(',');
                string taskpath = string.Join(",", result[0], result[1], malnum);

                _TaskEntity.AsUpdateable()
                           .SetColumns(it => new TaskEntity
                           {
                               TaskPath = taskpath,
                           })
                           .Where(x => x.TaskNo == input.orderId)
                           .ExecuteCommand();

                // 添加agv详情表里面的库位点
                TaskDetailEntity entity = new TaskDetailEntity();
                entity.TaskID = input.orderId.ToLong();
                entity.TaskPath = malnum;
                entity.CreateTime = DateTime.Now;

                _TaskDetailEntity.Insert(entity);

                // 得到入库的数据

                var temInvData = _TemInventory.AsQueryable()
                         .Where(x => x.InBoundNum == taskData.InAndOutBoundNum)
                         .Select(x => new
                         {
                             x.ProductionDate
                         })
                         .ToList();

                // 将生产日期保存
                List<DateTime?> temInvTime = new List<DateTime?>();
                for (int i = 0; i < temInvData.Count; i++)
                {
                    temInvTime.Add(temInvData[i].ProductionDate);
                }

                // 修改库位表中的状态为占用
                _Storage.AsUpdateable()
                         .AS("EG_WMS_Storage")
                         .SetColumns(it => new Entity.EG_WMS_Storage
                         {
                             // 预占用
                             StorageOccupy = 2,
                             TaskNo = taskData.TaskNo,
                             // 得到日期最大的生产日期
                             StorageProductionDate = temInvTime.Max(),
                             StorageNum = malnum,
                         })
                         .Where(x => x.StorageNum == malnum)
                         .ExecuteCommand();

                // 根据入库编号去修改库位

                _InAndOutBound.AsUpdateable()
                   .AS("EG_WMS_InAndOutBound")
                   .SetColumns(it => new Entity.EG_WMS_InAndOutBound
                   {
                       EndPoint = malnum,
                   })
                   .Where(u => u.InAndOutBoundNum == taskData.InAndOutBoundNum)
                   .ExecuteCommand();

                _InAndOutBoundDetail.AsUpdateable()
                    .AS("EG_WMS_InAndOutBoundDetail")
                    .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                    {
                        StorageNum = malnum,
                        RegionNum = regionnum,
                        WHNum = whnum,
                    })
                    .Where(u => u.InAndOutBoundNum == taskData.InAndOutBoundNum)
                    .ExecuteCommand();


                // 根据入库编号得到临时库位表里面的库存编号
                var dataTemList = _TemInventory.AsQueryable()
                               .Where(u => u.InBoundNum == taskData.InAndOutBoundNum)
                               .Select(x => x.InventoryNum)
                               .ToList();

                for (int i = 0; i < dataTemList.Count; i++)
                {
                    _TemInventoryDetail.AsUpdateable()
                                    .AS("EG_WMS_Tem_InventoryDetail")
                                    .SetColumns(it => new Entity.EG_WMS_Tem_InventoryDetail
                                    {
                                        StorageNum = malnum,
                                        RegionNum = regionnum,
                                        WHNum = whnum,
                                    })
                                    .Where(u => u.InventoryNum == dataTemList[i])
                                    .ExecuteCommand();
                }

                // 修改料箱的库位信息（宇翔没有料箱）
                //_WorkBin.AsUpdateable()
                //        .SetColumns(x => new Entity.EG_WMS_WorkBin
                //        {
                //            StorageNum = malnum,   
                //        })
                //        .Where(u => u.InAndOutBoundNum == taskData.InAndOutBoundNum)
                //        .ExecuteCommand();

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

        #endregion

        return new
        {
            PointName = malnum,
        };
    }

    #endregion

    #region （策略）（独立库位）



    #endregion

    #region 得到所有立库上面有料箱数据的

    /// <summary>
    /// 得到所有立库上面有料箱数据的
    /// </summary>
    /// <returns></returns>
    public List<string> GetWorkBinAllDatas()
    {

        return _Storage.AsQueryable()
                     .Where(x => x.StorageOccupy == 1 && x.StorageType == 1)
                     .Select(x => x.StorageNum)
                     .ToList();

    }


    #endregion

}

//-------------------------------------/归档/-------------------------------------//

#region （策略）（立库）堆高车出库WMS自动推荐的库位（按照先入先出，以及输入物料、物料数量去推荐哪几个库位）

///// <summary>
///// （策略）（立库）堆高车出库WMS自动推荐的库位（按照先入先出，以及输入物料、物料数量去推荐哪几个库位）
///// </summary>
///// <param name="materielnum">物料编号</param>
///// <param name="quantity">出库总数</param>
///// <returns></returns>
//[HttpPost]
//[ApiDescriptionSettings(Name = "AGVStackingHighCarStorageOutBound", Order = 996)]
//public List<string> AGVStackingHighCarStorageOutBound(string materielnum, int quantity)
//{
//    // 根据物料产品筛选物料在哪几个库位上

//    var storagenum = _Storage.AsQueryable()
//            .InnerJoin<EG_WMS_InventoryDetail>((a, b) => a.StorageNum == b.StorageNum)
//            .InnerJoin<EG_WMS_Inventory>((a, b, c) => b.InventoryNum == c.InventoryNum)
//            .Where((a, b, c) => a.StorageType == 1 && a.StorageOccupy == 1 && a.StorageStatus == 0 && c.MaterielNum == materielnum)
//            .OrderBy((a, b, c) => a.UpdateTime, OrderByType.Asc)
//            .Select((a, b, c) => new { a.StorageNum, c.ICountAll })
//            .ToList();

//    if (storagenum == null)
//    {
//        return new List<string> { "当前没有合适的库位！" };
//    }

//    List<string> result = new List<string>();
//    int? sumcount = 0;
//    for (int i = 0; i < storagenum.Count; i++)
//    {
//        // 第一种情况（输入的数量恰好等于或大于第一个库位上的数量）
//        if (storagenum[i].ICountAll >= quantity)
//        {
//            result.Clear();
//            result.Add(storagenum[i].StorageNum.ToString());
//            break;
//        }
//        else if (storagenum[i].ICountAll < quantity)
//        {
//            if (sumcount >= quantity)
//            {
//                break;
//            }
//            sumcount += storagenum[i].ICountAll;
//            result.Add(storagenum[i].StorageNum.ToString());
//        }
//        else if (i == storagenum.Count - 1)
//        {
//            if (sumcount < quantity)
//            {
//                return new List<string> { "当前在库中该物料数量不足出库数量！" };
//            }
//        }
//    }
//    return result;
//}


#endregion
