using Admin.NET.Core;
using System.Formats.Asn1;

namespace Admin.NET.Application.Service.EG_WMS_Storage;

/// <summary>
/// 库位管理接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGStorageService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<Entity.EG_WMS_Storage> _rep;
    private readonly SqlSugarRepository<Entity.EG_WMS_Region> _region;

    public EGStorageService(SqlSugarRepository<Entity.EG_WMS_Storage> rep, SqlSugarRepository<Entity.EG_WMS_Region> region)
    {
        _rep = rep;
        _region = region;
    }

    #region 查询区域下库位总数

    /// <summary>
    /// 查询区域下库位总数
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "GetSumStorageCount")]
    public List<GetCountStorage> GetSumStorageCount()
    {
        return _rep.AsQueryable()
            .Where(a => a.StorageStatus == 0)
            .GroupBy(a => a.RegionNum)
            .Select(a => new GetCountStorage
            {
                RegionNum = a.RegionNum,
                SumCountStorage = SqlFunc.AggregateCount(a.StorageNum)
            })
            .ToList();

    }



    #endregion

    #region 得到每个区域下有多少个库位

    /// <summary>
    /// 得到每个区域下有多少个库位
    /// </summary>
    /// <param name="page">页数</param>
    /// <param name="pageSize">每页容量</param>
    /// <returns></returns>

    [HttpGet]
    [ApiDescriptionSettings(Name = "SelectRegionStorageCount")]
    public async Task<SqlSugarPagedList<SelectRegionStorageCountDto>> SelectRegionStorageCount(int page, int pageSize)
    {

        var data = _region.AsQueryable()
                       .LeftJoin<Entity.EG_WMS_Storage>((a, b) => a.RegionNum == b.RegionNum)
                       .InnerJoin<Entity.EG_WMS_WareHouse>((a, b, c) => a.WHNum == c.WHNum)
                       .GroupBy(a => a.RegionNum)
                       .Select((a, b, c) => new SelectRegionStorageCountDto
                       {
                           id = a.Id,
                           RegionNum = a.RegionNum,
                           RegionName = a.RegionName,
                           WHNum = c.WHNum,
                           WHName = c.WHName,
                           TotalStorage = SqlFunc.AggregateCount(b.StorageNum),
                           EnabledStorage = SqlFunc.AggregateCount(b.StorageStatus == 0),
                           UsedStorage = SqlFunc.AggregateSum(SqlFunc.IIF(b.StorageOccupy == 1, 1, 0)),
                           Remake = b.StorageRemake,
                           CreateUserName = a.CreateUserName,
                           UpdateUserName = a.UpdateUserName,
                           // 区域绑定物料
                           RegionMaterielNum = a.RegionMaterielNum,
                       });

        return await data.ToPagedListAsync(page, pageSize);

    }


    #endregion

    #region 分页查询库位表内容（联表：区域、仓库）

    /// <summary>
    /// 分页查询库位表内容（联表：区域、仓库）
    /// </summary>
    /// <param name="page">页数</param>
    /// <param name="pageSize">每页容量</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "GetStorageRegionAndWH")]
    public async Task<SqlSugarPagedList<StorageRegionAndWhDto>> GetStorageRegionAndWH(int page, int pageSize)
    {
        var query = _rep.AsQueryable()
                        .InnerJoin<Entity.EG_WMS_Region>((a, b) => a.RegionNum == b.RegionNum)
                        .InnerJoin<Entity.EG_WMS_WareHouse>((a, b, c) => b.WHNum == c.WHNum)
                        .OrderBy(a => a.StorageNum, OrderByType.Asc)
                        .Select((a, b, c) => new StorageRegionAndWhDto
                        {
                            // 库位id
                            ID = a.Id,
                            StorageNum = a.StorageNum,
                            StorageName = a.StorageName,
                            StorageStatus = (int)a.StorageStatus,
                            WHNum = c.WHNum,
                            WHName = c.WHName,
                            RegionNum = b.RegionNum,
                            RegionName = b.RegionName,
                            StorageType = a.StorageType,
                            RoadwayNum = (int)a.RoadwayNum,
                            ShelfNum = (int)a.ShelfNum,
                            FloorNumber = (int)a.FloorNumber,
                            StorageOccupy = (int)a.StorageOccupy,
                            StorageRemake = a.StorageRemake,
                            CreateUserName = a.CreateUserName,
                            CreateTime = (DateTime)a.CreateTime,
                            StorageGroup = a.StorageGroup,
                        });


        return await query.ToPagedListAsync(page, pageSize);

    }

    #endregion

    #region 增加库位
    /// <summary>
    /// 增加库位
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddEGStorageInput input)
    {
        var entity = input.Adapt<Entity.EG_WMS_Storage>();
        await _rep.InsertAsync(entity);
    }
    #endregion

    #region 删除库位
    /// <summary>
    /// 删除库位
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteEGStorageInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
    }

    #endregion

    #region 更新库位
    /// <summary>
    /// 更新库位
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGStorageInput input)
    {
        var entity = input.Adapt<Entity.EG_WMS_Storage>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    #endregion

    #region 获取库位
    /// <summary>
    /// 获取库位
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<Entity.EG_WMS_Storage> Get([FromQuery] QueryByIdEGStorageInput input)
    {
        // 模糊查询
        return await _rep.GetFirstAsync(u => u.StorageNum.Contains(input.StorageNum) ||
                                        u.StorageName.Contains(input.StorageName) ||
                                        u.StorageGroup.Contains(input.StorageGroup));
    }
    #endregion

    #region 获取库位列表
    /// <summary>
    /// 获取库位列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGStorageOutput>> List([FromQuery] EGStorageInput input)
    {
        return await _rep.AsQueryable().Select<EGStorageOutput>().ToListAsync();
    }
    #endregion

    //-------------------------------------//-------------------------------------//

    #region 归档
    //return _rep.AsQueryable()
    //     .InnerJoin<Entity.EG_WMS_Region>((a, b) => a.RegionNum == b.RegionNum)
    //     .InnerJoin<EG_WMS_WareHouse>((a, b, c) => b.WHNum == c.WHNum)
    //     .OrderBy(a => a.StorageNum, OrderByType.Asc)
    //     .Select((a, b, c) => new StorageRegionAndWhDto
    //     {
    //         StorageNum = a.StorageNum,
    //         StorageName = a.StorageName,
    //         StorageStatus = (int)a.StorageStatus,
    //         WHName = c.WHName,
    //         RegionName = b.RegionName,
    //         StorageType = a.StorageType,
    //         RoadwayNum = (int)a.RoadwayNum,
    //         ShelfNum = (int)a.ShelfNum,
    //         FloorNumber = (int)a.FloorNumber,
    //         StorageOccupy = (int)a.StorageOccupy,
    //         StorageRemake = a.StorageRemake,
    //         CreateUserName = a.CreateUserName,
    //         CreateTime = (DateTime)a.CreateTime,
    //         StorageGroup = a.StorageGroup,
    //     })
    //     .Skip((page - 1) * pageSize)
    //     .Take(pageSize)
    //     .ToList();
    #endregion

    #region 获取仓库名称
    /// <summary>
    /// 获取仓库名称
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    //[ApiDescriptionSettings(Name = "EGWareHouseWHNameDropdown"), HttpGet]
    //public async Task<dynamic> EGWareHouseWHNumDropdown()
    //{
    //    return await _rep.Context.Queryable<EGWareHouse>()
    //            .Select(u => new
    //            {
    //                ISWareHouseName = u.WHName,
    //                // 主键
    //                Value = u.Id
    //            }
    //            ).ToListAsync();
    //}
    #endregion

    #region 获取区域名称
    /// <summary>
    /// 获取区域名称
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    //[ApiDescriptionSettings(Name = "EGRegionRegionNameDropdown"), HttpGet]
    //public async Task<dynamic> EGRegionRegionNumDropdown()
    //{
    //    return await _rep.Context.Queryable<EGRegion>()
    //            .Select(u => new
    //            {
    //                ISRegionName = u.RegionName,
    //                // 主键
    //                Value = u.Id
    //            }
    //            ).ToListAsync();
    //}
    #endregion

    #region 分页查询库位
    /// <summary>
    /// 分页查询库位
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    //[HttpPost]
    //[ApiDescriptionSettings(Name = "Page")]
    //public async Task<SqlSugarPagedList<EGStorageOutput>> Page(EGStorageInput input)
    //{
    //    var query = _rep.AsQueryable()
    //                .WhereIF(!string.IsNullOrWhiteSpace(input.StorageNum), u => u.StorageNum.Contains(input.StorageNum.Trim()))
    //                .WhereIF(!string.IsNullOrWhiteSpace(input.StorageName), u => u.StorageName.Contains(input.StorageName.Trim()))
    //                .WhereIF(!string.IsNullOrWhiteSpace(input.StorageAddress), u => u.StorageAddress.Contains(input.StorageAddress.Trim()))
    //                .WhereIF(!string.IsNullOrWhiteSpace(input.StorageType), u => u.StorageType.Contains(input.StorageType.Trim()))
    //                //.WhereIF(!string.IsNullOrWhiteSpace(input.StorageOccupy), u => u.StorageOccupy.Contains(input.StorageOccupy.Trim()))
    //                .WhereIF(!string.IsNullOrWhiteSpace(input.StorageRemake), u => u.StorageRemake.Contains(input.StorageRemake.Trim()))
    //                .WhereIF(!string.IsNullOrWhiteSpace(input.CreateUserName), u => u.CreateUserName.Contains(input.CreateUserName.Trim()))
    //                .WhereIF(!string.IsNullOrWhiteSpace(input.UpdateUserName), u => u.UpdateUserName.Contains(input.UpdateUserName.Trim()))
    //                // 库位状态
    //                .WhereIF(input.StorageStatus >= 0, u => u.StorageStatus == input.StorageStatus)
    //                .WhereIF(input.RoadwayNum > 0, u => u.RoadwayNum == input.RoadwayNum)
    //                .WhereIF(input.ShelfNum > 0, u => u.ShelfNum == input.ShelfNum)
    //                .WhereIF(input.FloorNumber > 0, u => u.FloorNumber == input.FloorNumber)
    //                // 组别
    //                .WhereIF(!string.IsNullOrWhiteSpace(input.StorageGroup), u => u.StorageGroup.Contains(input.StorageGroup.Trim()))
    //                .WhereIF(!string.IsNullOrWhiteSpace(input.WHNum), u => u.WHNum.Contains(input.WHNum.Trim()))
    //                .WhereIF(!string.IsNullOrWhiteSpace(input.RegionNum), u => u.RegionNum.Contains(input.RegionNum.Trim()))
    //                //处理外键和TreeSelector相关字段的连接
    //                .LeftJoin<EG_WMS_WareHouse>((u, whnum) => u.WHNum == whnum.WHNum)
    //                .LeftJoin<EG_WMS_Region>((u, whnum, regionnum) => u.RegionNum == regionnum.RegionNum)
    //                .Select((u, whnum, regionnum) => new EGStorageOutput
    //                {
    //                    StorageNum = u.StorageNum,
    //                    StorageName = u.StorageName,
    //                    StorageAddress = u.StorageAddress,
    //                    StorageType = u.StorageType,
    //                    // 库位状态
    //                    StorageStatus = u.StorageStatus,
    //                    StorageLong = u.StorageLong,
    //                    StorageWidth = u.StorageWidth,
    //                    StorageHigh = u.StorageHigh,
    //                    StorageOccupy = u.StorageOccupy,
    //                    StorageRemake = u.StorageRemake,
    //                    CreateUserName = u.CreateUserName,
    //                    UpdateUserName = u.UpdateUserName,
    //                    Id = u.Id,
    //                    RoadwayNum = u.RoadwayNum,
    //                    ShelfNum = u.ShelfNum,
    //                    FloorNumber = u.FloorNumber,
    //                    WHNum = u.WHNum,
    //                    EGWareHouseWHNum = whnum.WHNum,
    //                    RegionNum = u.RegionNum,
    //                    EGRegionRegionNum = regionnum.RegionNum,
    //                });
    //    query = query.OrderBuilder(input);
    //    return await query.ToPagedListAsync(input.Page, input.PageSize);
    //}
    #endregion
}

