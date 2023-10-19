namespace Admin.NET.Application;
/// <summary>
/// 出库信息接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGOutBoundService : IDynamicApiController, ITransient
{
    #region 引用实体

    private readonly SqlSugarRepository<EGOutBound> _rep;
    private readonly SqlSugarRepository<EG_WMS_Inventory> _db;

    #endregion

    #region 关系注入
    public EGOutBoundService
        (
         SqlSugarRepository<EGOutBound> rep,
         SqlSugarRepository<EG_WMS_Inventory> db
        )
    {
        _rep = rep;
        _db = db;
    }

    #endregion

    #region 分页查询出库信息
    /// <summary>
    /// 分页查询出库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<EGOutBoundOutput>> Page(EGOutBoundInput input)
    {
        var query = _rep.AsQueryable()
                    .WhereIF(!string.IsNullOrWhiteSpace(input.OutboundNum), u => u.OutboundNum.Contains(input.OutboundNum.Trim()))
                    .WhereIF(input.OutboundType > 0, u => u.OutboundType == input.OutboundType)
                    .WhereIF(input.OutboundCount > 0, u => u.OutboundCount == input.OutboundCount)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.OutboundUser), u => u.OutboundUser.Contains(input.OutboundUser.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WHNum), u => u.WHNum.Contains(input.WHNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.PalletNum), u => u.PalletNum.Contains(input.PalletNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinNum), u => u.WorkBinNum.Contains(input.WorkBinNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.OutboundRemake), u => u.OutboundRemake.Contains(input.OutboundRemake.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), u => u.MaterielNum.Contains(input.MaterielNum.Trim()))

                    // 获取创建日期
                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
                    .Select<EGOutBoundOutput>()
