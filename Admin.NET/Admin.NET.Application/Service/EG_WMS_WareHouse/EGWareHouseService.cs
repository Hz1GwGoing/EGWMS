namespace Admin.NET.Application.Service.EG_WMS_WareHouse;

/// <summary>
/// 仓库管理接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGWareHouseService : IDynamicApiController, ITransient
{
    private static readonly ToolTheCurrentTime _TimeStamp = new ToolTheCurrentTime();

    #region 引用实体
    private readonly SqlSugarRepository<Entity.EG_WMS_WareHouse> _rep;
    private readonly SqlSugarRepository<Entity.EG_WMS_Region> _region;
    #endregion

    #region 关系注入
    public EGWareHouseService(SqlSugarRepository<Entity.EG_WMS_WareHouse> rep, SqlSugarRepository<Entity.EG_WMS_Region> region)
    {
        _rep = rep;
        _region = region;
    }

    #endregion

    #region 可根据条件筛选仓库内容分页

    /// <summary>
    /// 可根据条件筛选仓库内容分页
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>

    [HttpPost]
    [ApiDescriptionSettings(Name = "GetWareHouseDataAll")]
    public async Task<SqlSugarPagedList<GetWareHouseDataDto>> GetWareHouseDataAll(EGWareHouseInput input)
    {

        var data = _region.AsQueryable()
             .RightJoin<Entity.EG_WMS_WareHouse>((a, b) => a.WHNum == b.WHNum)
             .LeftJoin<Entity.EG_WMS_Storage>((a, b, c) => a.RegionNum == c.RegionNum)
             .WhereIF(!string.IsNullOrWhiteSpace(input.WHNum), (a, b, c) => b.WHNum.Contains(input.WHNum))
             .WhereIF(!string.IsNullOrWhiteSpace(input.WHName), (a, b, c) => b.WHName.Contains(input.WHName))
             .WhereIF(!string.IsNullOrWhiteSpace(input.WHAddress), (a, b, c) => b.WHAddress.Contains(input.WHAddress))
             .WhereIF(input.WHStatus >= 0, (a, b, c) => b.WHStatus == input.WHStatus)
             .WhereIF(input.WHType >= 0, (a, b, c) => b.WHType == input.WHType)
             .Select((a, b, c) => new GetWareHouseDataDto
             {
                 Id = b.Id,
                 WHNum = b.WHNum,
                 WHName = b.WHName,
                 WHType = (int)b.WHType,
                 WHAddress = b.WHAddress,
                 WHStatus = (int)b.WHStatus,

                 // 当前仓库下区域总数
                 CurrentRegionCount = SqlFunc.Subqueryable<Entity.EG_WMS_Region>()
                                             .Where(x => x.WHNum == b.WHNum && x.IsDelete == false)
                                             .Select(x => SqlFunc.AggregateCount(x.RegionNum)),
                 // 当前仓库下库位总数
                 CurrentStorageCount = SqlFunc.Subqueryable<Entity.EG_WMS_Storage>()
                                              .Where(x => x.RegionNum == (SqlFunc.Subqueryable<Entity.EG_WMS_Region>()
                                              // 添加GroupBy == 会自动转换成 In
                                              .Where(x => x.WHNum == b.WHNum).GroupBy(x => x.RegionNum).Select(x => x.RegionNum)))
                                              .Select(x => SqlFunc.AggregateCount(x.StorageNum)),
                 // 当前仓库下可用库位总数
                 CurrentStorageCountUsAble = SqlFunc.Subqueryable<Entity.EG_WMS_Storage>()
                                              .Where(x => x.StorageOccupy == 0 && x.RegionNum == (SqlFunc.Subqueryable<Entity.EG_WMS_Region>()
                                              .Where(x => x.WHNum == b.WHNum).GroupBy(x => x.RegionNum).Select(x => x.RegionNum)))
                                              .Select(x => SqlFunc.AggregateCount(x.StorageNum)),
                 WHRemake = b.WHRemake,
                 CreateUserName = b.CreateUserName,
                 UpdateUserName = b.UpdateUserName,

             })
             .GroupBy(a => a.WHNum);

        return await data.ToPagedListAsync(input.page, input.pageSize);


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
        input.WHNum = _TimeStamp.GetTheCurrentTimeTimeStamp("WH");
        var entity = input.Adapt<Entity.EG_WMS_WareHouse>();
        await _rep.InsertAsync(entity);
    }

    #endregion

    #region 删除仓库（软删除）
    /// <summary>
    /// 删除仓库（软删除）
    /// </summary>
    /// <param name="id">仓库id</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(long id)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        // 查找这个仓库下面有没有区域，有区域则无法删除
        var whregioncount = _region.AsQueryable()
                .Where(x => x.WHNum == entity.WHNum)
                .Count();
        if (whregioncount != 0)
        {
            throw Oops.Oh("当前仓库下有区域，无法删除这个仓库！");
        }
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
        var entity = input.Adapt<Entity.EG_WMS_WareHouse>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }
    #endregion

    #region 获取仓库列表
    /// <summary>
    /// 获取仓库列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGWareHouseOutput>> List([FromQuery] EGWareHouseInput input)
    {
        return await _rep.AsQueryable().Select<EGWareHouseOutput>().ToListAsync();
    }
    #endregion


}

//-------------------------------------/归档/-------------------------------------//


#region 分页查询仓库

///// <summary>
///// 分页查询仓库
///// </summary>
///// <param name="input"></param>
///// <returns></returns>
//    [HttpPost]
//    [ApiDescriptionSettings(Name = "Page")]
//    public async Task<SqlSugarPagedList<EGWareHouseOutput>> Page(EGWareHouseInput input)
//    {

//        var query = _rep.AsQueryable()
//                    .WhereIF(!string.IsNullOrWhiteSpace(input.WHNum), u => u.WHNum.Contains(input.WHNum.Trim()))
//                    .WhereIF(!string.IsNullOrWhiteSpace(input.WHName), u => u.WHName.Contains(input.WHName.Trim()))
//                    .WhereIF(input.WHType >= 0, u => u.WHType == input.WHType)
//                    .WhereIF(!string.IsNullOrWhiteSpace(input.WHAddress), u => u.WHAddress.Contains(input.WHAddress.Trim()))
//                    .WhereIF(input.WHStatus >= 0, u => u.WHStatus == input.WHStatus)

//                    // 获取创建日期
//                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
//                    .Select<EGWareHouseOutput>()
//;
//        query = query.OrderBuilder(input);
//        return await query.ToPagedListAsync(input.Page, input.PageSize);
//    }

#endregion
