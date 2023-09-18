using Admin.NET.Application.Const;
using Admin.NET.Application.Entity;
using Admin.NET.Core;
using Furion.DependencyInjection;
using Furion.FriendlyException;
using System.Collections.Generic;

namespace Admin.NET.Application;
/// <summary>
/// 入库信息接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGJoinBoundService : IDynamicApiController, ITransient
{
    #region 引用实体
    private readonly SqlSugarRepository<EGJoinBound> _rep;

    #endregion

    // 库存
    EGInventoryService _inventoryService;

    public EGJoinBoundService(SqlSugarRepository<EGJoinBound> rep)
    {
        _rep = rep;
    }

    #region 分页查询入库信息
    /// <summary>
    /// 分页查询入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<EGJoinBoundOutput>> Page(EGJoinBoundInput input)
    {
        var query = _rep.AsQueryable()
                    .WhereIF(!string.IsNullOrWhiteSpace(input.JoinBoundNum), u => u.JoinBoundNum.Contains(input.JoinBoundNum.Trim()))
                    .WhereIF(input.JoinBoundType > 0, u => u.JoinBoundType == input.JoinBoundType)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.JoinBoundUser), u => u.JoinBoundUser.Contains(input.JoinBoundUser.Trim()))
                    .WhereIF(input.JoinBoundCount > 0, u => u.JoinBoundCount == input.JoinBoundCount)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WHNum), u => u.WHNum.Contains(input.WHNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.PalletNum), u => u.PalletNum.Contains(input.PalletNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinNum), u => u.WorkBinNum.Contains(input.WorkBinNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.JoinBoundRemake), u => u.JoinBoundRemake.Contains(input.JoinBoundRemake.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), u => u.MaterielNum.Contains(input.MaterielNum.Trim()))
                    // 获取创建日期
                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
                    .Select<EGJoinBoundOutput>()
;
        if (input.JoinBoundTimeRange != null && input.JoinBoundTimeRange.Count > 0)
        {
            DateTime? start = input.JoinBoundTimeRange[0];
            query = query.WhereIF(start.HasValue, u => u.JoinBoundTime > start);
            if (input.JoinBoundTimeRange.Count > 1 && input.JoinBoundTimeRange[1].HasValue)
            {
                var end = input.JoinBoundTimeRange[1].Value.AddDays(1);
                query = query.Where(u => u.JoinBoundTime < end);
            }
        }
        if (input.JoinBoundOutTimeRange != null && input.JoinBoundOutTimeRange.Count > 0)
        {
            DateTime? start = input.JoinBoundOutTimeRange[0];
            query = query.WhereIF(start.HasValue, u => u.JoinBoundOutTime > start);
            if (input.JoinBoundOutTimeRange.Count > 1 && input.JoinBoundOutTimeRange[1].HasValue)
            {
                var end = input.JoinBoundOutTimeRange[1].Value.AddDays(1);
                query = query.Where(u => u.JoinBoundOutTime < end);
            }
        }
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    #endregion

    #region 增加入库信息

    /// <summary>
    /// 增加入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    //[HttpPost]
    //[ApiDescriptionSettings(Name = "Add")]
    //public async Task Add(AddEGJoinBoundInput input)
    //{
    //    // 获得入库时的库存总数
    //    //int inventoryCount = (int)input.ICountAll;

    //    var entity = input.Adapt<EGJoinBound>();
    //    await _rep.InsertAsync(entity);

    //}

    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddEGJoinBoundInput input)
    {
        // 获得入库时的库存总数
        //int inventoryCount = (int)input.ICountAll;

        var entity = input.Adapt<EGJoinBound>();
        await _rep.InsertAsync(entity);
        

    }
    #endregion

    #region 删除入库信息
    /// <summary>
    /// 删除入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteEGJoinBoundInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
    }
    #endregion

    #region 更新入库信息
    /// <summary>
    /// 更新入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGJoinBoundInput input)
    {
        var entity = input.Adapt<EGJoinBound>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }
    #endregion

    #region 获取入库信息
    /// <summary>
    /// 获取入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<EGJoinBound> Get([FromQuery] QueryByIdEGJoinBoundInput input)
    {
        //return await _rep.GetFirstAsync(u => u.Id == input.Id);

        // 模糊查询（入库编号）
        return await _rep.GetFirstAsync(u => u.JoinBoundNum.Contains(input.JoinBoundNum));

    }
    #endregion

    #region 获取入库信息列表
    /// <summary>
    /// 获取入库信息列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGJoinBoundOutput>> List([FromQuery] EGJoinBoundInput input)
    {
        return await _rep.AsQueryable().Select<EGJoinBoundOutput>().ToListAsync();
    }
    #endregion


}