;
        if (input.OutboundTimeRange != null && input.OutboundTimeRange.Count > 0)
        {
            DateTime? start = input.OutboundTimeRange[0];
            query = query.WhereIF(start.HasValue, u => u.OutboundTime > start);
            if (input.OutboundTimeRange.Count > 1 && input.OutboundTimeRange[1].HasValue)
            {
                var end = input.OutboundTimeRange[1].Value.AddDays(1);
                query = query.Where(u => u.OutboundTime < end);
            }
        }
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }
    #endregion

    #region 更新出库信息
    /// <summary>
    /// 更新出库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGOutBoundInput input)
    {
        var entity = input.Adapt<EGOutBound>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }
    #endregion

    #region 获取出库信息
    /// <summary>
    /// 获取出库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<EGOutBound> Get([FromQuery] QueryByIdEGOutBoundInput input)
    {
        //return await _rep.GetFirstAsync(u => u.Id == input.Id);

        // 模糊查询
        return await _rep.GetFirstAsync(u => u.OutboundNum.Contains(input.OutboundNum));

    }
    #endregion

    #region 获取出库信息列表
    /// <summary>
    /// 获取出库信息列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGOutBoundOutput>> List([FromQuery] EGOutBoundInput input)
    {
        return await _rep.AsQueryable().Select<EGOutBoundOutput>().ToListAsync();
    }
    #endregion

    #region 添加一条出库信息
    /// <summary>
    /// 添加一条出库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost]
    [ApiDescriptionSettings(Name = "InsertOutBoundRecords")]
    public async Task InsertOutBoundRecords(EGOutBound input)
    {
        // 查询是否有编号相同的记录
        var is_vaild = await _rep.GetFirstAsync(u => u.OutboundNum == input.OutboundNum);
        if (is_vaild != null)
        {
            throw new Exception("已存在此出库记录！");
        }
        else
        {
            // 获得当前时间时间戳
            string timesStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            // 生成自动编号
            input.OutboundNum = "EGCK" + timesStamp;

            await _rep.InsertAsync(input);
        }
    }


    #endregion

    #region 出库操作
    /// <summary>
    /// 出库操作（改变出库状态）
    /// </summary>
    /// <param name="materielnum">物料编号</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPut]
    [ApiDescriptionSettings(Name = "InventoryAndOutBound")]
    public async Task InventoryAndOutBound(string materielnum)
    {
        // 先查询是否有这条记录
        var is_vaild = await _rep.GetFirstAsync(u => u.MaterielNum == materielnum);
        if (is_vaild != null)
        {
            throw new Exception("未找到需要出库的数据！");
        }
        else
        {
            // 将库存表中物料的出库状态改变成为已出库
            _db.AsUpdateable()
           .AS("EGInventory")
           .SetColumns(it => new EG_WMS_Inventory { OutboundStatus = 1 })
           .Where(u => u.MaterielNum == materielnum)
           .ExecuteCommand();
        }
    }


    #endregion


    //-------------------------------------//-------------------------------------//

    #region 增加出库信息
    /// <summary>
    /// 增加出库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    //[HttpPost]
    //[ApiDescriptionSettings(Name = "Add")]
    //public async Task Add(AddEGOutBoundInput input)
    //{
    //    var entity = input.Adapt<EGOutBound>();
    //    await _rep.InsertAsync(entity);
    //}

    /// <summary>
    /// 删除出库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteEGOutBoundInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
    }
    #endregion

    #region 获得库存信息（使用物料编号查询）

    /// <summary>
    /// 获得库存信息（使用物料编号查询）
    /// </summary>
    //[HttpGet]
    //[ApiDescriptionSettings(Name = "GetMaterielData")]

    //public async Task<List<EGInventory>> GetMaterielData(string materielNum)
    //{
    //    // 模糊查询  
    //    List<EGInventory> data = await _db.GetListAsync(u => u.MaterielNum.Contains(materielNum));
    //    if (data == null)
    //    {
    //        throw Oops.Oh(ErrorCodeEnum.D1002);
    //    }
    //    // 创建一个新的列表来保存 EGInventory 对象  
    //    var inventoryList = new List<EGInventory>();

    //    // 遍历查询结果，为每个结果创建一个新的 EGInventory 对象，并添加到列表中  
    //    foreach (var item in data)
    //    {
    //        var Inventory = new EGInventory
    //        {
    //            MaterielNum = item.MaterielNum,
    //            WHNum = item.WHNum,
    //            ICountAll = item.ICountAll,
    //            IFrostCount = item.IFrostCount,
    //            IUsable = item.IUsable,
    //            IWaitingCount = item.IWaitingCount,
    //        };
    //        inventoryList.Add(Inventory);
    //    }

    //    return inventoryList;
    //}


    #endregion

    #region 根据用户需要出库的数量，对库存进行增减
    /// <summary>
    /// 根据用户需要出库的数量，对库存进行增减
    /// </summary>
    /// <param name="userInputCount">出库数量</param>
    /// <param name="materielnum">物料编号</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    //[HttpPut]
    //[ApiDescriptionSettings(Name = "UpdateCount")]

    //public async Task UpdateCount(int userInputCount, string materielnum)
    //{
    //    // 查询有没有这个物料  
    //    var inventory = await _db.GetListAsync(u => u.MaterielNum == materielnum);

    //    if (inventory.Count > 0)
    //    {
    //        // 如果有这个物料，就更新可用库存  
    //        var newCount = inventory[0].IUsable - userInputCount; // 新数量 = 原数量 - 用户输入的数量
    //        if (newCount < 0) // 如果新数量小于0，就不能出库  
    //        {
    //            throw new Exception("出库数量超过可用库存");
    //        }
    //        else
    //        {
    //            // 更新可用库存  
    //            _db.AsUpdateable().AS("EGInventory").SetColumns("IUsable", newCount).Where(u => u.MaterielNum == materielnum).ExecuteCommand();
    //        }
    //    }
    //    else
    //    {
    //        throw new Exception("未找到该物料");
    //    }
    //}



    #endregion
}

