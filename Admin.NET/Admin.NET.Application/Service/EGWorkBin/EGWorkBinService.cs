using Admin.NET.Application.Const;
using Admin.NET.Application.Entity;
using Admin.NET.Core;
using Furion.DependencyInjection;
using Furion.FriendlyException;
using System.Collections.Generic;

namespace Admin.NET.Application;
/// <summary>
/// 料箱信息接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGWorkBinService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<EGWorkBin> _rep;
    public EGWorkBinService(SqlSugarRepository<EGWorkBin> rep)
    {
        _rep = rep;
    }

    /// <summary>
    /// 分页查询料箱信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<EGWorkBinOutput>> Page(EGWorkBinInput input)
    {
        var query= _rep.AsQueryable()
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinNum), u => u.WorkBinNum.Contains(input.WorkBinNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinName), u => u.WorkBinName.Contains(input.WorkBinName.Trim()))
                    .WhereIF(input.WorkBinSpecs>0, u => u.WorkBinSpecs == input.WorkBinSpecs)
                    .WhereIF(input.MachineNum>0, u => u.MachineNum == input.MachineNum)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.Classes), u => u.Classes.Contains(input.Classes.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.ProductionLot), u => u.ProductionLot.Contains(input.ProductionLot.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.ProductionStaff), u => u.ProductionStaff.Contains(input.ProductionStaff.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.Inspector), u => u.Inspector.Contains(input.Inspector.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.Printer), u => u.Printer.Contains(input.Printer.Trim()))
                    .WhereIF(input.WorkBinStatus>0, u => u.WorkBinStatus == input.WorkBinStatus)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), u => u.MaterielNum.Contains(input.MaterielNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinRemake), u => u.WorkBinRemake.Contains(input.WorkBinRemake.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.StorageNum), u => u.StorageNum.Contains(input.StorageNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.PalletNum), u => u.PalletNum.Contains(input.PalletNum.Trim()))

                    // 获取创建日期
                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
                    .Select<EGWorkBinOutput>()
;
        if(input.ProductionDateRange != null && input.ProductionDateRange.Count >0)
        {
                DateTime? start= input.ProductionDateRange[0]; 
                query = query.WhereIF(start.HasValue, u => u.ProductionDate > start);
                if (input.ProductionDateRange.Count >1 && input.ProductionDateRange[1].HasValue)
                {
                    var end = input.ProductionDateRange[1].Value.AddDays(1);
                    query = query.Where(u => u.ProductionDate < end);
                }
        } 
        if(input.PrintTimeRange != null && input.PrintTimeRange.Count >0)
        {
                DateTime? start= input.PrintTimeRange[0]; 
                query = query.WhereIF(start.HasValue, u => u.PrintTime > start);
                if (input.PrintTimeRange.Count >1 && input.PrintTimeRange[1].HasValue)
                {
                    var end = input.PrintTimeRange[1].Value.AddDays(1);
                    query = query.Where(u => u.PrintTime < end);
                }
        } 
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加料箱信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddEGWorkBinInput input)
    {
        var entity = input.Adapt<EGWorkBin>();
        await _rep.InsertAsync(entity);
    }

    /// <summary>
    /// 删除料箱信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteEGWorkBinInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
    }

    /// <summary>
    /// 更新料箱信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGWorkBinInput input)
    {
        var entity = input.Adapt<EGWorkBin>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取料箱信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<EGWorkBin> Get([FromQuery] QueryByIdEGWorkBinInput input)
    {
        //return await _rep.GetFirstAsync(u => u.Id == input.Id);

        // 模糊查询
        return await _rep.GetFirstAsync(u => u.WorkBinNum.Contains(input.WorkBinNum) || u.WorkBinName.Contains(input.WorkBinName));

    }

    /// <summary>
    /// 获取料箱信息列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGWorkBinOutput>> List([FromQuery] EGWorkBinInput input)
    {
        return await _rep.AsQueryable().Select<EGWorkBinOutput>().ToListAsync();
    }





}

