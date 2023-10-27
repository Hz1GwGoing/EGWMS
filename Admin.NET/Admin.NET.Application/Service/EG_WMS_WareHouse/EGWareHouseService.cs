namespace Admin.NET.Application;


/// <summary>
/// 仓库管理接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGWareHouseService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<EG_WMS_WareHouse> _rep;
    public EGWareHouseService(SqlSugarRepository<EG_WMS_WareHouse> rep)
    {
        _rep = rep;
    }

    #region 分页查询仓库

    /// <summary>
    /// 分页查询仓库
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<EGWareHouseOutput>> Page(EGWareHouseInput input)
    {

        var query = _rep.AsQueryable()
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WHNum), u => u.WHNum.Contains(input.WHNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WHName), u => u.WHName.Contains(input.WHName.Trim()))
                    .WhereIF(input.WHType > 0, u => u.WHType == input.WHType)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WHAddress), u => u.WHAddress.Contains(input.WHAddress.Trim()))
                    .WhereIF(input.WHStatus > 0, u => u.WHStatus == input.WHStatus)
                    .WhereIF(input.RegionCount > 0, u => u.RegionCount == input.RegionCount)
                    .WhereIF(input.StoreroomCount > 0, u => u.StoreroomCount == input.StoreroomCount)
                    .WhereIF(input.StoreroomUsable > 0, u => u.StoreroomUsable == input.StoreroomUsable)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.CreateUserName), u => u.CreateUserName.Contains(input.CreateUserName.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.UpdateUserName), u => u.UpdateUserName.Contains(input.UpdateUserName.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WHRemake), u => u.WHRemake.Contains(input.WHRemake.Trim()))

                    // 获取创建日期
                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
                    .Select<EGWareHouseOutput>()
;
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    #endregion

    #region 增加仓库
    /// <summary>
    /// 增加仓库
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddEGWareHouseInput input)
    {

        var entity = input.Adapt<EG_WMS_WareHouse>();
        
        await _rep.InsertAsync(entity);
    }

    #endregion

    #region 删除仓库（软删除）
    /// <summary>
    /// 删除仓库（软删除）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteEGWareHouseInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
    }

    #endregion

    #region 更新仓库
    /// <summary>
    /// 更新仓库
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGWareHouseInput input)
    {
        var entity = input.Adapt<EG_WMS_WareHouse>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }
    #endregion

    #region 获取仓库
    /// <summary>
    /// 获取仓库
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<EG_WMS_WareHouse> Get([FromQuery] QueryByIdEGWareHouseInput input)
    {
        //return await _rep.GetFirstAsync(u => u.Id == input.Id);

        // 模糊查询
        return await _rep.GetFirstAsync(u => u.WHName.Contains(input.WHName) || u.WHNum.Contains(input.WHNum));

    }
    #endregion

    #region 获取仓库列表
    /// <summary>
    /// 获取仓库列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGWareHouseOutput>> List([FromQuery] EGWareHouseInput input)
    {
        return await _rep.AsQueryable().Select<EGWareHouseOutput>().ToListAsync();
    }
    #endregion

}

