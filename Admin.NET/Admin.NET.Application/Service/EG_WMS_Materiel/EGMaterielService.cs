namespace Admin.NET.Application;

/// <summary>
/// 物料管理接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGMaterielService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<EG_WMS_Materiel> _rep;
    private readonly SqlSugarRepository<EG_WMS_Inventory> _Inventory = App.GetService<SqlSugarRepository<EG_WMS_Inventory>>();

    public EGMaterielService(SqlSugarRepository<EG_WMS_Materiel> rep)
    {
        _rep = rep;
    }

    #region 修改物料安全库存

    /// <summary>
    /// 修改物料安全库存
    /// </summary>
    /// <param name="materielnum">物料编号</param>
    /// <param name="SafetyCount">安全库存</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "UpdateSafetyCount")]
    public async Task UpdateSafetyCount(string materielnum, int SafetyCount)
    {
        if (SafetyCount == 0 || SafetyCount < 0)
        {
            throw Oops.Oh("安全库存数量必须大于零");
        }
        var materieldata = await _rep.GetFirstAsync(x => x.MaterielNum == materielnum);
        if (materieldata == null)
        {
            throw Oops.Oh("物料编号有误，查询不到该种物料");
        }
        await _rep.AsUpdateable()
                  .SetColumns(it => new EG_WMS_Materiel
                  {
                      QuantityNeedCount = SafetyCount,
                      UpdateTime = DateTime.Now,
                  })
                  .Where(x => x.MaterielNum == materielnum)
                  .ExecuteCommandAsync();
    }


    #endregion

    #region 查询物料是否不在安全库存里

    /// <summary>
    /// 查询物料是否不在安全库存里
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "MaterialSafetyInventory")]
    public List<InventoryEarlyWarningDto> MaterialSafetyInventory()
    {
        List<InventoryEarlyWarningDto> inventories = new List<InventoryEarlyWarningDto>();

        // 查询每种物料的需在库数量
        var materieldata = _rep.AsQueryable()
                               .Where(x => x.IsDelete == false)
                               .GroupBy(x => x.MaterielNum)
                               .Distinct()
                               .Select(x => new { x.MaterielNum, x.MaterielName, x.MaterielSpecs, x.QuantityNeedCount })
                               .ToList();

        for (int i = 0; i < materieldata.Count; i++)
        {
            // 查询在库数据
            int? invmaterielcount = _Inventory.AsQueryable()
                                             .Where(x => x.MaterielNum == materieldata[i].MaterielNum)
                                             .Sum(x => x.ICountAll);

            if (invmaterielcount < materieldata[i].QuantityNeedCount || invmaterielcount == null)
            {
                if (invmaterielcount == null)
                {
                    invmaterielcount = 0;
                }
                InventoryEarlyWarningDto invEarlyData = new InventoryEarlyWarningDto()
                {
                    MaterielNum = materieldata[i].MaterielNum,
                    MaterielName = materieldata[i].MaterielName,
                    MaterielSpecs = materieldata[i].MaterielSpecs,
                    InventoryCount = invmaterielcount,
                };
                inventories.Add(invEarlyData);
            }
        }
        return inventories;
    }

    #endregion

    #region 分页查询物料信息
    /// <summary>
    /// 分页查询物料信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<EGMaterielOutput>> Page(EGMaterielInput input)
    {
        var query = _rep.AsQueryable()
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), u => u.MaterielNum.Contains(input.MaterielNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielName), u => u.MaterielName.Contains(input.MaterielName.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielType), u => u.MaterielType.Contains(input.MaterielType.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielSpecs), u => u.MaterielSpecs.Contains(input.MaterielSpecs.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielDescribe), u => u.MaterielDescribe.Contains(input.MaterielDescribe.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielSource), u => u.MaterielSource.Contains(input.MaterielSource.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.CreateUserName), u => u.CreateUserName.Contains(input.CreateUserName.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.UpdateUserName), u => u.UpdateUserName.Contains(input.UpdateUserName.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielReamke), u => u.MaterielReamke.Contains(input.MaterielReamke.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielMainUnit), u => u.MaterielMainUnit.Contains(input.MaterielMainUnit.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielAssistUnit), u => u.MaterielAssistUnit.Contains(input.MaterielAssistUnit.Trim()))

                    // 获取创建日期
                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
                    .OrderBy(u => u.Id)
                    .Select<EGMaterielOutput>()
;
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }
    #endregion

    #region 增加物料信息
    /// <summary>
    /// 增加物料信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddEGMaterielInput input)
    {
        var entity = input.Adapt<EG_WMS_Materiel>();
        await _rep.InsertAsync(entity);
    }
    #endregion

    #region 删除物料信息（软删除）

    /// <summary>
    /// 删除物料信息（软删除）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteEGMaterielInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
    }
    #endregion

    #region 更新物料信息

    /// <summary>
    /// 更新物料信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGMaterielInput input)
    {
        var entity = input.Adapt<EG_WMS_Materiel>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }
    #endregion

    #region 根据物料编号名称类别筛选物料

    /// <summary>
    /// 根据物料编号名称类别筛选物料（模糊查询）（分页查询）
    /// 没有传递条件时返回所有物料信息
    /// </summary>
    /// <param name="input">编号、名称、类别、规格</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "GetMaterielsDataAndNumNameSpecs")]
    public async Task<SqlSugarPagedList<EG_WMS_Materiel>> GetMaterielsDataAndNumNameSpecs(EGMaterielDto input)
    {
        if (input.MaterielNum == null && input.MaterielName == null && input.MaterielType == null && input.MaterielSpecs == null)
        {
            var dataone = _rep.AsQueryable()
             .Skip((input.page - 1) * input.pageSize)
             .Take(input.pageSize);


            return await dataone.ToPagedListAsync(input.page, input.pageSize);
        }

        var datatwo = _rep.AsQueryable()
         .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), x => x.MaterielNum == input.MaterielNum)
         .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielName), x => x.MaterielName.Contains(input.MaterielName))
         .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielType), x => x.MaterielType.Contains(input.MaterielType))
         .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielSpecs), x => x.MaterielType.Contains(input.MaterielSpecs))
         .Skip((input.page - 1) * input.pageSize)
         .Take(input.pageSize);

        return await datatwo.ToPagedListAsync(input.page, input.pageSize);

    }

    #endregion

    //-------------------------------------//-------------------------------------//


}

