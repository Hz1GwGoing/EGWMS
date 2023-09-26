namespace Admin.NET.Application;

/// <summary>
/// 盘点信息接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGTakeStockService : IDynamicApiController, ITransient
{
    #region 引用实体
    private readonly SqlSugarRepository<EGTakeStock> _rep;
    private readonly SqlSugarRepository<EGInventory> _model;
    private readonly SqlSugarRepository<EGInventoryDetail> _InventoryDetail;
    private readonly SqlSugarRepository<EGMateriel> _db;
    #endregion

    #region 关系注入
    public EGTakeStockService
        (
          SqlSugarRepository<EGTakeStock> rep,
          SqlSugarRepository<EGInventory> model,
          SqlSugarRepository<EGMateriel> db,
          SqlSugarRepository<EGInventoryDetail> InventoryDetail
        )
    {
        _rep = rep;
        _model = model;
        _db = db;
        _InventoryDetail = InventoryDetail;
    }
    #endregion

    #region 删除盘点信息
    /// <summary>
    /// 删除盘点信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteEGTakeStockInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
    }
    #endregion

    #region 获取盘点信息（模糊查询）
    /// <summary>
    /// 获取盘点信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<EGTakeStock> Get([FromQuery] QueryByIdEGTakeStockInput input)
    {
        //return await _rep.GetFirstAsync(u => u.Id == input.Id);

        // 模糊查询
        return await _rep.GetFirstAsync(u => u.TakeStockNum.Contains(input.TakeStockNum));

    }

    #endregion

    #region 获取盘点信息（联表查询）<已添加错误异常处理器>

    /// <summary>
    /// 获取盘点信息（联表查询）
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "GetTakeStockMessage")]
    public async Task<List<TakeStockData>> GetTakeStockMessage()
    {
        List<EGTakeStock> data = null;
        List<string> materielNums = new List<string>();
        List<TakeStockData> takeStockDataList = new List<TakeStockData>();

        try
        {
            data = await _rep.GetListAsync();
            // 循环遍历得到物料编号  
            foreach (var item in data)
            {
                materielNums.Add(item.MaterielNum);
            }

            // 将每一条数据得到，并且添加到takeStockDataList集合中  
            for (int i = 0; i < materielNums.Count; i++)
            {
                string num = materielNums[i];
                var materielData = await _db.GetFirstAsync(u => u.MaterielNum == num);
                var inventoryData = await _model.GetFirstAsync(u => u.MaterielNum == num);
                var takeStockData = await _rep.GetFirstAsync(u => u.MaterielNum == num);
                var materielnum = materielData.MaterielNum;
                var materielname = materielData.MaterielName;
                var materielspecs = materielData.MaterielSpecs;
                var icountAll = inventoryData.ICountAll;
                // 盘点状态  
                var takestockstatus = takeStockData.TakeStockStatus;
                var takestockcount = takeStockData.TakeStockCount;
                var diffCount = takeStockData.TakeStockDiffCount;

                TakeStockData Data = new TakeStockData()
                {
                    MaterielNum = materielnum,
                    MaterielName = materielname,
                    MaterielSpecs = materielspecs,
                    TakeStockStatus = (int)takestockstatus,
                    ICountAll = (int)icountAll,
                    TakeStockCount = (int)takestockcount,
                    DiffCount = (int)diffCount
                };
                takeStockDataList.Add(Data);
            }
        }
        catch (Exception ex)
        {
            // 这里处理你的异常，例如记录日志，抛出异常等。 
            throw new Exception("错误，请检查数据库中是否有重复数据！");
        }
        return takeStockDataList;
    }

    #endregion

    // 物料盘 所有库位上包含有指定物料的托盘分别生成一个
    // 库位盘 按照勾选的库位 生成盘点任务

    #region 添加一条盘点信息（可以输入数量，生成盘点记录；也可以不输入数量，生成未盘点记录）

    /// <summary>
    /// 添加一条盘点信息（可以输入数量，生成盘点记录；也可以不输入数量，生成未盘点记录）
    /// </summary>
    /// <param name="takestocknum">盘点编号</param>
    /// <param name="materielnum">物料编号</param>
    /// <param name="takestockcount">盘点数量</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>

    [HttpPost]
    [ApiDescriptionSettings(Name = "AddTakeStockMessage")]
    public async Task AddTakeStockMessage(string takestocknum, string materielnum, int? takestockcount)
    {
        List<EGInventory> is_valid = await _model.GetListAsync(u => u.MaterielNum == materielnum);
        if (is_valid == null)
        {
            throw new Exception("此条物料编号不存在！");
        }
        else
        {
            // 物料编号
            string materielNum = is_valid[0].MaterielNum;
            // 物料存量
            int icountAll = (int)is_valid[0].ICountAll;
            // 没有输入盘点数量
            if (takestockcount == null)
            {
                EGTakeStock eGTakeStock = new EGTakeStock()
                {
                    TakeStockNum = takestocknum,
                    MaterielNum = materielNum,
                    // 盘点状态
                    TakeStockStatus = 0,
                    TakeStockDiffCount = null,
                    TakeStockCount = null,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                };
                await _rep.InsertAsync(eGTakeStock);
            }
            else
            {
                // 差值数量
                int diffCount = (int)(icountAll - takestockcount);
                // 盘亏
                if (diffCount > 0)
                {
                    EGTakeStock eGTakeStock = new EGTakeStock()
                    {
                        TakeStockNum = takestocknum,
                        MaterielNum = materielNum,
                        // 盘点状态
                        TakeStockStatus = 2,
                        TakeStockDiffCount = diffCount,
                        TakeStockCount = takestockcount,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now,
                    };
                    await _rep.InsertAsync(eGTakeStock);
                }
                // 盘赢
                else if (diffCount <= 0)
                {
                    // 转换为绝对值
                    int absDiffcount = Math.Abs(diffCount);

                    EGTakeStock eGTakeStock = new EGTakeStock()
                    {
                        TakeStockNum = takestocknum,
                        MaterielNum = materielNum,
                        // 盘点状态
                        TakeStockStatus = 1,
                        TakeStockDiffCount = absDiffcount,
                        TakeStockCount = takestockcount,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now,
                    };
                    await _rep.InsertAsync(eGTakeStock);
                }
            }
        }
    }

    #endregion

    #region 输入盘点数量改变未盘点的状态

    /// <summary>
    /// 输入盘点数量改变未盘点的状态
    /// </summary>
    /// <param name="takestocknum">盘点编号</param>
    /// <param name="takestockcount">盘点数量</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPut]
    [ApiDescriptionSettings(Name = "UpdateTakeStockMessageStatus")]
    public async Task UpdateTakeStockMessageStatus(string takestocknum, int takestockcount)
    {
        // 根据用户输入的盘点编号查找相应记录
        List<EGTakeStock> is_vaild = await _rep.GetListAsync(u => u.TakeStockNum == takestocknum);
        if (is_vaild == null)
        {
            throw new Exception("为找到此条盘点记录");
        }
        else
        {
            string materielNum = is_vaild[0].MaterielNum;
            List<EGInventory> data = await _model.GetListAsync(u => u.MaterielNum == materielNum);
            int icountAll = (int)data[0].ICountAll;
            // 差值数量
            int diffCount = icountAll - takestockcount;
            // 盘亏
            if (diffCount > 0)
            {
                // 将盘点表中的盘点状态改编成盘亏
                _rep.AsUpdateable()
               .AS("EGTakeStock")
               .SetColumns(it => new EGTakeStock
               { TakeStockStatus = 2, TakeStockCount = takestockcount, TakeStockDiffCount = diffCount })
               .Where(u => u.TakeStockNum == takestocknum)
               .ExecuteCommand();

            }
            // 盘赢
            else if (diffCount <= 0)
            {
                // 转换为绝对值
                int absDiffcount = Math.Abs(diffCount);

                // 将盘点表中的盘点状态改编成盘赢
                _rep.AsUpdateable()
               .AS("EGTakeStock")
               .SetColumns(it => new EGTakeStock
               { TakeStockStatus = 1, TakeStockCount = takestockcount, TakeStockDiffCount = absDiffcount })
               .Where(u => u.TakeStockNum == takestocknum)
               .ExecuteCommand();
            }

        }

    }



    #endregion

    #region 根据库位生成盘点任务（未完成）

    //public async Task A(string storagenum)
    //{
    //    // 查询库存详情表中库位编号相等的
    //    List<EGInventoryDetail> data = await _InventoryDetail.GetListAsync(u => u.StorageNum == storagenum);

    //    for (int i = 0; i < data.Count; i++)
    //    {

    //    }

    //}



    #endregion

    //-------------------------------------//-------------------------------------//

    #region 增加盘点信息
    /// <summary>
    /// 增加盘点信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    //[HttpPost]
    //[ApiDescriptionSettings(Name = "Add")]
    //public async Task Add(AddEGTakeStockInput input)
    //{
    //    var entity = input.Adapt<EGTakeStock>();
    //    await _rep.InsertAsync(entity);
    //}
    #endregion

    #region 获取盘点信息列表
    /// <summary>
    /// 获取盘点信息列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    //[HttpGet]
    //[ApiDescriptionSettings(Name = "List")]
    //public async Task<List<EGTakeStockOutput>> List([FromQuery] EGTakeStockInput input)
    //{
    //    return await _rep.AsQueryable().Select<EGTakeStockOutput>().ToListAsync();
    //}

    #endregion

    #region 分页查询盘点信息
    /// <summary>
    /// 分页查询盘点信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    //    [HttpPost]
    //    [ApiDescriptionSettings(Name = "Page")]
    //    public async Task<SqlSugarPagedList<EGTakeStockOutput>> Page(EGTakeStockInput input)
    //    {
    //        var query = _rep.AsQueryable()
    //                    .WhereIF(!string.IsNullOrWhiteSpace(input.TakeStockNum), u => u.TakeStockNum.Contains(input.TakeStockNum.Trim()))
    //                    .WhereIF(input.TakeStockStatus > 0, u => u.TakeStockStatus == input.TakeStockStatus)
    //                    // 盘点数量
    //                    .WhereIF(input.TakeStockCount > 0, u => u.TakeStockCount == input.TakeStockCount)
    //                    // 差值数量
    //                    .WhereIF(input.TakeStockDiffCount > 0, u => u.TakeStockDiffCount == input.TakeStockDiffCount)
    //                    .WhereIF(!string.IsNullOrWhiteSpace(input.TakeStockUser), u => u.TakeStockUser.Contains(input.TakeStockUser.Trim()))
    //                    .WhereIF(!string.IsNullOrWhiteSpace(input.TakeStockRemake), u => u.TakeStockRemake.Contains(input.TakeStockRemake.Trim()))
    //                    .WhereIF(!string.IsNullOrWhiteSpace(input.MaterielNum), u => u.MaterielNum.Contains(input.MaterielNum.Trim()))
    //                    // 获取创建日期
    //                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
    //                    .Select<EGTakeStockOutput>()
    //;
    //        if (input.TakeStockTimeRange != null && input.TakeStockTimeRange.Count > 0)
    //        {
    //            DateTime? start = input.TakeStockTimeRange[0];
    //            query = query.WhereIF(start.HasValue, u => u.TakeStockTime > start);
    //            if (input.TakeStockTimeRange.Count > 1 && input.TakeStockTimeRange[1].HasValue)
    //            {
    //                var end = input.TakeStockTimeRange[1].Value.AddDays(1);
    //                query = query.Where(u => u.TakeStockTime < end);
    //            }
    //        }
    //        query = query.OrderBuilder(input);
    //        return await query.ToPagedListAsync(input.Page, input.PageSize);
    //    }
    #endregion

    #region 更新盘点信息
    /// <summary>
    /// 更新盘点信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    //[HttpPost]
    //[ApiDescriptionSettings(Name = "Update")]
    //public async Task Update(UpdateEGTakeStockInput input)
    //{
    //    var entity = input.Adapt<EGTakeStock>();
    //    await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    //}
    #endregion

    #region 获取盘点信息（联表查询）<未添加错误异常处理器>
    //public async Task<List<TakeStockData>> GetTakeStockMessage()
    //{
    //    List<EGTakeStock> data = await _rep.GetListAsync();
    //    List<string> materielNums = new List<string>();
    //    // 循环遍历得到物料编号
    //    foreach (var item in data)
    //    {
    //        materielNums.Add(item.MaterielNum);
    //    }
    //    List<TakeStockData> takeStockDataList = new List<TakeStockData>();
    //    // 将每一条数据得到，并且添加到takeStockDataList集合中
    //    for (int i = 0; i < materielNums.Count; i++)
    //    {
    //        string num = materielNums[i];
    //        var materielData = await _db.GetFirstAsync(u => u.MaterielNum == num);
    //        var inventoryData = await _model.GetFirstAsync(u => u.MaterielNum == num);
    //        var takeStockData = await _rep.GetFirstAsync(u => u.MaterielNum == num);
    //        var materielnum = materielData.MaterielNum;
    //        var materielname = materielData.MaterielName;
    //        var materielspecs = materielData.MaterielSpecs;
    //        var icountAll = inventoryData.ICountAll;
    //        // 盘点状态
    //        var takestockstatus = takeStockData.TakeStockStatus;
    //        var takestockcount = takeStockData.TakeStockCount;
    //        var diffCount = takeStockData.TakeStockDiffCount;

    //        TakeStockData Data = new TakeStockData()
    //        {
    //            MaterielNum = materielnum,
    //            MaterielName = materielname,
    //            MaterielSpecs = materielspecs,
    //            TakeStockStatus = (int)takestockstatus,
    //            ICountAll = (int)icountAll,
    //            TakeStockCount = (int)takestockcount,
    //            DiffCount = (int)diffCount
    //        };
    //        takeStockDataList.Add(Data);
    //    }
    //    return takeStockDataList;
    //}
    #endregion
}

