namespace Admin.NET.Application;


/// <summary>
/// 区域管理接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGRegionService : IDynamicApiController, ITransient
{
    private static readonly ToolTheCurrentTime _TimeStamp = new ToolTheCurrentTime();


    #region 实体引入
    private readonly SqlSugarRepository<EG_WMS_Region> _rep;
    private readonly SqlSugarRepository<EG_WMS_Storage> _Storage;
    #endregion

    #region 关系注入
    public EGRegionService
    (
    SqlSugarRepository<EG_WMS_Region> rep,
    SqlSugarRepository<EG_WMS_Storage> Storage
    )
    {
        _rep = rep;
        _Storage = Storage;
    }
    #endregion

    #region 可根据条件筛选区域内容分页

    /// <summary>
    /// 可根据条件筛选区域内容分页
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>

    [HttpPost]
    [ApiDescriptionSettings(Name = "SelectRegionStorageCount")]
    public async Task<SqlSugarPagedList<SelectRegionStorageCountDto>> SelectRegionStorageCount(EGRegionInput input)
    {

        var data = _rep.AsQueryable()
                       .LeftJoin<Entity.EG_WMS_Storage>((a, b) => a.RegionNum == b.RegionNum)
                       .InnerJoin<Entity.EG_WMS_WareHouse>((a, b, c) => a.WHNum == c.WHNum)
                       .GroupBy(a => a.RegionNum)
                       .WhereIF(!string.IsNullOrWhiteSpace(input.RegionNum), (a, b, c) => a.RegionNum.Contains(input.RegionNum.Trim()))
                       .WhereIF(!string.IsNullOrWhiteSpace(input.RegionName), (a, b, c) => a.RegionName.Contains(input.RegionName.Trim()))
                       .WhereIF(input.RegionStatus >= 0, (a, b, c) => a.RegionStatus == input.RegionStatus)
                       .WhereIF(!string.IsNullOrWhiteSpace(input.WHNum), (a, b, c) => a.WHNum.Contains(input.WHNum.Trim()))
                       .Select((a, b, c) => new SelectRegionStorageCountDto
                       {
                           id = a.Id,
                           RegionNum = a.RegionNum,
                           RegionName = a.RegionName,
                           WHNum = c.WHNum,
                           WHName = c.WHName,
                           TotalStorage = SqlFunc.AggregateCount(b.StorageNum),
                           //EnabledStorage = SqlFunc.AggregateCount(b.StorageStatus == 0 && b.StorageOccupy == 0),
                           EnabledStorage = SqlFunc.Subqueryable<Entity.EG_WMS_Storage>()
                                                   .Where(x => x.StorageOccupy == 0 && x.StorageStatus == 0 && x.RegionNum == a.RegionNum)
                                                   .Select(x => SqlFunc.AggregateCount(x.StorageNum)),
                           UsedStorage = SqlFunc.AggregateSum(SqlFunc.IIF(b.StorageOccupy == 1, 1, 0)),
                           Remake = b.StorageRemake,
                           CreateUserName = a.CreateUserName,
                           UpdateUserName = a.UpdateUserName,
                           // 区域绑定物料
                           RegionMaterielNum = a.RegionMaterielNum,
                       });

        return await data.ToPagedListAsync(input.page, input.pageSize);

    }


    #endregion

    #region 根据区域查询已占用的库位

    /// <summary>
    /// 根据区域查询已占用的库位
    /// </summary>
    /// <param name="regionnum">区域编号</param>
    /// <param name="page">页数</param>
    /// <param name="pageSize">每页容量</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "ReasonRegionSelectStorage")]
    public async Task<SqlSugarPagedList<SelectStorageDto>> ReasonRegionSelectStorage(string regionnum, int page, int pageSize)
    {

        var data = _Storage.AsQueryable()
                           .Where(x => x.RegionNum == regionnum && x.StorageOccupy == 1)
                           .Select(x => new SelectStorageDto
                           {
                               StorageNum = x.StorageNum,
                               StorageName = x.StorageName,
                               StorageOccupy = (int)x.StorageOccupy,
                               StorageStatus = (int)x.StorageStatus,
                           });

        return await data.ToPagedListAsync(page, pageSize);


    }


    #endregion

    #region 根据区域查询未占用的库位

    /// <summary>
    /// 根据区域查询未占用的库位
    /// </summary>
    /// <param name="regionnum">区域编号</param>
    /// <param name="page">页数</param>
    /// <param name="pageSize">每页容量</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "ReasonRegionSelectNotStorage")]
    public async Task<SqlSugarPagedList<SelectStorageDto>> ReasonRegionSelectNotStorage(string regionnum, int page, int pageSize)
    {

        var data = _Storage.AsQueryable()
                           .Where(x => x.RegionNum == regionnum && x.StorageOccupy == 0)
                           .Select(x => new SelectStorageDto
                           {
                               StorageNum = x.StorageNum,
                               StorageName = x.StorageName,
                               StorageOccupy = (int)x.StorageOccupy,
                               StorageStatus = (int)x.StorageStatus,
                           });

        return await data.ToPagedListAsync(page, pageSize);


    }


    #endregion

    #region 查询一共有多少个区域

    /// <summary>
    /// 查询一共有多少个区域
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "GetSumRegionCount")]
    public List<GetSumRegionCountDto> GetSumRegionCount()
    {
        return _rep.AsQueryable()
                   .InnerJoin<EG_WMS_WareHouse>((a, b) => a.WHNum == b.WHNum)
                   .Where(a => a.RegionStatus == 0)
                   .Select((a, b) => new GetSumRegionCountDto
                   {
                       WHNum = a.WHNum,
                       WHName = b.WHName,
                       sumcount = SqlFunc.AggregateCount(a.RegionNum)

                   })
                   .GroupBy(a => a.WHNum)
                   .ToList();

    }

    #endregion

    #region 得到这个区域下有多少个库位

    /// <summary>
    /// 得到这个区域下有多少个库位
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "GetRegionStorageCount")]
    public async Task<SqlSugarPagedList<GetRegionStorageCountDto>> GetRegionStorageCount(int page, int pageSize)
    {

        var data = _Storage.AsQueryable()
                       .InnerJoin<EG_WMS_Region>((a, b) => a.RegionNum == b.RegionNum)
                       .GroupBy(a => a.RegionNum)
                       .Where((a, b) => a.RegionNum == b.RegionNum)
                       .Select((a, b) => new GetRegionStorageCountDto
                       {
                           RegionNum = b.RegionNum,
                           RegionName = b.RegionName,
                           RegionStatus = (int)b.RegionStatus,
                           StorageCount = SqlFunc.AggregateCount(a.StorageNum)

                       });

        return await data.ToPagedListAsync(page, pageSize);

    }




    #endregion

    #region 当前区域下所有库位

    /// <summary>
    /// 当前区域下所有库位
    /// </summary>
    /// <param name="regionnum">区域编号</param>
    /// <param name="page">页码</param>
    /// <param name="pagesize">每页容量</param>
    /// <returns></returns>

    [HttpPost]
    [ApiDescriptionSettings(Name = "AllLocationsUnderStorage")]
    public async Task<SqlSugarPagedList<SelectStorageDto>> AllLocationsUnderStorage(string regionnum, int page, int pagesize)
    {
        var data = _rep.AsQueryable()
                       .InnerJoin<EG_WMS_Storage>((a, b) => a.RegionNum == b.RegionNum)
                       .OrderBy((a, b) => b.StorageNum, OrderByType.Desc)
                       .Where(a => a.RegionNum == regionnum)
                       .Select<SelectStorageDto>();

        return await data.ToPagedListAsync(page, pagesize);

    }


    #endregion

    #region 所有未占用库位

    /// <summary>
    /// 所有未占用库位
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pagesize">每页容量</param>
    /// <returns></returns>

    [HttpPost]
    [ApiDescriptionSettings(Name = "AllUnoccupiedStorage")]
    public async Task<SqlSugarPagedList<SelectStorageDto>> AllUnoccupiedStorage(int page, int pagesize)
    {

        var data = _Storage.AsQueryable()
                           .Where(x => x.StorageType == 0 && x.StorageOccupy == 0)
                           .OrderBy(x => x.StorageNum, OrderByType.Desc)
                           .Select<SelectStorageDto>();

        return await data.ToPagedListAsync(page, pagesize);

    }


    #endregion

    #region 增加区域
    /// <summary>
    /// 增加区域
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddEGRegionInput input)
    {
        input.RegionNum = _TimeStamp.GetTheCurrentTimeTimeStamp("Region");
        var entity = input.Adapt<EG_WMS_Region>();
        await _rep.InsertAsync(entity);
    }
    #endregion

    #region 删除区域
    /// <summary>
    /// 删除区域
    /// </summary>
    /// <param name="id">区域id</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(long id)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        // 根据这个区域去查询这个区域下有没有库位，有库位不能删除
        var regionstoragecount = _Storage.AsQueryable()
                                    .Where(x => x.RegionNum == entity.RegionNum)
                                    .Count();
        if (regionstoragecount != 0)
        {
            throw Oops.Oh("当前区域下有库位，无法删除这个区域！");
        }

        await _rep.FakeDeleteAsync(entity);   //假删除
    }
    #endregion

    #region 更新区域
    /// <summary>
    /// 更新区域
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGRegionInput input)
    {
        await _rep.AsUpdateable()
                           .SetColumns(it => new EG_WMS_Region
                           {
                               RegionMaterielNum = input.RegionMaterielNum,
                               RegionName = input.RegionName,
                               RegionType = input.RegionType,
                               UpdateTime = DateTime.Now,
                           })
                           .Where(it => it.Id == input.Id)
                           .ExecuteCommandAsync();
    }
    #endregion

}

