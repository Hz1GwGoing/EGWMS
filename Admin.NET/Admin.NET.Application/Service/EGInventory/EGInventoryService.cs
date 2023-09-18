using Admin.NET.Application.Const;
using Admin.NET.Application.Entity;
using Admin.NET.Core;
using Furion.DependencyInjection;
using Furion.FriendlyException;
using System.Collections.Generic;

namespace Admin.NET.Application;
/// <summary>
/// 库存主表接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGInventoryService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<EGInventory> _rep;
    public EGInventoryService(SqlSugarRepository<EGInventory> rep)
    {
        _rep = rep;
    }

    /// <summary>
    /// 分页查询库存主表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<EGInventoryOutput>> Page(EGInventoryInput input)
    {
        var query = _rep.AsQueryable()
                    .WhereIF(!string.IsNullOrWhiteSpace(input.InventoryNum), u => u.InventoryNum.Contains(input.InventoryNum.Trim()))
                    .WhereIF(input.ICountAll > 0, u => u.ICountAll == input.ICountAll)
                    .WhereIF(input.IUsable > 0, u => u.IUsable == input.IUsable)
                    .WhereIF(input.IFrostCount > 0, u => u.IFrostCount == input.IFrostCount)
                    .WhereIF(input.IWaitingCount > 0, u => u.IWaitingCount == input.IWaitingCount)

                    // 获取创建日期
                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
                    .Select<EGInventoryOutput>()
;
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加库存主表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddEGInventoryInput input)
    {
        var entity = input.Adapt<EGInventory>();
        await _rep.InsertAsync(entity);
    }

    /// <summary>
    /// 删除库存主表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteEGInventoryInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
    }

    /// <summary>
    /// 更新库存主表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGInventoryInput input)
    {
        var entity = input.Adapt<EGInventory>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取库存主表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<EGInventory> Get([FromQuery] QueryByIdEGInventoryInput input)
    {
        //return await _rep.GetFirstAsync(u => u.Id == input.Id);

        // 模糊查询
        return await _rep.GetFirstAsync(u => u.InventoryNum.Contains(input.InventoryNum));

    }

    /// <summary>
    /// 获取库存主表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGInventoryOutput>> List([FromQuery] EGInventoryInput input)
    {
        return await _rep.AsQueryable().Select<EGInventoryOutput>().ToListAsync();
    }





}

