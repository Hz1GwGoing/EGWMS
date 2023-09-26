namespace Admin.NET.Application;

/// <summary>
/// 入库信息接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGJoinBoundService : IDynamicApiController, ITransient
{
    #region 引用实体
    private readonly SqlSugarRepository<EGInventory> _db;
    private readonly SqlSugarRepository<EGJoinBound> _rep;
    private readonly SqlSugarRepository<EGInventoryDetail> _InventoryDetail;
    #endregion

    #region 关系注入
    public EGJoinBoundService
        (
          SqlSugarRepository<EGJoinBound> rep,
          SqlSugarRepository<EGInventory> db,
          SqlSugarRepository<EGInventoryDetail> inventoryDetail
        )
    {
        _rep = rep;
        _db = db;
        _InventoryDetail = inventoryDetail;
    }

    #endregion

    #region 增加一条入库信息

    /// <summary>
    /// 增加入库信息
    /// </summary>
    /// <param name="input">入库信息字段</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "JoinBoundAdd")]
    public async Task JoinBoundAdd(InboundInfo input)
    {
        // 获得入库编号相同的数据
        var isExist = await _rep.GetFirstAsync(u => u.JoinBoundNum == input.JoinBoundNum);
        if (isExist != null) throw Oops.Oh(ErrorCodeEnum.D1006);
        ////var data = input.detail;
        List<EGJoinBound> data = input.detail.Select(inventory => new EGJoinBound
        {
            // 入库编号
            JoinBoundNum = input.JoinBoundNum,
            // 入库类型
            //JoinBoundType = input.JoinBoundType,
            // 入库人
            //JoinBoundUser = input.JoinBoundUser,
            // 入库数量
            JoinBoundCount = input.JoinBoundCount,
            // 入库时间
            //JoinBoundTime = DateTime.Now,
            // 入库状态
            JoinBoundStatus = input.JoinBoundStatus,
            // 回库更新时间
            //JoinBoundOutTime = input.JoinBoundOutTime,
            // 物料编号
            MaterielNum = input.MaterielNum,
            // 仓库编号
            WHNum = input.WHNum,
            // 栈板编号
            PalletNum = input.PalletNum,
            // 料箱编号
            WorkBinNum = input.WorkBinNum,
            // 入库备注
            JoinBoundRemake = input.JoinBoundRemake,
            // 创建时间                    
            CreateTime = DateTime.Now,

        })
        .ToList();
        Console.WriteLine(data.ToJson());
        _rep.InsertOrUpdate(data);
    }
    #endregion

    #region 将入库数据保存到库存表中

    /// <summary>
    /// 将入库数据保存到库存表中
    /// </summary>
    /// <param name="joinnum">入库编号</param>
    /// <param name="storagenum">库位编号</param>
    /// <param name="regionnum">区域编号</param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "InsertJoinBoundToInventory")]
    public async Task InsertJoinBoundToInventory(string joinnum, string? storagenum, string? regionnum)
    {
        //从数据库中查询所有入库信息，条件为入库单号一致
        var data = await _rep.GetListAsync(u => u.JoinBoundNum == joinnum);
        if (data == null)
        {
            // 错误异常处理
            throw Oops.Oh(ErrorCodeEnum.D1006);
        }
        else
        {
            Console.WriteLine("输出：" + data.ToJson());

            // 使用LINQ查询选择需要的属性，而不是依赖于特定索引位置  
            var joinBoundNum = data.Select(u => u.JoinBoundNum).FirstOrDefault();
            var joinBoundCount = data.Select(u => u.JoinBoundCount).FirstOrDefault();
            var whNum = data.Select(u => u.WHNum).FirstOrDefault();
            var materielNum = data.Select(u => u.MaterielNum).FirstOrDefault();

            // 入库状态
            //var joinBoundStatus = data.Select(u => u.JoinBoundStatus).FirstOrDefault();

            // 创建一个新的EGInventory对象  

            var inventory = new EGInventory
            {
                JoinBoundNum = joinBoundNum,
                ICountAll = joinBoundCount,
                WHNum = whNum,
                MaterielNum = materielNum,
            };

            // 创建一个新的EGInventoryDetail对象

            var inventorydetail = new EGInventoryDetail
            {
                // 物料编号
                MaterielNum = materielNum,
                // 库位编号
                StorageNum = storagenum,
                // 区域编号
                RegionNum = regionnum,
            };

            // 添加库存
            await _db.InsertAsync(inventory);
            // 将编号保存到详情表中
            await _InventoryDetail.InsertAsync(inventorydetail);
            // 更新状态 将入库状态改变 0未入库 1已入库（库存数量已保存到库存表中）
            _rep.AsUpdateable().AS("EGJoinBound").SetColumns("JoinBoundStatus", 1).Where(u => u.JoinBoundNum == joinnum).ExecuteCommand();

        }

    }
    #endregion

    #region 分页查询入库信息
    /// <summary>
    /// 分页查询入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<EGJoinBoundOutput>> Page(EGJoinBoundInput input)
    {
        var query = _rep.AsQueryable()
                    .WhereIF(!string.IsNullOrWhiteSpace(input.JoinBoundNum), u => u.JoinBoundNum.Contains(input.JoinBoundNum.Trim()))
                    .WhereIF(input.JoinBoundType > 0, u => u.JoinBoundType == input.JoinBoundType)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.JoinBoundUser), u => u.JoinBoundUser.Contains(input.JoinBoundUser.Trim()))
                    .WhereIF(input.JoinBoundCount > 0, u => u.JoinBoundCount == input.JoinBoundCount)
                    // 入库状态
                    .WhereIF(input.JoinBoundStatus > 0, u => u.JoinBoundStatus == input.JoinBoundStatus)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WHNum), u => u.WHNum.Contains(input.WHNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.PalletNum), u => u.PalletNum.Contains(input.PalletNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.WorkBinNum), u => u.WorkBinNum.Contains(input.WorkBinNum.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.JoinBoundRemake), u => u.JoinBoundRemake.Contains(input.JoinBoundRemake.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), u => u.MaterielNum.Contains(input.MaterielNum.Trim()))
                    // 获取创建日期
                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
                    .Select<EGJoinBoundOutput>()
