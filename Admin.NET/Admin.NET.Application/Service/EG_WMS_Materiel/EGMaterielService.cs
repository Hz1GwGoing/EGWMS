namespace Admin.NET.Application;

/// <summary>
/// 物料管理接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGMaterielService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<EG_WMS_Materiel> _rep;
    private readonly SqlSugarRepository<EG_WMS_Inventory> _Inventory = App.GetService<SqlSugarRepository<EG_WMS_Inventory>>();
    private readonly SqlSugarRepository<EG_WMS_WorkBin> _WorkBin = App.GetService<SqlSugarRepository<EG_WMS_WorkBin>>();

    public EGMaterielService(SqlSugarRepository<EG_WMS_Materiel> rep)
    {
        _rep = rep;
    }

    #region 修改物料安全库存

    /// <summary>
    /// 修改物料安全库存
    /// </summary>
    /// <param name="materielnum">物料编号</param>
    /// <param name="SafetyCount">安全库存（不传参数时为默认null，既不设置安全库存提醒）</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "UpdateSafetyCount")]
    public async Task UpdateSafetyCount(string materielnum, int? SafetyCount = null)
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

    #region 查询物料是否存在安全库存里

    /// <summary>
    /// 查询物料是否存在安全库存里
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "MaterialSafetyInventory")]
    public List<InventoryEarlyWarningDto> MaterialSafetyInventory()
    {
        List<InventoryEarlyWarningDto> inventories = new List<InventoryEarlyWarningDto>();

        // 查询每种物料的需在库数量
        var materieldata = _rep.AsQueryable()
                               .Where(x => x.IsDelete == false && x.QuantityNeedCount != null)
                               .GroupBy(x => x.MaterielNum)
                               .Distinct()
                               .Select(x => new { x.MaterielNum, x.MaterielName, x.MaterielSpecs, x.QuantityNeedCount })
                               .ToList();

        for (int i = 0; i < materieldata.Count; i++)
        {
            // 查询在库数据
            int? invmaterielcount = _Inventory.AsQueryable()
                                             .Where(x => x.MaterielNum == materieldata[i].MaterielNum && x.OutboundStatus == 0)
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

    #region 物料在库时间管控（提前1/4预警时间）

    /// <summary>
    /// 物料在库时间管控（提前1/4预警时间）
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "EarlyWarningTimeOfMaterielStorage")]
    public async Task<List<MaterielStorageTimeWarringDto>> EarlyWarningTimeOfMaterielStorage()
    {
        List<MaterielStorageTimeWarringDto> datas = new List<MaterielStorageTimeWarringDto>();
        // 查询在库数据
        var _invdata = _Inventory.AsQueryable()
                                 .InnerJoin<EG_WMS_InventoryDetail>((a, b) => a.InventoryNum == b.InventoryNum)
                                 .InnerJoin<EG_WMS_Materiel>((a, b, c) => a.MaterielNum == c.MaterielNum)
                                 .Where((a, b, c) => a.OutboundStatus == 0 && c.InventoryDateTime != null)
                                 .GroupBy((a, b, c) => a.MaterielNum)
                                 .Select((a, b, c) => new
                                 {
                                     a.MaterielNum,
                                     a.ICountAll,
                                     a.ProductionDate,
                                     b.StorageNum,
                                     b.WorkBinNum,
                                     c.MaterielName,
                                 })
                                 .ToList();

        DateTime oldtime = new DateTime();
        DateTime newtime = new DateTime();
        DateTime newtimequarter = new DateTime();
        DateTime nowtime = DateTime.Now;
        for (int i = 0; i < _invdata.Count; i++)
        {
            // 查询当前物料的预警时间
            EG_WMS_Materiel _materieldata = await _rep.GetFirstAsync(x => x.MaterielNum == _invdata[i].MaterielNum);
            // 生产时间
            oldtime = (DateTime)_invdata[i].ProductionDate;
            // 生产时间加上提醒时间
            newtime = oldtime.AddHours((double)_materieldata.InventoryDateTime);
            // 生产时间加上提醒时间减去提醒时间的四分之一
            newtimequarter = newtime.AddHours(-((double)_materieldata.InventoryDateTime) / 4);
            // 在生产时间加上提醒时间和生产时间加上提醒时间减去提醒时间四分之一之间，提前提醒用户哪些库存快要到提醒时间
            if (nowtime < newtime && nowtime >= newtimequarter)
            {
                MaterielStorageTimeWarringDto materieldata = new MaterielStorageTimeWarringDto()
                {
                    MaterielNum = _invdata[i].MaterielNum,
                    MaterielName = _invdata[i].MaterielName,
                    WorkBin = _invdata[i].WorkBinNum,
                    Icount = (int)_invdata[i].ICountAll,
                    StorageNum = _invdata[i].StorageNum,
                    InventoryTime = oldtime,
                    EarlyWarningTime = newtime,
                };
                datas.Add(materieldata);
            }
        }
        return datas;
    }



    #endregion

    #region 物料在库时间管控（超过预警时间）

    /// <summary>
    /// 物料在库时间管控（超过预警时间）
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "MaterialInStockTimeControlExceed")]
    public async Task<List<MaterielStorageTimeWarringDto>> MaterialInStockTimeControlExceed()
    {
        List<MaterielStorageTimeWarringDto> datas = new List<MaterielStorageTimeWarringDto>();
        // 查询在库数据
        var _invdata = _Inventory.AsQueryable()
                                 .InnerJoin<EG_WMS_InventoryDetail>((a, b) => a.InventoryNum == b.InventoryNum)
                                 .InnerJoin<EG_WMS_Materiel>((a, b, c) => a.MaterielNum == c.MaterielNum)
                                 .Where((a, b, c) => a.OutboundStatus == 0 && c.InventoryDateTime != null)
                                 .GroupBy((a, b, c) => a.MaterielNum)
                                 .Select((a, b, c) => new
                                 {
                                     a.MaterielNum,
                                     a.ICountAll,
                                     a.ProductionDate,
                                     b.StorageNum,
                                     b.WorkBinNum,
                                     c.MaterielName
                                 })
                                 .ToList();

        DateTime oldtime = new DateTime();
        DateTime newtime = new DateTime();
        DateTime nowtime = DateTime.Now;
        for (int i = 0; i < _invdata.Count; i++)
        {
            // 查询当前物料的预警时间
            EG_WMS_Materiel _materieldata = await _rep.GetFirstAsync(x => x.MaterielNum == _invdata[i].MaterielNum);
            // 生产时间
            oldtime = (DateTime)_invdata[i].ProductionDate;
            // 生产时间加上提醒时间
            newtime = oldtime.AddHours((double)_materieldata.InventoryDateTime);
            // 已经超过在库时间
            if (newtime <= nowtime)
            {
                MaterielStorageTimeWarringDto materieldata = new MaterielStorageTimeWarringDto()
                {
                    MaterielNum = _invdata[i].MaterielNum,
                    MaterielName = _invdata[i].MaterielName,
                    WorkBin = _invdata[i].WorkBinNum,
                    Icount = (int)_invdata[i].ICountAll,
                    StorageNum = _invdata[i].StorageNum,
                    InventoryTime = oldtime,
                    EarlyWarningTime = newtime,
                };
                datas.Add(materieldata);
            }
        }
        return datas;
    }


    #endregion

    #region 修改物料在库时间提醒

    /// <summary>
    /// 修改物料在库时间提醒
    /// </summary>
    /// <param name="materielnum">物料编号</param>
    /// <param name="hour">预警时间/单位：小时（不传参数时为默认null，既不设置在库时间提醒）</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "UpdateWarningMaterialTimeInStorage")]
    public async Task UpdateWarningMaterialTimeInStorage(string materielnum, double? hour = null)
    {
        var materieldata = await _rep.GetFirstAsync(x => x.MaterielNum == materielnum);
        if (materieldata == null)
        {
            throw Oops.Oh("物料编号有误，查询不到该种物料");
        }
        if (hour < 0)
        {
            throw Oops.Oh("预警时间不能小于0");
        }

        await _rep.AsUpdateable()
                  .SetColumns(it => new EG_WMS_Materiel
                  {
                      InventoryDateTime = hour,
                      UpdateTime = DateTime.Now,
                  })
                  .Where(x => x.MaterielNum == materielnum)
                  .ExecuteCommandAsync();
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
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielSource), u => u.MaterielSource.Contains(input.MaterielSource.Trim()))
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
        var data = await _rep.GetFirstAsync(x => x.MaterielNum == input.MaterielNum);
        if (data != null)
        {
            throw Oops.Oh(ErrorCodeEnum.N1001);
        }

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
         .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielSpecs), x => x.MaterielSpecs.Contains(input.MaterielSpecs))
         .Skip((input.page - 1) * input.pageSize)
         .Take(input.pageSize);

        return await datatwo.ToPagedListAsync(input.page, input.pageSize);

    }

    #endregion

    //-------------------------------------//-------------------------------------//


}

