using Admin.NET.Application.Const;
using Admin.NET.Application.Entity;
using Admin.NET.Core;
using Furion.DependencyInjection;
using Furion.FriendlyException;
using System.Collections.Generic;

namespace Admin.NET.Application;
/// <summary>
/// 盘点信息接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGTakeStockService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<EGTakeStock> _rep;
    public EGTakeStockService(SqlSugarRepository<EGTakeStock> rep)
    {
        _rep = rep;
    }

    /// <summary>
    /// 分页查询盘点信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<EGTakeStockOutput>> Page(EGTakeStockInput input)
    {
        var query= _rep.AsQueryable()
                    .WhereIF(!string.IsNullOrWhiteSpace(input.TakeStockNum), u => u.TakeStockNum.Contains(input.TakeStockNum.Trim()))
                    .WhereIF(input.TakeStockStatus>0, u => u.TakeStockStatus == input.TakeStockStatus)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.TakeStockUser), u => u.TakeStockUser.Contains(input.TakeStockUser.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.TakeStockRemake), u => u.TakeStockRemake.Contains(input.TakeStockRemake.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), u => u.MaterielNum.Contains(input.MaterielNum.Trim()))
                    // 获取创建日期
                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
                    .Select<EGTakeStockOutput>()
;
        if(input.TakeStockTimeRange != null && input.TakeStockTimeRange.Count >0)
        {
                DateTime? start= input.TakeStockTimeRange[0]; 
                query = query.WhereIF(start.HasValue, u => u.TakeStockTime > start);
                if (input.TakeStockTimeRange.Count >1 && input.TakeStockTimeRange[1].HasValue)
                {
                    var end = input.TakeStockTimeRange[1].Value.AddDays(1);
                    query = query.Where(u => u.TakeStockTime < end);
                }
        } 
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加盘点信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddEGTakeStockInput input)
    {
        var entity = input.Adapt<EGTakeStock>();
        await _rep.InsertAsync(entity);
    }

    /// <summary>
    /// 删除盘点信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteEGTakeStockInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
    }

    /// <summary>
    /// 更新盘点信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGTakeStockInput input)
    {
        var entity = input.Adapt<EGTakeStock>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取盘点信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<EGTakeStock> Get([FromQuery] QueryByIdEGTakeStockInput input)
    {
        //return await _rep.GetFirstAsync(u => u.Id == input.Id);

        // 模糊查询
        return await _rep.GetFirstAsync(u => u.TakeStockNum.Contains(input.TakeStockNum));

    }

    /// <summary>
    /// 获取盘点信息列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGTakeStockOutput>> List([FromQuery] EGTakeStockInput input)
    {
        return await _rep.AsQueryable().Select<EGTakeStockOutput>().ToListAsync();
    }





}

