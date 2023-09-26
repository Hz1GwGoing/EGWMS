namespace Admin.NET.Application;

/// <summary>
/// 物料管理接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGMaterielService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<EGMateriel> _rep;
    public EGMaterielService(SqlSugarRepository<EGMateriel> rep)
    {
        _rep = rep;
    }

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
        var entity = input.Adapt<EGMateriel>();
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
        var entity = input.Adapt<EGMateriel>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }
    #endregion

    #region 获取物料信息
    /// <summary>
    /// 获取物料信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<EGMateriel> Get([FromQuery] QueryByIdEGMaterielInput input)
    {
        //return await _rep.GetFirstAsync(u => u.Id == input.Id);

        // 模糊查询
        return await _rep.GetFirstAsync(u => u.MaterielNum.Contains(input.MaterielNum) || u.MaterielName.Contains(input.MaterielName));
    }
    #endregion

    #region 获取物料信息列表

    /// <summary>
    /// 获取物料信息列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGMaterielOutput>> List([FromQuery] EGMaterielInput input)
    {
        //return await _rep.AsQueryable().Select<EGMaterielOutput>().ToListAsync();
        // 模糊查询
        return await _rep.AsQueryable()
               .Where(x => x.MaterielName.Contains(input.MaterielName))
               .Select<EGMaterielOutput>()
               .ToListAsync();
    }
    #endregion

    //-------------------------------------//-------------------------------------//


}

