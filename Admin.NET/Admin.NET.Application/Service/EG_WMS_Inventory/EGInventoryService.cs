﻿
namespace Admin.NET.Application;
/// <summary>
/// 库存主表接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGInventoryService : IDynamicApiController, ITransient
{
    #region 实体引用


    private readonly SqlSugarRepository<EG_WMS_Inventory> _rep;
    private readonly SqlSugarRepository<EG_WMS_Materiel> _materiel;
    private readonly SqlSugarRepository<EG_WMS_InAndOutBound> _inandoutbound;
    #endregion

    #region 关系注入

    public EGInventoryService
        (
        SqlSugarRepository<EG_WMS_Inventory> rep,
        SqlSugarRepository<EG_WMS_Materiel> materiel,
        SqlSugarRepository<EG_WMS_InAndOutBound> inandoutbound
        )
    {
        _rep = rep;
        _materiel = materiel;
        _inandoutbound = inandoutbound;
    }
    #endregion

    #region 当日的入库数量

    /// <summary>
    /// 当日的入库数量
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "DailyInBoundQuantity")]
    public async Task<List<DailyInOutBoundQuantityDto>> DailyInBoundQuantity()
    {

        //string sql = "SELECT DATE_FORMAT(InAndOutBoundTime, '%Y-%m-%d %H') as time, SUM(InAndOutBoundCount) " +
        //     "as count FROM eg_wms_inandoutbound WHERE InAndOutBoundTime >= CURDATE() AND InAndOutBoundTime<CURDATE() +INTERVAL 1 DAY GROUP BY time  ORDER BY  time";

        DateTime todayStart = DateTime.Today;
        DateTime todayEnd = todayStart.AddDays(1);
        return await _inandoutbound.AsQueryable()
                                 .Where(x => x.InAndOutBoundStatus == 1 && x.InAndOutBoundType == 0 && x.SuccessOrNot == 0)
                                 .Where(x => x.InAndOutBoundTime >= todayStart && x.InAndOutBoundTime < todayEnd)
                                 .GroupBy(x => SqlFunc.MappingColumn(x.InAndOutBoundTime.Value.ToString(), "DATE_FORMAT(InAndOutBoundTime, '%Y-%m-%d %H')"))
                                 .Select(x => new DailyInOutBoundQuantityDto
                                 {
                                     HourTime = SqlFunc.MappingColumn(x.InAndOutBoundTime.Value.ToString(), "DATE_FORMAT(InAndOutBoundTime, '%Y-%m-%d %H')"),
                                     SumCount = (int)SqlFunc.AggregateSum(x.InAndOutBoundCount)
                                 })
                                 .ToListAsync();




    }

    #endregion

    #region 当日的出库数量

    /// <summary>
    /// 当日的出库数量
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "DailyOutBoundQuantity")]
    public async Task<List<DailyInOutBoundQuantityDto>> DailyOutBoundQuantity()
    {

        DateTime todayStart = DateTime.Today;
        DateTime todayEnd = todayStart.AddDays(1);
        return await _inandoutbound.AsQueryable()
                                 .Where(x => x.InAndOutBoundStatus == 3 && x.InAndOutBoundType == 1 && x.SuccessOrNot == 0)
                                 .Where(x => x.InAndOutBoundTime >= todayStart && x.InAndOutBoundTime < todayEnd)
                                 .GroupBy(x => SqlFunc.MappingColumn(x.InAndOutBoundTime.Value.ToString(), "DATE_FORMAT(InAndOutBoundTime, '%Y-%m-%d %H')"))
                                 .Select(x => new DailyInOutBoundQuantityDto
                                 {
                                     HourTime = SqlFunc.MappingColumn(x.InAndOutBoundTime.Value.ToString(), "DATE_FORMAT(InAndOutBoundTime, '%Y-%m-%d %H')"),
                                     SumCount = (int)SqlFunc.AggregateSum(x.InAndOutBoundCount)
                                 })
                                 .ToListAsync();




    }

    #endregion

    #region 分页查询库存主表
    /// <summary>
    /// 分页查询库存主表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<EGInventoryOutput>> Page(EGInventoryInput input)
    {
        var query = _rep.AsQueryable()
            // 库存编号
            //.WhereIF(!string.IsNullOrWhiteSpace(input.InventoryNum), u => u.InventoryNum.Contains(input.InventoryNum.Trim()))
            .WhereIF(input.ICountAll > 0, u => u.ICountAll == input.ICountAll)
            .WhereIF(input.IUsable > 0, u => u.IUsable == input.IUsable)
            .WhereIF(input.IFrostCount > 0, u => u.IFrostCount == input.IFrostCount)
            .WhereIF(input.IWaitingCount > 0, u => u.IWaitingCount == input.IWaitingCount)
            // 物料编号
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), u => u.MaterielNum == input.MaterielNum.Trim())
            // 备注
            .WhereIF(!string.IsNullOrWhiteSpace(input.InventoryRemake), u => u.InventoryRemake == input.InventoryRemake.Trim())
            // 出库状态
            .WhereIF(input.OutboundStatus > 0, u => u.OutboundStatus == input.OutboundStatus)
            // 获取创建日期
            .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
            // 倒序
            .OrderBy(it => it.CreateTime, OrderByType.Desc)
            .Select<EGInventoryOutput>()
