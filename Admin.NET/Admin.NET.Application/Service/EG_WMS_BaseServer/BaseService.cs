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
    private readonly SqlSugarRepository<EG_WMS_Storage> _Storage;
    /// <summary>
    /// 区域表
    /// </summary>
    private readonly SqlSugarRepository<EG_WMS_Region> _Region;
    /// <summary>
    /// 料箱表
    /// </summary>
    private readonly SqlSugarRepository<EG_WMS_WorkBin> _workbin;

    private readonly SqlSugarRepository<EG_WMS_Relocation> _Relocation;


    #endregion

    #region 关系注入

    public BaseService
       (
       SqlSugarRepository<Entity.EG_WMS_InAndOutBound> InAndOutBound,
       SqlSugarRepository<EG_WMS_InAndOutBoundDetail> InAndOutBoundDetail,
       SqlSugarRepository<EG_WMS_Inventory> Inventory,
       SqlSugarRepository<EG_WMS_InventoryDetail> InventoryDetail,
       SqlSugarRepository<EG_WMS_Storage> storage,
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
    /// 得到所有的库存信息以及关联关系
    /// </summary>
    /// <returns></returns>

    [HttpGet]
    [ApiDescriptionSettings(Name = "GetAllInventoryMessage")]
    public List<GetAllInventoryData> GetAllInventoryMessage()
    {
        List<GetAllInventoryData> datas = _Inventory.AsQueryable()
                    .InnerJoin<EG_WMS_InventoryDetail>((o, cus) => o.InventoryNum == cus.InventoryNum)
                    .InnerJoin<EG_WMS_Materiel>((o, cus, ml) => ml.MaterielNum == o.MaterielNum)
                    .InnerJoin<EG_WMS_Storage>((o, cus, ml, age) => cus.StorageNum == age.StorageNum)
                    .InnerJoin<EG_WMS_Region>((o, cus, ml, age, ion) => age.RegionNum == ion.RegionNum)
                    .InnerJoin<EG_WMS_WareHouse>((o, cus, ml, age, ion, wh) => ion.WHNum == wh.WHNum)
                    .InnerJoin<EG_WMS_WorkBin>((o, cus, ml, age, ion, wh, wb) => cus.WorkBinNum == wb.WorkBinNum)
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
                    .ToList();



        return datas;
    }

    #endregion

    #region 查询得到出入库详情表

    /// <summary>
    /// 查询得到出入库详情表
    /// </summary>
    /// <param name="type">出入库（0 - 入库（默认） 1 - 出库 ）</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "GetAllInAndBoundDetailMessage")]
    public List<GetAllInAndBoundDetailData> GetAllInAndBoundDetailMessage(int type = 0)
    {
        
        return _InAndOutBound.AsQueryable()
                       .InnerJoin<EG_WMS_InAndOutBoundDetail>((a, b) => a.InAndOutBoundNum == b.InAndOutBoundNum)
                       .InnerJoin<EG_WMS_Inventory>((a, b, c) => c.InAndOutBoundNum == b.InAndOutBoundNum)
                       .InnerJoin<EG_WMS_Materiel>((a, b, c, d) => d.MaterielNum == c.MaterielNum)
                       .InnerJoin<EG_WMS_InventoryDetail>((a, b, c, d, e) => e.InventoryNum == c.InventoryNum)
                       .InnerJoin<EG_WMS_WorkBin>((a, b, c, d, e, f) => e.WorkBinNum == f.WorkBinNum)
                       .InnerJoin<EG_WMS_Region>((a, b, c, d, e, f, g) => g.RegionNum == e.RegionNum)
                       .InnerJoin<EG_WMS_WareHouse>((a, b, c, d, e, f, g, h) => h.WHNum == e.WHNum)
                       .InnerJoin<EG_WMS_Storage>((a, b, c, d, e, f, g, h, i) => i.StorageNum == e.StorageNum)
                       .Where(a => a.InAndOutBoundType == type)
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

    }

    #endregion

}
