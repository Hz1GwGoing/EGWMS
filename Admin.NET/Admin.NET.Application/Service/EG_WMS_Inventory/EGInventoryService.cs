namespace Admin.NET.Application;
/// <summary>
/// 库存主表接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGInventoryService : IDynamicApiController, ITransient
{
    #region 实体引用


    private readonly SqlSugarRepository<EG_WMS_Inventory> _rep;
    private readonly SqlSugarRepository<EG_WMS_Materiel> _materiel;
    #endregion

    #region 关系注入

    public EGInventoryService
        (
        SqlSugarRepository<EG_WMS_Inventory> rep,
        SqlSugarRepository<EG_WMS_Materiel> materiel
        )
    {
        _rep = rep;
        _materiel = materiel;
    }
    #endregion

    #region 分页查询库存主表
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
                    // 库存编号
                    //.WhereIF(!string.IsNullOrWhiteSpace(input.InventoryNum), u => u.InventoryNum.Contains(input.InventoryNum.Trim()))
                    .WhereIF(input.ICountAll > 0, u => u.ICountAll == input.ICountAll)
                    .WhereIF(input.IUsable > 0, u => u.IUsable == input.IUsable)
                    .WhereIF(input.IFrostCount > 0, u => u.IFrostCount == input.IFrostCount)
                    .WhereIF(input.IWaitingCount > 0, u => u.IWaitingCount == input.IWaitingCount)
                    // 物料编号
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), u => u.MaterielNum == input.MaterielNum.Trim())
                    // 备注
                    .WhereIF(!string.IsNullOrWhiteSpace(input.InventoryRemake), u => u.InventoryRemake == input.InventoryRemake.Trim())
                    // 出库状态
                    .WhereIF(input.OutboundStatus > 0, u => u.OutboundStatus == input.OutboundStatus)
                    // 获取创建日期
                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
                    // 倒序
                    .OrderBy(it => it.CreateTime, OrderByType.Desc)
                    .Select<EGInventoryOutput>()
;
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }
    #endregion

    #region 更新库存主表
    /// <summary>
    /// 更新库存主表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGInventoryInput input)
    {
        var entity = input.Adapt<EG_WMS_Inventory>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }
    #endregion

    #region 模糊查询符条件的数据（根据物料名称、编号、类别）
    /// <summary>
    /// 模糊查询符条件的数据（根据物料名称、编号、类别）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<List<EGInventoryAndMaterielDto>> Detail(QueryByIdEGInventoryInput input)
    {
        List<string> materielNums = new List<string>();
        List<EGInventoryAndMaterielDto> EGInventoryAndMaterielData = new List<EGInventoryAndMaterielDto>();

        try
        {
            var materielsData = _materiel.AsQueryable()
                                         .Where(u => u.MaterielName.Contains(input.MaterielName) || u.MaterielNum.Contains(input.MaterielNum) || u.MaterielType.Contains(input.MaterielType))
                                         .ToList();


            if (materielsData.Count != 0)
            {
                // 得到模糊查询里面的物料编号  
                foreach (var item in materielsData)
                {
                    materielNums.Add(item.MaterielNum);
                }

                for (int i = 0; i < materielNums.Count; i++)
                {
                    string num = materielNums[i];

                    try
                    {
                        var materieItem = await _materiel.GetFirstAsync(u => u.MaterielNum == num);
                        var inventoryItem = await _rep.GetFirstAsync(u => u.MaterielNum == num);
                        // 物料名称  
                        string materielnum = materieItem.MaterielNum;
                        // 物料名称  
                        string materiename = materieItem.MaterielName;
                        // 规格  
                        string? materielspecs = materieItem.MaterielSpecs;
                        // 在库数量  
                        int? icountall = inventoryItem.ICountAll;
                        // 可用数量  
                        int? iusable = inventoryItem.IUsable;

                        EGInventoryAndMaterielDto inventory = new EGInventoryAndMaterielDto()
                        {
                            MaterielNum = materielnum,
                            MaterielName = materiename,
                            MaterielSpecs = materielspecs,
                            ICountAll = icountall,
                            IUsable = iusable,
                        };
                        EGInventoryAndMaterielData.Add(inventory);
                    }
                    catch (Exception ex)
                    {
                        throw Oops.Oh(ex.Message, ex);
                    }
                }
            }
            else
            {
                throw Oops.Oh("查询不到记录");
            }
        }
        catch (Exception ex)
        {
            throw Oops.Oh(ex.Message);
        }
        return EGInventoryAndMaterielData;
    }

    #endregion

    #region 获取库存主表列表
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

    #endregion

    //-------------------------------------//-------------------------------------//

    #region 增加库存主表
    /// <summary>
    /// 增加库存主表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    //[HttpPost]
    //[ApiDescriptionSettings(Name = "Add")]
    //public async Task Add(AddEGInventoryInput input)
    //{
    //    var entity = input.Adapt<EGInventory>();
    //    await _rep.InsertAsync(entity);
    //}
    #endregion

    #region 删除库存主表
    /// <summary>
    /// 删除库存主表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    //[HttpPost]
    //[ApiDescriptionSettings(Name = "Delete")]
    //public async Task Delete(DeleteEGInventoryInput input)
    //{
    //    var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
    //    await _rep.FakeDeleteAsync(entity);   //假删除
    //}
    #endregion

    #region 模糊查询（根据物料编号和物料名称和物料规格查询）
    //public async Task<List<EGInventoryAndMaterielDto>> Get([FromQuery] QueryByIdEGInventoryInput input)
    //{

    //    模糊查询（根据物料编号和物料名称和物料规格查询）
    //    List<string> materielNums = new List<string>();
    //    List<EGInventoryAndMaterielDto> EGInventoryAndMaterielData = new List<EGInventoryAndMaterielDto>();

    //    List<EGMateriel> materielsData = await _materiel.GetListAsync
    //        (
    //            u => u.MaterielNum.Contains(input.MaterielNum) ||
    //            u.MaterielName.Contains(input.MaterielName) ||
    //            u.MaterielSpecs.Contains(input.MaterielSpecs)
    //        );
    //    if (materielsData != null)
    //    {
    //        得到模糊查询里面的物料编号
    //        foreach (var item in materielsData)
    //        {
    //            materielNums.Add(item.MaterielNum);
    //        }

    //        for (int i = 0; i < materielNums.Count; i++)
    //        {
    //            string num = materielNums[i];

    //            var materieItem = await _materiel.GetFirstAsync(u => u.MaterielNum == num);
    //            var inventoryItem = await _rep.GetFirstAsync(u => u.MaterielNum == num);
    //            物料名称
    //           var materielnum = materieItem.MaterielNum;
    //            物料名称
    //           var materiename = materieItem.MaterielName;
    //            规格
    //           var materielspecs = materieItem.MaterielSpecs;
    //            在库数量
    //           var icountall = inventoryItem.ICountAll;
    //            可用数量
    //           var iusable = inventoryItem.IUsable;


    //            EGInventoryAndMaterielDto inventory = new EGInventoryAndMaterielDto()
    //            {
    //                MaterielNum = materielnum,
    //                MaterielName = materiename,
    //                MaterielSpecs = materielspecs,
    //                ICountAll = (int)icountall,
    //                IUsable = (int)iusable,

    //            };
    //            EGInventoryAndMaterielData.Add(inventory);
    //        }

    //    }
    //    return EGInventoryAndMaterielData;

    //}
    #endregion

}