//-------------------------------------//-------------------------------------//

#region 获取仓库名称列表
///// <summary>
///// 获取仓库名称列表
///// </summary>
///// <returns></returns>
//[HttpGet]
//[ApiDescriptionSettings(Name = "EGWareHouseWHNameDropdown")]
//public async Task<dynamic> EGWareHouseWHNumDropdown()
//{
//    return await _rep.Context.Queryable<EG_WMS_WareHouse>()
//            .Select(u => new
//            {

//                ISWareHouse = u.WHName,
//                Value = u.Id
//            }
//            ).ToListAsync();
//}

#endregion

#region 分页查询区域
//    /// <summary>
//    /// 分页查询区域
//    /// </summary>
//    /// <param name="input"></param>
//    /// <returns></returns>
//    [HttpPost]
//    [ApiDescriptionSettings(Name = "Page")]
//    public async Task<SqlSugarPagedList<EGRegionOutput>> Page(EGRegionInput input)
//    {
//        var query = _rep.AsQueryable()
//                    .WhereIF(!string.IsNullOrWhiteSpace(input.RegionNum), u => u.RegionNum.Contains(input.RegionNum.Trim()))
//                    .WhereIF(!string.IsNullOrWhiteSpace(input.RegionName), u => u.RegionName.Contains(input.RegionName.Trim()))
//                    .WhereIF(input.RegionStatus >= 0, u => u.RegionStatus == input.RegionStatus)
//                    .WhereIF(!string.IsNullOrWhiteSpace(input.WHNum), u => u.WHNum.Contains(input.WHNum.Trim()))
//                    //处理外键和TreeSelector相关字段的连接
//                    .LeftJoin<EG_WMS_WareHouse>((u, whnum) => u.WHNum == whnum.WHNum)
//                    .Select((u, whnum) => new EGRegionOutput
//                    {
//                        Id = u.Id,
//                        RegionNum = u.RegionNum,
//                        RegionName = u.RegionName,
//                        RegionStatus = u.RegionStatus,
//                        CreateUserName = u.CreateUserName,
//                        UpdateUserName = u.UpdateUserName,
//                        RegionRemake = u.RegionRemake,
//                        WHNum = u.WHNum,
//                        EGWareHouseWHNum = whnum.WHNum,
//                    })
//;
//        query = query.OrderBuilder(input);
//        return await query.ToPagedListAsync(input.Page, input.PageSize);
//    }
#endregion