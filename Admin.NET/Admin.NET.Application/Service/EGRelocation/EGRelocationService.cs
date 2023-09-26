namespace Admin.NET.Application;

/// <summary>
/// 移库信息接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGRelocationService : IDynamicApiController, ITransient
{
    #region 引用实体
    private readonly SqlSugarRepository<EGRelocation> _rep;
    private readonly SqlSugarRepository<EGInventoryDetail> _model;
    #endregion

    #region 关系注入
    public EGRelocationService
        (
         SqlSugarRepository<EGRelocation> rep,
         SqlSugarRepository<EGInventoryDetail> model
        )
    {
        _rep = rep;
        _model = model;
    }
    #endregion

    #region 分页查询移库信息
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

    #endregion

    #region 删除移库信息
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
    #endregion

    #region 更新移库信息
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
    #endregion

    #region 获取移库信息
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
    #endregion

    #region 获取移库信息列表
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

    #endregion

    #region 添加一条（移库记录）
    /// <summary>
    /// 添加一条移库记录
    /// </summary>
    /// <param name="input">移库记录字段</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost]
    [ApiDescriptionSettings(Name = "InsertRepositoryRecords")]

    public async Task InsertRepositoryRecords(EGRelocation input)
    {
        // 查询是否有一致的记录
        var is_vaild = await _rep.GetFirstAsync(u => u.RelocatioNum == input.RelocatioNum);
        if (is_vaild != null)
        {
            throw new Exception("已存在此移库记录！");
        }
        else
        {
            await _rep.InsertAsync(input);
        }
    }

    #endregion

    #region 移库操作

    /// <summary>
    /// 移库操作（更新详情表）
    /// </summary>
    /// <param name="materielnum">物料编号</param>
    /// <param name="regionnum">区域编号</param>
    /// <param name="storagenum">库位编号</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPut]
    [ApiDescriptionSettings(Name = "LibraryTransferOperation")]

    public async Task LibraryTransferOperation(string materielnum, string regionnum, string storagenum)
    {
        // 在移库记录表中根据物料编号查询是否有相同信息的数据
        var is_vaild = await _rep.GetListAsync(u => u.MaterielNum == materielnum);
        if (is_vaild == null)
        {
            throw new Exception("未找到需要移库的数据！");
        }
        else
        {
            //修改
            _model.AsUpdateable()
           .AS("EGInventoryDetail")
           .SetColumns(it => new EGInventoryDetail { RegionNum = regionnum, StorageNum = storagenum, InventoryDetailRemake = "此物料已被移动到当前库位" })
           .Where(u => u.MaterielNum == materielnum)
           .ExecuteCommand();

            #region 不实现（可以根据里面的物料的数量进行移库操作）
            // 判断用户输入的移库数量在库存中是否足够
            // 先查询库存表中，相关物料的可用数量

            //var issable = _db.AsQueryable().Where(u => u.MaterielNum == materielnum).Select(it => it.IUsable).ToInt();
            //List<EGInventory> data = _db.AsQueryable().Where(u => u.MaterielNum == materielnum).ToList();
            //int issable = 0;
            //foreach (var item in data)
            //{
            //    issable = (int)item.IUsable;
            //}
            //var newcount = issable - userInputCount;

            //// 如果用户输入的数量大于库存表中的数量
            //if (newcount < 0)
            //{
            //    throw new Exception("移库的数量大于库存中的数量，无法移库！");
            //}
            //else
            //{
            //    // 修改
            //    _model.AsUpdateable()
            //   .AS("EGInventoryDetail")
            //   .SetColumns(it => new EGInventoryDetail { RegionNum = regionnum, StorageNum = storagenum, })
            //   .Where(u => u.MaterielNum == materielnum);

            // 如果移库的数量等于库存中的数量
            //if (userInputCount == issable)
            //{
            //    // 修改
            //    _model.AsUpdateable()
            //   .AS("EGInventoryDetail")
            //   .SetColumns(it => new EGInventoryDetail { RegionNum = regionnum, StorageNum = storagenum, })
            //   .Where(u => u.MaterielNum == materielnum);
            //}
            // 如果移库的数量小于库存中的数量
            //if (issable > userInputCount)
            //{
            //    List<EGInventory> data = await _db.GetListAsync(u => u.MaterielNum == materielnum);
            //    foreach (EGInventory item in data)
            //    {
            //        var newid = item.Id;
            //        var newicountall = item.ICountAll;
            //    }
            //    // 将库存表中的库存减少
            //}
            #endregion
        }
    }
}
#endregion

//-------------------------------------//-------------------------------------//

#region 增加移库信息
/// <summary>
/// 增加移库信息
/// </summary>
/// <param name="input"></param>
/// <returns></returns>
//[HttpPost]
//[ApiDescriptionSettings(Name = "Add")]
//public async Task Add(AddEGRelocationInput input)
//{
//    var entity = input.Adapt<EGRelocation>();
//    await _rep.InsertAsync(entity);
//}
#endregion



