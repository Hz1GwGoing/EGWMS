using Admin.NET.Application.Const;
using Admin.NET.Application.Entity;
using Admin.NET.Core;
using Furion.DependencyInjection;
using Furion.FriendlyException;
using System.Collections.Generic;

namespace Admin.NET.Application;
/// <summary>
/// 出库信息接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGOutBoundService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<EGOutBound> _rep;
    public EGOutBoundService(SqlSugarRepository<EGOutBound> rep)
    {
        _rep = rep;
    }

    /// <summary>
    /// 分页查询出库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<EGOutBoundOutput>> Page(EGOutBoundInput input)
    {
        var query = _rep.AsQueryable()
                    .WhereIF(!string.IsNullOrWhiteSpace(input.OutboundNum), u => u.OutboundNum.Contains(input.OutboundNum.Trim()))
                    .WhereIF(input.OutboundType > 0, u => u.OutboundType == input.OutboundType)
                    .WhereIF(input.OutboundCount > 0, u => u.OutboundCount == input.OutboundCount)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.OutboundUser), u => u.OutboundUser.Contains(input.OutboundUser.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WHNum), u => u.WHNum.Contains(input.WHNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.PalletNum), u => u.PalletNum.Contains(input.PalletNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinNum), u => u.WorkBinNum.Contains(input.WorkBinNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.OutboundRemake), u => u.OutboundRemake.Contains(input.OutboundRemake.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), u => u.MaterielNum.Contains(input.MaterielNum.Trim()))

                    // 获取创建日期
                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
                    .Select<EGOutBoundOutput>()
;
        if (input.OutboundTimeRange != null && input.OutboundTimeRange.Count > 0)
        {
            DateTime? start = input.OutboundTimeRange[0];
            query = query.WhereIF(start.HasValue, u => u.OutboundTime > start);
            if (input.OutboundTimeRange.Count > 1 && input.OutboundTimeRange[1].HasValue)
            {
                var end = input.OutboundTimeRange[1].Value.AddDays(1);
                query = query.Where(u => u.OutboundTime < end);
            }
        }
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加出库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddEGOutBoundInput input)
    {
        var entity = input.Adapt<EGOutBound>();
        await _rep.InsertAsync(entity);
    }

    /// <summary>
    /// 删除出库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteEGOutBoundInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
    }

    /// <summary>
    /// 更新出库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGOutBoundInput input)
    {
        var entity = input.Adapt<EGOutBound>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取出库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<EGOutBound> Get([FromQuery] QueryByIdEGOutBoundInput input)
    {
        //return await _rep.GetFirstAsync(u => u.Id == input.Id);

        // 模糊查询
        return await _rep.GetFirstAsync(u => u.OutboundNum.Contains(input.OutboundNum));

    }

    /// <summary>
    /// 获取出库信息列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGOutBoundOutput>> List([FromQuery] EGOutBoundInput input)
    {
        return await _rep.AsQueryable().Select<EGOutBoundOutput>().ToListAsync();
    }





}

