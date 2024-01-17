namespace Admin.NET.Application;

/// <summary>
/// 料箱信息接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGWorkBinService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<EG_WMS_WorkBin> _rep;
    public EGWorkBinService(SqlSugarRepository<EG_WMS_WorkBin> rep)
    {
        _rep = rep;
    }


    #region 分页查询料箱信息
    /// <summary>
    /// 分页查询料箱信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<EGWorkBinOutput>> Page(EGWorkBinInput input)
    {
        var query = _rep.AsQueryable()
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinNum), u => u.WorkBinNum.Contains(input.WorkBinNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinName), u => u.WorkBinName.Contains(input.WorkBinName.Trim()))
                    // 料箱规格
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinSpecs), u => u.WorkBinSpecs.Contains(input.WorkBinSpecs.Trim()))
                    .WhereIF(input.MachineNum > 0, u => u.MachineNum == input.MachineNum)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.Classes), u => u.Classes.Contains(input.Classes.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.ProductionLot), u => u.ProductionLot.Contains(input.ProductionLot.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.ProductionStaff), u => u.ProductionStaff.Contains(input.ProductionStaff.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.Inspector), u => u.Inspector.Contains(input.Inspector.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.Printer), u => u.Printer.Contains(input.Printer.Trim()))
                    .WhereIF(input.WorkBinStatus > 0, u => u.WorkBinStatus == input.WorkBinStatus)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), u => u.MaterielNum == input.MaterielNum)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinRemake), u => u.WorkBinRemake.Contains(input.WorkBinRemake.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.StorageNum), u => u.StorageNum.Contains(input.StorageNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.PalletNum), u => u.PalletNum.Contains(input.PalletNum.Trim()))

                    // 获取创建日期
                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)

                    .Select<EGWorkBinOutput>()
;
        if (input.ProductionDateRange != null && input.ProductionDateRange.Count > 0)
        {
            DateTime? start = input.ProductionDateRange[0];
            query = query.WhereIF(start.HasValue, u => u.ProductionDate > start);
            if (input.ProductionDateRange.Count > 1 && input.ProductionDateRange[1].HasValue)
            {
                var end = input.ProductionDateRange[1].Value.AddDays(1);
                query = query.Where(u => u.ProductionDate < end);
            }
        }
        if (input.PrintTimeRange != null && input.PrintTimeRange.Count > 0)
        {
            DateTime? start = input.PrintTimeRange[0];
            query = query.WhereIF(start.HasValue, u => u.PrintTime > start);
            if (input.PrintTimeRange.Count > 1 && input.PrintTimeRange[1].HasValue)
            {
                var end = input.PrintTimeRange[1].Value.AddDays(1);
                query = query.Where(u => u.PrintTime < end);
            }
        }
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
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

    #region 料箱回溯

    /// <summary>
    /// 料箱回溯（TODO:完善中，请勿使用）
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

        // TODO：通过入库编号反推出库编号


        //var data = _rep.AsQueryable()
        //               .InnerJoin<EG_WMS_InAndOutBound>((a, b) => a.InAndOutBoundNum == b.InAndOutBoundNum)
        //               .InnerJoin<EG_WMS_Inventory>((a, b, c) => a.InAndOutBoundNum == c.InBoundNum)
        //               .Where((a, b, c) => a.WorkBinNum == string1 && a.WorkBinStatus == 0)
        //               .WhereIF(
        //               (a, b, c) =>
        //               SqlFunc.Subqueryable<EG_WMS_Inventory>()
        //               .InnerJoin<EG_WMS_InventoryDetail>((inv, invd) => inv.InventoryNum == invd.InventoryNum)
        //               .Where(x => x.)
        //               )
        //               .OrderBy(x => x.CreateTime, OrderByType.Desc)
        //               .Select(x => new WorkBinBacktrackingDto
        //               {
        //                   IsCount = x.ProductCount

        //               }, true);


        //return await data.ToPagedListAsync(page, pagesize);
        return null;
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