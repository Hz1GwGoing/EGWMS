using Admin.NET.Application.Const;
using Admin.NET.Application.Entity;
using Admin.NET.Core;
using Furion.DependencyInjection;
using Furion.FriendlyException;
using System.Collections.Generic;

namespace Admin.NET.Application;
/// <summary>
/// 移库信息接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGRelocationService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<EGRelocation> _rep;
    public EGRelocationService(SqlSugarRepository<EGRelocation> rep)
    {
        _rep = rep;
    }

    /// <summary>
    /// 分页查询移库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<EGRelocationOutput>> Page(EGRelocationInput input)
    {
        var query = _rep.AsQueryable()
                    .WhereIF(!string.IsNullOrWhiteSpace(input.RelocatioNum), u => u.RelocatioNum.Contains(input.RelocatioNum.Trim()))
                    .WhereIF(input.RelocationType > 0, u => u.RelocationType == input.RelocationType)
                    .WhereIF(input.RelocationCount > 0, u => u.RelocationCount == input.RelocationCount)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.RelocationUser), u => u.RelocationUser.Contains(input.RelocationUser.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WHNum), u => u.WHNum.Contains(input.WHNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.PalletNum), u => u.PalletNum.Contains(input.PalletNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinNum), u => u.WorkBinNum.Contains(input.WorkBinNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.Relocation), u => u.Relocation.Contains(input.Relocation.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), u => u.MaterielNum.Contains(input.MaterielNum.Trim()))

                    // 获取创建日期
                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
                    .Select<EGRelocationOutput>()
;
        if (input.RelocationTimeRange != null && input.RelocationTimeRange.Count > 0)
        {
            DateTime? start = input.RelocationTimeRange[0];
            query = query.WhereIF(start.HasValue, u => u.RelocationTime > start);
            if (input.RelocationTimeRange.Count > 1 && input.RelocationTimeRange[1].HasValue)
            {
                var end = input.RelocationTimeRange[1].Value.AddDays(1);
                query = query.Where(u => u.RelocationTime < end);
            }
        }
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加移库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddEGRelocationInput input)
    {
        var entity = input.Adapt<EGRelocation>();
        await _rep.InsertAsync(entity);
    }

    /// <summary>
    /// 删除移库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteEGRelocationInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
    }

    /// <summary>
    /// 更新移库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGRelocationInput input)
    {
        var entity = input.Adapt<EGRelocation>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取移库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<EGRelocation> Get([FromQuery] QueryByIdEGRelocationInput input)
    {
        //return await _rep.GetFirstAsync(u => u.Id == input.Id);

        // 模糊查询
        return await _rep.GetFirstAsync(u => u.RelocatioNum.Contains(input.RelocatioNum));

    }

    /// <summary>
    /// 获取移库信息列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGRelocationOutput>> List([FromQuery] EGRelocationInput input)
    {
        return await _rep.AsQueryable().Select<EGRelocationOutput>().ToListAsync();
    }





}

