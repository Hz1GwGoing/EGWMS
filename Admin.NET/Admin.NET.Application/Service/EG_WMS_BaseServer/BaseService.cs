using Admin.NET.Application.Entity;
using System.Collections.Generic;

namespace Admin.NET.Application.Service.EG_WMS_BaseServer;

/// <summary>
/// 基础接口，获得所有数据
/// </summary>

[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class BaseService : IDynamicApiController, ITransient
{

    #region 引用实体

    /// <summary>
    /// 出入库主表
    /// </summary>
    private readonly SqlSugarRepository<Entity.EG_WMS_InAndOutBound> _InAndOutBound;
    /// <summary>
    /// 出入库详细表
    /// </summary>
    private readonly SqlSugarRepository<EG_WMS_InAndOutBoundDetail> _InAndOutBoundDetail;
    /// <summary>
    /// 库存表
    /// </summary>
    private readonly SqlSugarRepository<EG_WMS_Inventory> _Inventory;
    /// <summary>
    /// 库存详情表
    /// </summary>
    private readonly SqlSugarRepository<EG_WMS_InventoryDetail> _InventoryDetail;
    /// <summary>
    /// 库位表
    /// </summary>
    private readonly SqlSugarRepository<Entity.EG_WMS_Storage> _Storage;
    /// <summary>
    /// 区域表
    /// </summary>
    private readonly SqlSugarRepository<EG_WMS_Region> _Region;
    /// <summary>
    /// 料箱表
    /// </summary>
    private readonly SqlSugarRepository<EG_WMS_WorkBin> _workbin;
    /// <summary>
    /// 移库表
    /// </summary>
    private readonly SqlSugarRepository<EG_WMS_Relocation> _Relocation;


    #endregion

    #region 关系注入

    public BaseService
       (
       SqlSugarRepository<Entity.EG_WMS_InAndOutBound> InAndOutBound,
       SqlSugarRepository<EG_WMS_InAndOutBoundDetail> InAndOutBoundDetail,
       SqlSugarRepository<EG_WMS_Inventory> Inventory,
       SqlSugarRepository<EG_WMS_InventoryDetail> InventoryDetail,
       SqlSugarRepository<Entity.EG_WMS_Storage> storage,
       SqlSugarRepository<EG_WMS_Region> Region,
       SqlSugarRepository<EG_WMS_WorkBin> WorkBin,
       SqlSugarRepository<EG_WMS_Relocation> Relocation

       )
    {
        _InAndOutBound = InAndOutBound;
        _Inventory = Inventory;
        _InventoryDetail = InventoryDetail;
        _InAndOutBoundDetail = InAndOutBoundDetail;
        _Storage = storage;
        _Region = Region;
        _workbin = WorkBin;
        _Relocation = Relocation;
    }


    #endregion

    #region 得到所有的库存信息以及关联关系

    /// <summary>
    /// 得到所有的库存信息以及关联关系（分页查询）
    /// </summary>
    /// <param name="page">分页页数</param>
    /// <param name="pageSize">每页容量</param>
    /// <returns></returns>

    [HttpPost]
    [ApiDescriptionSettings(Name = "GetAllInventoryMessage")]
    public List<GetAllInventoryData> GetAllInventoryMessage(int page, int pageSize)
    {
        List<GetAllInventoryData> datas = _Inventory.AsQueryable()
                    .InnerJoin<EG_WMS_InventoryDetail>((o, cus) => o.InventoryNum == cus.InventoryNum)
                    .InnerJoin<EG_WMS_Materiel>((o, cus, ml) => ml.MaterielNum == o.MaterielNum)
                    .InnerJoin<Entity.EG_WMS_Storage>((o, cus, ml, age) => cus.StorageNum == age.StorageNum)
                    .InnerJoin<EG_WMS_Region>((o, cus, ml, age, ion) => age.RegionNum == ion.RegionNum)
                    .InnerJoin<EG_WMS_WareHouse>((o, cus, ml, age, ion, wh) => ion.WHNum == wh.WHNum)
                    .InnerJoin<EG_WMS_WorkBin>((o, cus, ml, age, ion, wh, wb) => cus.WorkBinNum == wb.WorkBinNum)
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
                    })
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();


        return datas;
    }

    #endregion

    #region 查询得到出入库详情表

    /// <summary>
    /// 查询得到出入库详情表
    /// </summary>
    /// <param name="inandoutbound">出入库编号</param>
    /// <param name="type">出入库（0 - 入库（默认） 1 - 出库 ）</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "GetAllInAndBoundDetailMessage")]
    public List<GetAllInAndBoundDetailData> GetAllInAndBoundDetailMessage(string inandoutbound, int type = 0)
    {

        List<GetAllInAndBoundDetailData> data = _InAndOutBound.AsQueryable()
                      .InnerJoin<EG_WMS_InAndOutBoundDetail>((a, b) => a.InAndOutBoundNum == b.InAndOutBoundNum)
                      .InnerJoin<EG_WMS_Inventory>((a, b, c) => c.InAndOutBoundNum == b.InAndOutBoundNum)
                      .InnerJoin<EG_WMS_Materiel>((a, b, c, d) => d.MaterielNum == c.MaterielNum)
                      .InnerJoin<EG_WMS_InventoryDetail>((a, b, c, d, e) => e.InventoryNum == c.InventoryNum)
                      .InnerJoin<EG_WMS_WorkBin>((a, b, c, d, e, f) => e.WorkBinNum == f.WorkBinNum)
                      .InnerJoin<EG_WMS_Region>((a, b, c, d, e, f, g) => g.RegionNum == e.RegionNum)
                      .InnerJoin<EG_WMS_WareHouse>((a, b, c, d, e, f, g, h) => h.WHNum == e.WHNum)
                      .InnerJoin<Entity.EG_WMS_Storage>((a, b, c, d, e, f, g, h, i) => i.StorageNum == e.StorageNum)
                      .Where((a, b, c, d, e, f, g, h, i) => a.InAndOutBoundType == type && a.InAndOutBoundNum == inandoutbound)
                      .OrderBy(a => a.Id)
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
                      })
                      .ToList();


        return data;

    }

    #endregion

    #region 根据物料编号查询物料的总数

    /// <summary>
    /// 根据物料编号查询物料的总数
    /// </summary>
    /// <param name="page">页数</param>
    /// <param name="pageSize">每页容量</param>
    /// <returns></returns>
    public List<MaterielDataSumDto> MaterialAccorDingSumCount(int page, int pageSize)
    {
        List<MaterielDataSumDto> data = _Inventory.AsQueryable()
                   .InnerJoin<EG_WMS_Materiel>((inv, mat) => inv.MaterielNum == mat.MaterielNum)
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
                   })
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

        return data;

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
    public List<GetMaterielNumDataList> GetMaterileNumAllInventoryRecords(string MaterielNum, int page, int pageSize)
    {
        var data = _Inventory.AsQueryable()
                   .InnerJoin<EG_WMS_InventoryDetail>((inv, invd) => inv.InventoryNum == invd.InventoryNum)
                   .InnerJoin<EG_WMS_Materiel>((inv, invd, ma) => inv.MaterielNum == ma.MaterielNum)
                   .InnerJoin<EG_WMS_Region>((inv, invd, ma, re) => invd.RegionNum == re.RegionNum)
                   .InnerJoin<EG_WMS_WareHouse>((inv, invd, ma, re, wh) => invd.WHNum == wh.WHNum)
                   .Where((inv, invd) => inv.MaterielNum == MaterielNum && inv.OutboundStatus == 0 && inv.IsDelete == false)
                   .Select<GetMaterielNumDataList>()
                   .Skip((page - 1) * pageSize)
                   .Take(pageSize)
                   .ToList();

        return data;

    }



    #endregion

    #region （策略）返回推荐的库位

    /// <summary>
    /// （策略）返回推荐的库位
    /// </summary>
    /// <param name="materielNum">物料编号</param>
    /// <returns></returns>
    public string StrategyReturnRecommendStorage(string materielNum)
    {
        // 根据物料编号，得到这个物料属于那个区域
        var dataRegion = _Region.GetFirst(x => x.RegionMaterielNum == materielNum);
        if (dataRegion == null)
        {
            throw Oops.Oh("区域未绑定物料");
        }

        // 查询是否有正在进行中的任务库位的组别

        var dataStorageGroup = _Storage.AsQueryable()
                   .Where(a => a.TaskNo != null && a.RegionNum == dataRegion.RegionNum)
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
                 .Where(a => a.StorageStatus == 0 && a.StorageOccupy == 0 && a.RegionNum == dataRegion.RegionNum && !strings.Contains(a.StorageGroup))
                 .OrderBy(a => a.StorageNum, OrderByType.Desc)
                 .Select(a => new
                 {
                     a.StorageNum,
                     a.StorageGroup,
                 })
                 .ToList();


        if (getStorage == null || getStorage.Count == 0)
        {
            throw Oops.Oh("没有合适的库位");
        }

        return getStorage[0].StorageNum;
    }

    #endregion

}