;
        if (input.JoinBoundTimeRange != null && input.JoinBoundTimeRange.Count > 0)
        {
            DateTime? start = input.JoinBoundTimeRange[0];
            query = query.WhereIF(start.HasValue, u => u.JoinBoundTime > start);
            if (input.JoinBoundTimeRange.Count > 1 && input.JoinBoundTimeRange[1].HasValue)
            {
                var end = input.JoinBoundTimeRange[1].Value.AddDays(1);
                query = query.Where(u => u.JoinBoundTime < end);
            }
        }
        if (input.JoinBoundOutTimeRange != null && input.JoinBoundOutTimeRange.Count > 0)
        {
            DateTime? start = input.JoinBoundOutTimeRange[0];
            query = query.WhereIF(start.HasValue, u => u.JoinBoundOutTime > start);
            if (input.JoinBoundOutTimeRange.Count > 1 && input.JoinBoundOutTimeRange[1].HasValue)
            {
                var end = input.JoinBoundOutTimeRange[1].Value.AddDays(1);
                query = query.Where(u => u.JoinBoundOutTime < end);
            }
        }
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    #endregion

    #region 删除入库信息
    /// <summary>
    /// 删除入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteEGJoinBoundInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
    }
    #endregion

    #region 更新入库信息
    /// <summary>
    /// 更新入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateEGJoinBoundInput input)
    {
        var entity = input.Adapt<EGJoinBound>();
        await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }
    #endregion

    #region 获取入库信息
    /// <summary>
    /// 获取入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<EGJoinBound> Get([FromQuery] QueryByIdEGJoinBoundInput input)
    {
        //return await _rep.GetFirstAsync(u => u.Id == input.Id);

        // 模糊查询（入库编号）
        return await _rep.GetFirstAsync(u => u.JoinBoundNum.Contains(input.JoinBoundNum));

    }
    #endregion

    #region 获取入库信息列表
    /// <summary>
    /// 获取入库信息列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGJoinBoundOutput>> List([FromQuery] EGJoinBoundInput input)
    {
        return await _rep.AsQueryable().Select<EGJoinBoundOutput>().ToListAsync();
    }
    #endregion


    //-------------------------------------//-------------------------------------//

    #region 得到入库表中的数据
    /// <summary>
    /// 得到入库表中的数据
    /// </summary>
    /// <returns></returns>
    //public List<EGJoinBound> GetInventoryData()
    //{
    //    var EGJoinBounds = _sql.Queryable<EGJoinBound>().Where().ToList();
    //    return EGJoinBounds;
    //}
    #endregion

    #region 增加入库信息

    /// <summary>
    /// 增加入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>

    //[HttpPost]
    //[ApiDescriptionSettings(Name = "Add")]
    //public async Task Add(AddEGJoinBoundInput input)
    //{
    //    // 获得入库时的库存总数
    //    //int inventoryCount = (int)input.ICountAll;

    //    var entity = input.Adapt<EGJoinBound>();
    //    await _rep.InsertAsync(entity);

    //}
    #endregion

    #region 归类
    //List<EGJoinBound> joinboundData = await _rep.GetFirstAsync(u => u.JoinBoundNum == joinnum);
    //var EGJoinBounds = _sql.Queryable<EGJoinBound>().Where(u => u.MaterielNum = materielnum).ToList();
    //List<EGJoinBound> joinboundData = await _rep.GetFirstAsync(u => u.JoinBoundNum == joinnum);
    //List<EGInventory> data = joinboundData.Select(u => new EGInventory
    //{
    //    WHNum = u.WHNum,
    //    PalletNum = u.PalletNum,
    //    WorkBinNum = u.WorkBinNum,
    //    JoinBoundNum = u.JoinBoundNum,
    //    JoinBoundRemake = u.JoinBoundRemake,
    //    CreateTime = DateTime.Now,

    //    //}).ToList();
    //    //Console.WriteLine(data.ToJson());
    //    //_rep.InsertOrUpdate(data);


    //    // string joinboundnum = joinboundData.JoinBoundNum;
    //    // int joinboundcount = (int)joinboundData.JoinBoundCount;
    //    // string MaterielNum = joinboundData.MaterielNum;

    //    // var inventoryItem = new EGInventory
    //    // {
    //    //     JoinBoundNum = joinboundnum,
    //    //     ICountAll = joinboundcount,
    //    //     MaterielNum = MaterielNum
    //    // };

    //    // if (joinboundData == null) throw Oops.Oh(ErrorCodeEnum.D1002);
    //    // //_sql.Insertable<EGInventory>(inventoryItem).ExecuteCommand();

    //}

    // 将入库数据保存到库存表中 
    #endregion

    #region 获取入库信息（不使用）
    /// <summary>
    /// 获取入库信息（不使用）
    /// </summary>
    //[HttpPost]
    //public async Task GetJoinBound(EGGetJoinBoundInput input)
    //{
    //    EGInventory eGInventory = new EGInventory();
    //    // 库存编号
    //    eGInventory.InventoryNum = input.InventoryNum;
    //    // 库存总数
    //    eGInventory.ICountAll = input.JoinBoundCount;

    //    // 插入指定字段

    //    //_db.AsInsertable(eGInventory).InsertColumns("InventoryNum").ExecuteReturnIdentity();
    //}
    #endregion




}

