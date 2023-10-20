namespace Admin.NET.Application;
/// <summary>
/// 库存明细表接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGInventoryDetailService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<EG_WMS_InventoryDetail> _rep;
    public EGInventoryDetailService(SqlSugarRepository<EG_WMS_InventoryDetail> rep)
    {
        _rep = rep;
    }

    #region 分页查询库存明细
    /// <summary>
    /// 分页查询库存明细
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<EGInventoryDetailOutput>> Page(EGInventoryDetailInput input)
    {
        var query = _rep.AsQueryable()
                    //.WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), u => u.MaterielNum.Contains(input.MaterielNum.Trim()))
                    // 库存编号
                    //.WhereIF(!string.IsNullOrWhiteSpace(input.InventoryNum), u => u.InventoryNum.Contains(input.InventoryNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.ProductionLot), u => u.ProductionLot.Contains(input.ProductionLot.Trim()))
                    .WhereIF(input.CurrentCount > 0, u => u.CurrentCount == input.CurrentCount)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.StorageNum), u => u.StorageNum.Contains(input.StorageNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WHNum), u => u.WHNum.Contains(input.WHNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.RegionNum), u => u.RegionNum.Contains(input.RegionNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.ShelfNum), u => u.ShelfNum.Contains(input.ShelfNum.Trim()))
                    // 物料编号
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), u => u.MaterielNum.Contains(input.MaterielNum.Trim()))
                    // 备注
                    .WhereIF(!string.IsNullOrWhiteSpace(input.InventoryDetailRemake), u =>
                    u.InventoryDetailRemake.Contains(input.InventoryDetailRemake.Trim()))
                    .WhereIF(input.FrozenState > 0, u => u.FrozenState == input.FrozenState)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.PalletNum), u => u.PalletNum.Contains(input.PalletNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinNum), u => u.WorkBinNum.Contains(input.WorkBinNum.Trim()))
                    // 获取创建日期
                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
                    // 倒序
                    .OrderBy(it => it.CreateTime, OrderByType.Desc)
                    .Select<EGInventoryDetailOutput>();
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }
    #endregion

    #region 更新库存明细
    /// <summary>
    /// 更新库存明细
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGInventoryDetailInput input)
    {
        var entity = input.Adapt<EG_WMS_InventoryDetail>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }
    #endregion

    #region 获取库存明细
    /// <summary>
    /// 获取库存明细
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<EG_WMS_InventoryDetail> Get([FromQuery] QueryByIdEGInventoryDetailInput input)
    {
        //return await _rep.GetFirstAsync(u => u.Id == input.Id);

        // 模糊查询（）
        return await _rep.GetFirstAsync(u => u.MaterielNum.Contains(input.MaterielNum));
    }
    #endregion

    #region 获取库存明细列表
    /// <summary>
    /// 获取库存明细列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGInventoryDetailOutput>> List([FromQuery] EGInventoryDetailInput input)
    {
        return await _rep.AsQueryable().Select<EGInventoryDetailOutput>().ToListAsync();
    }
    #endregion


    //-------------------------------------//-------------------------------------//


    #region 增加库存明细
    /// <summary>
    /// 增加库存明细
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    //[HttpPost]
    //[ApiDescriptionSettings(Name = "Add")]
    //public async Task Add(AddEGInventoryDetailInput input)
    //{
    //    var entity = input.Adapt<EGInventoryDetail>();
    //    await _rep.InsertAsync(entity);
    //}
    #endregion

    #region 删除库存明细
    /// <summary>
    /// 删除库存明细
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    //[HttpPost]
    //[ApiDescriptionSettings(Name = "Delete")]
    //public async Task Delete(DeleteEGInventoryDetailInput input)
    //{
    //    var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
    //    await _rep.FakeDeleteAsync(entity);   //假删除
    //}
    #endregion

}


