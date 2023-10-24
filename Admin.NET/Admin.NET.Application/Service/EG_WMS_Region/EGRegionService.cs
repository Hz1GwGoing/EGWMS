﻿namespace Admin.NET.Application;
/// <summary>
/// 区域管理接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGRegionService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<EG_WMS_Region> _rep;
    public EGRegionService(SqlSugarRepository<EG_WMS_Region> rep)
    {
        _rep = rep;
    }


    #region 分页查询区域
    /// <summary>
    /// 分页查询区域
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<EGRegionOutput>> Page(EGRegionInput input)
    {
        var query = _rep.AsQueryable()
                    .WhereIF(!string.IsNullOrWhiteSpace(input.RegionNum), u => u.RegionNum.Contains(input.RegionNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.RegionName), u => u.RegionName.Contains(input.RegionName.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WareHouseName), u => u.WareHouseName.Contains(input.WareHouseName.Trim()))
                    .WhereIF(input.RegionStatus > 0, u => u.RegionStatus == input.RegionStatus)
                    .WhereIF(input.StoreroomCount > 0, u => u.StoreroomCount == input.StoreroomCount)
                    .WhereIF(input.StoreroomUsable > 0, u => u.StoreroomUsable == input.StoreroomUsable)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.CreateUserName), u => u.CreateUserName.Contains(input.CreateUserName.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.UpdateUserName), u => u.UpdateUserName.Contains(input.UpdateUserName.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.RegionRemake), u => u.RegionRemake.Contains(input.RegionRemake.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WHNum), u => u.WHNum.Contains(input.WHNum.Trim()))
                    //处理外键和TreeSelector相关字段的连接
                    .LeftJoin<EG_WMS_WareHouse>((u, whnum) => u.WHNum == whnum.WHNum)
                    .Select((u, whnum) => new EGRegionOutput
                    {
                        Id = u.Id,
                        RegionNum = u.RegionNum,
                        RegionName = u.RegionName,
                        WareHouseName = u.WareHouseName,
                        RegionStatus = u.RegionStatus,
                        StoreroomCount = u.StoreroomCount,
                        StoreroomUsable = u.StoreroomUsable,
                        CreateUserName = u.CreateUserName,
                        UpdateUserName = u.UpdateUserName,
                        RegionRemake = u.RegionRemake,
                        WHNum = u.WHNum,
                        EGWareHouseWHNum = whnum.WHNum,
                    })
;
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
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
        var entity = input.Adapt<EG_WMS_Region>();
        await _rep.InsertAsync(entity);
    }
    #endregion

    #region 删除区域
    /// <summary>
    /// 删除区域
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteEGRegionInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
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
        var entity = input.Adapt<EG_WMS_Region>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }
    #endregion

    #region 获取区域
    /// <summary>
    /// 获取区域
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<EG_WMS_Region> Get([FromQuery] QueryByIdEGRegionInput input)
    {
        //return await _rep.GetFirstAsync(u => u.Id == input.Id);
        
        // 模糊查询
        return await _rep.GetFirstAsync(u => u.RegionName.Contains(input.RegionName) || u.RegionNum.Contains(input.RegionNum));
    }
    #endregion

    #region 获取区域列表
    /// <summary>
    /// 获取区域列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGRegionOutput>> List([FromQuery] EGRegionInput input)
    {
        return await _rep.AsQueryable().Select<EGRegionOutput>().ToListAsync();
    }
    #endregion

    #region 获取仓库名称列表
    /// <summary>
    /// 获取仓库名称列表
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "EGWareHouseWHNameDropdown"), HttpGet]
    public async Task<dynamic> EGWareHouseWHNumDropdown()
    {
        return await _rep.Context.Queryable<EG_WMS_WareHouse>()
                .Select(u => new
                {
                    ISWareHouse = u.WHName,
                    Value = u.Id
                }
                ).ToListAsync();
    }
    #endregion


}
