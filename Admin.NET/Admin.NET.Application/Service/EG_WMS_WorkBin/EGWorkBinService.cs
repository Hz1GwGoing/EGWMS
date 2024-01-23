using Nest;

namespace Admin.NET.Application;

/// <summary>
/// 料箱信息接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGWorkBinService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<EG_WMS_Inventory> _inv;
    private readonly SqlSugarRepository<EG_WMS_InventoryDetail> _invdetail;
    private readonly SqlSugarRepository<EG_WMS_WorkBin> _rep;
    public EGWorkBinService
    (SqlSugarRepository<EG_WMS_WorkBin> rep, SqlSugarRepository<EG_WMS_Inventory> inv, SqlSugarRepository<EG_WMS_InventoryDetail> invdetail)
    {
        _rep = rep;
        _inv = inv;
        _invdetail = invdetail;
    }

    #region 分页查询料箱信息

    /// <summary>
    /// 分页查询料箱信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<QueryWorkBinOccupancyDto>> Page(EGWorkBinInput input)
    {

        //SELECT DISTINCT
        //    WorkBinNum,
        //    CASE
        //        WHEN a.OutboundStatus = 0 THEN 'Occupied'
        //        WHEN a.OutboundStatus = 1 THEN 'Not Occupied'
        //        ELSE 'Unknown'
        //    END AS OccupancyStatus
        // FROM eg_wms_inventory AS a
        //INNER JOIN eg_wms_inventorydetail AS b ON a.InventoryNum = b.InventoryNum;


        return await _inv.AsQueryable()
                         .InnerJoin<EG_WMS_InventoryDetail>((a, b) => a.InventoryNum == b.InventoryNum)
                         .InnerJoin<EG_WMS_WorkBin>((a, b, c) => b.WorkBinNum == c.WorkBinNum)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinNum), (a, b, c) => c.WorkBinNum == input.WorkBinNum.Trim())
                         .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinName), (a, b, c) => c.WorkBinName.Contains(input.WorkBinName.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinSpecs), (a, b, c) => c.WorkBinSpecs.Contains(input.WorkBinSpecs.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.StorageNum), (a, b, c) => c.StorageNum.Contains(input.StorageNum.Trim()))
                         //// 获取创建日期
                         //.WhereIF(input.CreateTime > DateTime.MinValue, (a, b, c) =>)
                         .Distinct()
                         .OrderBy((a, b, c) => c.CreateTime, OrderByType.Desc) // 料箱时间倒序
                         .Select((a, b, c) => new QueryWorkBinOccupancyDto
                         {
                             WorkBinOccupancy = SqlFunc.IF(a.OutboundStatus == 0)
                                                       .Return<string>("已占用")
                                                       .ElseIF(a.OutboundStatus == 1)
                                                       .Return<string>("未占用")
                                                       .End("未知状态"),
                         }, true)
                         .ToPagedListAsync(input.Page, input.PageSize);




        //var query = _rep.AsQueryable()
        //            .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinNum), u => u.WorkBinNum == input.WorkBinNum.Trim())
        //            .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinName), u => u.WorkBinName.Contains(input.WorkBinName.Trim()))
        //            .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinSpecs), u => u.WorkBinSpecs.Contains(input.WorkBinSpecs.Trim()))
        //            .WhereIF(input.WorkBinStatus >= 0, u => u.WorkBinStatus == input.WorkBinStatus)
        //            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), u => u.MaterielNum == input.MaterielNum)
        //            .WhereIF(!string.IsNullOrWhiteSpace(input.StorageNum), u => u.StorageNum.Contains(input.StorageNum.Trim()))
        //            // 获取创建日期
        //            .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
        //            .Distinct()
        //            .Select();

        //if (input.ProductionDateRange != null && input.ProductionDateRange.Count > 0)
        //{
        //    DateTime? start = input.ProductionDateRange[0];
        //    query = query.WhereIF(start.HasValue, u => u.ProductionDate > start);
        //    if (input.ProductionDateRange.Count > 1 && input.ProductionDateRange[1].HasValue)
        //    {
        //        var end = input.ProductionDateRange[1].Value.AddDays(1);
        //        query = query.Where(u => u.ProductionDate < end);
        //    }
        //}
        //query = query.OrderBuilder(input);
        //return await query.ToPagedListAsync(input.Page, input.PageSize);
    }
    #endregion

    #region 增加料箱信息
    /// <summary>
    /// 增加料箱信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddEGWorkBinInput input)
    {
        int iscount = _rep.AsQueryable()
                         .Where(x => x.WorkBinNum == input.WorkBinNum && x.IsDelete == false)
                         .Count();

        if (iscount > 0)
        {
            throw Oops.Oh("当前已有该料箱，请勿重复添加！");
        }
        var entity = input.Adapt<EG_WMS_WorkBin>();
        await _rep.InsertAsync(entity);
    }
    #endregion

    #region 删除料箱信息
    /// <summary>
    /// 删除料箱信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteEGWorkBinInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
    }
    #endregion

    #region 更新料箱信息
    /// <summary>
    /// 更新料箱信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGWorkBinInput input)
    {
        var entity = input.Adapt<EG_WMS_WorkBin>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }
    #endregion

    #region 料箱回溯功能

    /// <summary>
    /// 料箱回溯功能
    /// </summary>
    /// <param name="workbinnum">料箱编号</param>
    /// <param name="page">页数</param>
    /// <param name="pagesize">每页容量</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "WorkBinBackTracKing")]
    public async Task<SqlSugarPagedList<WorkBinBacktrackingDto>> WorkBinBackTracKing(string workbinnum, int page, int pagesize)
    {
        // 避免转移错误
        string string1 = Uri.UnescapeDataString(workbinnum);

        #region SQL语句
        // SELECT DISTINCT a.WorkBinNum,c.MaterielNum,c.ICountAll,a.ProductionLot,b.StorageNum,c.OutboundStatus,
        // c.ProductionDate AS 生产时间,c.CreateTime AS 入库时间,c.UpdateTime AS 出库时间 FROM eg_wms_workbin AS a
        // INNER JOIN eg_wms_inventorydetail AS b
        // ON a.WorkBinNum = b.WorkBinNum
        // INNER JOIN eg_wms_inventory AS c
        // ON b.InventoryNum = c.InventoryNum
        // 判断条件
        // WHERE a.WorkBinNum = "1/22wb3"
        // ORDER BY c.CreateTime DESC

        #endregion

        var data = _rep.AsQueryable()
                       .InnerJoin<EG_WMS_InventoryDetail>((a, b) => a.WorkBinNum == b.WorkBinNum)
                       .InnerJoin<EG_WMS_Inventory>((a, b, c) => b.InventoryNum == c.InventoryNum)
                       .InnerJoin<EG_WMS_Materiel>((a, b, c, d) => d.MaterielNum == c.MaterielNum)
                       .Where(a => a.WorkBinNum == string1)
                       .OrderBy((a, b, c) => c.CreateTime, OrderByType.Desc)
                       .Distinct()
                       .Select((a, b, c, d) => new WorkBinBacktrackingDto
                       {
                           // 入库时间
                           StorageTime = (DateTime)c.CreateTime,
                           // 出库时间
                           OutBoundTime = (DateTime)c.UpdateTime,
                       }, true);


        return await data.ToPagedListAsync(page, pagesize);
    }


    #endregion
}


//-------------------------------------/归档/-------------------------------------//

#region 获取料箱信息列表
///// <summary>
///// 获取料箱信息列表
///// </summary>
///// <param name="input"></param>
///// <returns></returns>
//[HttpGet]
//[ApiDescriptionSettings(Name = "List")]
//public async Task<List<EGWorkBinOutput>> List([FromQuery] EGWorkBinInput input)
//{
//    return await _rep.AsQueryable().Select<EGWorkBinOutput>().ToListAsync();
//}

#endregion