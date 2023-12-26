using Admin.NET.Application.IService.EG_WMS_Pallet;

namespace Admin.NET.Application;
/// <summary>
/// 栈板管理接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGPalletService : IDynamicApiController, ITransient, IEGPalletServics
{

    private readonly SqlSugarRepository<EG_WMS_Pallet> _rep;
    public EGPalletService(SqlSugarRepository<EG_WMS_Pallet> rep)
    {
        _rep = rep;
    }

    #region 分页查询栈板
    /// <summary>
    /// 分页查询栈板
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<EGPalletOutput>> Page(EGPalletInput input)
    {
        var query = _rep.AsQueryable()
                    .WhereIF(!string.IsNullOrWhiteSpace(input.PalletNum), u => u.PalletNum.Contains(input.PalletNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.PalletName), u => u.PalletName.Contains(input.PalletName.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.PalletSpecs), u => u.PalletSpecs.Contains(input.PalletSpecs.Trim()))
                    .WhereIF(input.PalletStatus > 0, u => u.PalletStatus == input.PalletStatus)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.CreateUserName), u => u.CreateUserName.Contains(input.CreateUserName.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.PalletRemake), u => u.PalletRemake.Contains(input.PalletRemake.Trim()))
                    // 获取创建日期
                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.StorageNum), u => u.StorageNum.Contains(input.StorageNum.Trim()))
                    .Select<EGPalletOutput>()
;
        if (input.ExpirationDateRange != null && input.ExpirationDateRange.Count > 0)
        {
            DateTime? start = input.ExpirationDateRange[0];
            query = query.WhereIF(start.HasValue, u => u.ExpirationDate > start);
            if (input.ExpirationDateRange.Count > 1 && input.ExpirationDateRange[1].HasValue)
            {
                var end = input.ExpirationDateRange[1].Value.AddDays(1);
                query = query.Where(u => u.ExpirationDate < end);
            }
        }
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }
    #endregion

    #region 增加栈板
    /// <summary>
    /// 增加栈板
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task Add(AddEGPalletInput input)
    {
        var entity = input.Adapt<EG_WMS_Pallet>();
        await _rep.InsertAsync(entity);
    }
    #endregion

    #region 删除栈板
    /// <summary>
    /// 删除栈板
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteEGPalletInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
    }
    #endregion

    #region 更新栈板
    /// <summary>
    /// 更新栈板
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGPalletInput input)
    {
        var entity = input.Adapt<EG_WMS_Pallet>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }
    #endregion

    #region 获取栈板
    /// <summary>
    /// 获取栈板
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<EG_WMS_Pallet> Get([FromQuery] QueryByIdEGPalletInput input)
    {
        //return await _rep.GetFirstAsync(u => u.Id == input.Id);
        // 模糊查询
        return await _rep.GetFirstAsync(u => u.PalletNum.Contains(input.PalletNum) || u.PalletName.Contains(input.PalletName));

    }
    #endregion

    #region 获取栈板列表
    /// <summary>
    /// 获取栈板列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGPalletOutput>> List([FromQuery] EGPalletInput input)
    {
        return await _rep.AsQueryable().Select<EGPalletOutput>().ToListAsync();
    }
    #endregion

    //-------------------------------------//-------------------------------------//

}