;
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }
    #endregion

    #region 更新库存主表
    /// <summary>
    /// 更新库存主表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGInventoryInput input)
    {
        var entity = input.Adapt<EG_WMS_Inventory>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }
    #endregion

    #region 模糊查询符条件的数据（根据物料名称、编号、类别）
    /// <summary>
    /// 模糊查询符条件的数据（根据物料名称、编号、类别）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<List<EGInventoryAndMaterielDto>> Detail(QueryByIdEGInventoryInput input)
    {
        List<string> materielNums = new List<string>();
        List<EGInventoryAndMaterielDto> EGInventoryAndMaterielData = new List<EGInventoryAndMaterielDto>();

        try
        {
            var materielsData = _materiel.AsQueryable()
                                         .Where(u => u.MaterielName.Contains(input.MaterielName) || u.MaterielNum.Contains(input.MaterielNum) || u.MaterielType.Contains(input.MaterielType))
                                         .ToList();


            if (materielsData.Count != 0)
            {
                // 得到模糊查询里面的物料编号  
                foreach (var item in materielsData)
                {
                    materielNums.Add(item.MaterielNum);
                }

                for (int i = 0; i < materielNums.Count; i++)
                {
                    string num = materielNums[i];

                    try
                    {
                        var materieItem = await _materiel.GetFirstAsync(u => u.MaterielNum == num);
                        var inventoryItem = await _rep.GetFirstAsync(u => u.MaterielNum == num);
                        // 物料名称  
                        string materielnum = materieItem.MaterielNum;
                        // 物料名称  
                        string materiename = materieItem.MaterielName;
                        // 规格  
                        string? materielspecs = materieItem.MaterielSpecs;
                        // 在库数量  
                        int? icountall = inventoryItem.ICountAll;
                        // 可用数量  
                        int? iusable = inventoryItem.IUsable;

                        EGInventoryAndMaterielDto inventory = new EGInventoryAndMaterielDto()
                        {
                            MaterielNum = materielnum,
                            MaterielName = materiename,
                            MaterielSpecs = materielspecs,
                            ICountAll = icountall,
                            IUsable = iusable,
                        };
                        EGInventoryAndMaterielData.Add(inventory);
                    }
                    catch (Exception ex)
                    {
                        throw Oops.Oh(ex.Message, ex);
                    }
                }
            }
            else
            {
                throw Oops.Oh("查询不到记录");
            }
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ex.Message);
        }
        return EGInventoryAndMaterielData;
    }

    #endregion

    #region 获取库存主表列表
    /// <summary>
    /// 获取库存主表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGInventoryOutput>> List([FromQuery] EGInventoryInput input)
    {
        return await _rep.AsQueryable().Select<EGInventoryOutput>().ToListAsync();
    }

    #endregion

    //-------------------------------------//-------------------------------------//

    #region 模糊查询（根据物料编号和物料名称和物料规格查询）
    //public async Task<List<EGInventoryAndMaterielDto>> Get([FromQuery] QueryByIdEGInventoryInput input)
    //{

    //    模糊查询（根据物料编号和物料名称和物料规格查询）
    //    List<string> materielNums = new List<string>();
    //    List<EGInventoryAndMaterielDto> EGInventoryAndMaterielData = new List<EGInventoryAndMaterielDto>();

    //    List<EGMateriel> materielsData = await _materiel.GetListAsync
    //        (
    //            u => u.MaterielNum.Contains(input.MaterielNum) ||
    //            u.MaterielName.Contains(input.MaterielName) ||
    //            u.MaterielSpecs.Contains(input.MaterielSpecs)
    //        );
    //    if (materielsData != null)
    //    {
    //        得到模糊查询里面的物料编号
    //        foreach (var item in materielsData)
    //        {
    //            materielNums.Add(item.MaterielNum);
    //        }

    //        for (int i = 0; i < materielNums.Count; i++)
    //        {
    //            string num = materielNums[i];

    //            var materieItem = await _materiel.GetFirstAsync(u => u.MaterielNum == num);
    //            var inventoryItem = await _rep.GetFirstAsync(u => u.MaterielNum == num);
    //            物料名称
    //           var materielnum = materieItem.MaterielNum;
    //            物料名称
    //           var materiename = materieItem.MaterielName;
    //            规格
    //           var materielspecs = materieItem.MaterielSpecs;
    //            在库数量
    //           var icountall = inventoryItem.ICountAll;
    //            可用数量
    //           var iusable = inventoryItem.IUsable;


    //            EGInventoryAndMaterielDto inventory = new EGInventoryAndMaterielDto()
    //            {
    //                MaterielNum = materielnum,
    //                MaterielName = materiename,
    //                MaterielSpecs = materielspecs,
    //                ICountAll = (int)icountall,
    //                IUsable = (int)iusable,

    //            };
    //            EGInventoryAndMaterielData.Add(inventory);
    //        }

    //    }
    //    return EGInventoryAndMaterielData;

    //}
    #endregion

}

