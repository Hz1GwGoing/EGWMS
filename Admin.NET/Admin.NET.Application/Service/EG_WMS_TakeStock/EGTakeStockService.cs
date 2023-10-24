namespace Admin.NET.Application;

/// <summary>
/// 盘点信息接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGTakeStockService : IDynamicApiController, ITransient
{
    #region 引用实体
    private readonly SqlSugarRepository<EG_WMS_TakeStock> _rep;
    private readonly SqlSugarRepository<EG_WMS_Inventory> _model;
    private readonly SqlSugarRepository<EG_WMS_InventoryDetail> _InventoryDetail;
    private readonly SqlSugarRepository<EG_WMS_Materiel> _db;
    #endregion

    #region 关系注入
    public EGTakeStockService
        (
          SqlSugarRepository<EG_WMS_TakeStock> rep,
          SqlSugarRepository<EG_WMS_Inventory> model,
          SqlSugarRepository<EG_WMS_Materiel> db,
          SqlSugarRepository<EG_WMS_InventoryDetail> InventoryDetail
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
    public async Task<EG_WMS_TakeStock> Get([FromQuery] QueryByIdEGTakeStockInput input)
    {
        //return await _rep.GetFirstAsync(u => u.Id == input.Id);

        // 模糊查询
        return await _rep.GetFirstAsync(u => u.TakeStockNum.Contains(input.TakeStockNum));

    }

    #endregion

    #region 获取盘点信息列表
    /// <summary>
    /// 获取盘点信息列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EGTakeStockOutput>> List([FromQuery] EGTakeStockInput input)
    {
        return await _rep.AsQueryable().Select<EGTakeStockOutput>().ToListAsync();
    }

    #endregion

    #region 查找料箱（记录下来）


    #endregion

    #region 根据库位盘点（修改，待完善）

    /// <summary>
    /// 得到这个库位上面的所有料箱的数据
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public List<ViewTaskStock> GetViewTaskStocks(string storagenum)
    {
        // 扫库位 得到这个库位有多少个料箱，以及物料的数量
        // 展示出来
        // 人工生成盘点任务 并且料箱的信息和库位里面料箱的信息进行对比

        return _model.AsQueryable()
                .InnerJoin<EG_WMS_InventoryDetail>((inv, invd) => inv.InventoryNum == invd.InventoryNum)
                .Where((inv, invd) => invd.StorageNum == storagenum && inv.OutboundStatus == 0)
                .Select((inv, invd) => new ViewTaskStock
                {
                    // 料箱编号
                    WorkBinNum = invd.WorkBinNum,
                    // 物料编号
                    MaterielNum = inv.MaterielNum,
                    // 总数
                    ICountAll = (int)inv.ICountAll,
                    // 库位编号
                    StorageNum = invd.StorageNum,
                    // 生产批次
                    ProductionLot = invd.ProductionLot,
                    // 区域编号
                    RegionNum = invd.RegionNum,
                    // 仓库编号
                    WHNum = invd.WHNum,
                })
                .ToList();

    }

    /// <summary>
    /// 得到每个料箱里面的数据
    /// </summary>
    /// <param name="workbinnum"></param>
    /// <returns></returns>
    public List<ViewTaskStock> GetWorkBinData(string workbinnum)
    {
        // 得到这个料箱里面的数据
        return _InventoryDetail.AsQueryable()
                          .InnerJoin<EG_WMS_Inventory>((invd, inv) => inv.InventoryNum == invd.InventoryNum)
                          .Where((invd, inv) => invd.WorkBinNum == workbinnum && inv.OutboundStatus == 0)
                          .Select((invd, inv) => new ViewTaskStock
                          {
                              // 料箱编号
                              WorkBinNum = invd.WorkBinNum,
                              // 物料编号
                              MaterielNum = inv.MaterielNum,
                              // 总数
                              ICountAll = (int)inv.ICountAll,
                              // 库位编号
                              StorageNum = invd.StorageNum,
                              // 生产批次
                              ProductionLot = invd.ProductionLot,
                              // 区域编号
                              RegionNum = invd.RegionNum,
                              // 仓库编号
                              WHNum = invd.WHNum,
                          })
                          .ToList();
    }


    /// <summary>
    /// 根据人工扫描PDA出来的料箱数据去修改库存中的数据
    /// </summary>
    /// <returns></returns>
    public async Task TaskStockUpdateInventory(string? workbinnum)
    {

        // 自动生成盘点编号（时间戳）
        string timesStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        string takestocknum = "EGPD" + timesStamp;

        // 生成盘点任务
        EG_WMS_TakeStock data = new EG_WMS_TakeStock()
        {
            TakeStockNum = takestocknum,
            CreateTime = DateTime.Now,
            TakeStockType = 1,
            TakeStockTime = DateTime.Now,
            // 盘点状态
            TakeStockStatus = 0,
        };

        await _rep.InsertAsync(data);

        // 得到这个料箱里面的数据
        var pdaData = _InventoryDetail.AsQueryable()
                         .InnerJoin<EG_WMS_Inventory>((invd, inv) => inv.InventoryNum == invd.InventoryNum)
                         .Where((invd, inv) => invd.WorkBinNum == workbinnum && inv.OutboundStatus == 0)
                         .Select((invd, inv) => new { invd, inv })
                         .ToList();

        string remake = pdaData[0].invd.InventoryDetailRemake;

        try
        {
            if (workbinnum != null)
            {

                // 修改盘点任务
                _rep.AsUpdateable()
                    .AS("EG_WMS_TakeStock")
                    .SetColumns(it => new EG_WMS_TakeStock
                    {
                        TakeStockStatus = 1,
                        //TakeStockStorageNum = ,
                    })
                    .Where(x => x.TakeStockNum == takestocknum);


                // 修改这条数据

                _model.AsUpdateable()
                      .AS("EG_WMS_Inventory")
                      .SetColumns(it => new EG_WMS_Inventory
                      {
                          ICountAll = pdaData[0].inv.ICountAll,
                          MaterielNum = pdaData[0].inv.MaterielNum,
                          UpdateTime = DateTime.Now,
                      })
                      .Where(x => x.InventoryNum == pdaData[0].inv.InventoryNum);

                _InventoryDetail.AsUpdateable()
                                .AS("EG_WMS_InventoryDetail")
                                .SetColumns(it => new EG_WMS_InventoryDetail
                                {
                                    ProductionLot = pdaData[0].invd.ProductionLot,
                                    UpdateTime = DateTime.Now,
                                    InventoryDetailRemake = $"{remake},（此库存已盘点修改）"
                                })
                                .Where(x => x.InventoryNum == pdaData[0].invd.InventoryNum);
            }
            else
            {
                // 修改盘点任务
                _rep.AsUpdateable()
                    .AS("EG_WMS_TakeStock")
                    .SetColumns(it => new EG_WMS_TakeStock
                    {
                        TakeStockStatus = 1,
                        //TakeStockStorageNum = ,
                    })
                    .Where(x => x.TakeStockNum == takestocknum);
            }

        }

        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }


    }





    #endregion

    // 物料盘 所有库位上包含有指定物料的托盘分别生成一个
    // 库位盘 按照勾选的库位 生成盘点任务

    // 已修改，根据库位（盘点料箱）

    #region （根据库位）来盘点料箱（输入料箱数量，多了盘盈，少了盘亏）

    /// <summary>
    /// （根据库位）来盘点料箱（输入料箱数量，多了盘盈，少了盘亏）
    /// </summary>
    /// <param name="storagenum">库位编号</param>
    /// <param name="takestockcount">盘点数量</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost]
    [ApiDescriptionSettings(Name = "ReasonStorageTakeStockMessage")]
    public async Task ReasonStorageTakeStockMessage(string storagenum, int? takestockcount)
    {
        // 自动生成盘点编号（时间戳）
        string timesStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        string takestocknum = "EGPD" + timesStamp;

        // 查询库存详情表中库位编号相等的
        List<EG_WMS_InventoryDetail> inventoryDetailData = await _InventoryDetail.GetListAsync(u => u.StorageNum == storagenum);
        if (inventoryDetailData.Count == 0)
        {
            throw new Exception("没有存在这个库位!");
        }
        else
        {
            // 得到库位编号相同上所有的料箱数量（简易）
            int sumCount = inventoryDetailData.Count;

            // 没有输入盘点数量
            if (takestockcount == null)
            {
                EG_WMS_TakeStock eGTakeStock = new EG_WMS_TakeStock()
                {
                    TakeStockNum = takestocknum,
                    //MaterielNum = materielnum,
                    // 料箱数量
                    SumTakeStockCount = sumCount,
                    // 盘点状态
                    TakeStockStatus = 0,
                    TakeStockDiffCount = null,
                    TakeStockCount = null,
                    // 盘点类别
                    TakeStockType = 1,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                };
                await _rep.InsertAsync(eGTakeStock);
            }
            else
            {
                // 差值数量
                int diffCount = (int)(sumCount - takestockcount);

                // 盘亏
                if (diffCount > 0)
                {
                    EG_WMS_TakeStock eGTakeStock = new EG_WMS_TakeStock()
                    {
                        TakeStockNum = takestocknum,
                        // 料箱数量
                        SumTakeStockCount = sumCount,
                        // 盘点状态
                        TakeStockStatus = 2,
                        TakeStockDiffCount = diffCount,
                        TakeStockCount = takestockcount,
                        // 盘点类别
                        TakeStockType = 1,
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

                    EG_WMS_TakeStock eGTakeStock = new EG_WMS_TakeStock()
                    {
                        TakeStockNum = takestocknum,
                        // 料箱数量
                        SumTakeStockCount = sumCount,
                        // 盘点状态
                        TakeStockStatus = 1,
                        TakeStockDiffCount = absDiffcount,
                        TakeStockCount = takestockcount,
                        // 盘点类别
                        TakeStockType = 1,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now,
                    };
                    await _rep.InsertAsync(eGTakeStock);
                }

            }
        }
    }
    #endregion

    #region （根据库位）输入盘点数量改变未盘点的状态

    /// <summary>
    ///（根据库位）输入盘点数量改变未盘点的状态
    /// </summary>
    /// <param name="takestocknum">盘点编号</param>
    /// <param name="takestockcount">盘点数量</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPut]
    [ApiDescriptionSettings(Name = "UpdateTakeStockStorageNumMessageStatus")]
    public async Task UpdateTakeStockStorageNumMessageStatus(string takestocknum, int takestockcount)
    {
        // 根据用户输入的盘点编号查找相应记录
        List<EG_WMS_TakeStock> is_vaild = await _rep.GetListAsync(u => u.TakeStockNum == takestocknum);
        if (is_vaild.Count == 0)
        {
            throw new Exception("为找到此条盘点记录");
        }
        else
        {
            // 获得库位上的料箱数量（简易）
            int sumCount = (int)is_vaild[0].SumTakeStockCount;

            // 差值数量
            int diffCount = sumCount - takestockcount;
            // 盘亏
            if (diffCount > 0)
            {
                // 将盘点表中的盘点状态改编成盘亏
                _rep.AsUpdateable()
               .AS("EGTakeStock")
               .SetColumns(it => new EG_WMS_TakeStock
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
               .SetColumns(it => new EG_WMS_TakeStock
               { TakeStockStatus = 1, TakeStockCount = takestockcount, TakeStockDiffCount = absDiffcount })
               .Where(u => u.TakeStockNum == takestocknum)
               .ExecuteCommand();
            }

        }

    }

    #endregion

    // 已修改，根据物料（盘点料箱）

    #region （根据物料）生成盘点信息（输入物料编号，得到该物料的所有箱数）

    /// <summary>
    /// （根据物料）生成盘点信息（输入物料编号，得到该物料的所有箱数）
    /// </summary>
    /// <param name="materielnum">物料编号</param>
    /// <param name="takestockcount">盘点数量</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>

    [HttpPost]
    [ApiDescriptionSettings(Name = "AddTakeStockMessage")]
    public async Task AddTakeStockMessage(string materielnum, int? takestockcount)
    {

        // 自动生成时间戳
        string timesStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();

        string takestocknum = "EGPD" + timesStamp;

        List<EG_WMS_Inventory> is_valid = await _model.GetListAsync(u => u.MaterielNum == materielnum);
        if (is_valid.Count == 0)
        {
            throw new Exception("此条物料编号不存在！");
        }
        else
        {
            // 得到所有物料编号相同的料箱的数量（简易，得到的集合中，有几条物料信息就有几个料箱）
            int sumCount = is_valid.Count;

            // 没有输入盘点数量
            if (takestockcount == null)
            {
                EG_WMS_TakeStock eGTakeStock = new EG_WMS_TakeStock()
                {
                    TakeStockNum = takestocknum,
                    // 料箱数量
                    SumTakeStockCount = sumCount,
                    //MaterielNum = materielNum,
                    // 盘点状态
                    TakeStockStatus = 0,
                    TakeStockDiffCount = null,
                    TakeStockCount = null,
                    // 盘点类别
                    TakeStockType = 0,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                };
                await _rep.InsertAsync(eGTakeStock);
            }
            else
            {
                // 差值数量
                int diffCount = (int)(sumCount - takestockcount);
                // 盘亏
                if (diffCount > 0)
                {
                    EG_WMS_TakeStock eGTakeStock = new EG_WMS_TakeStock()
                    {
                        TakeStockNum = takestocknum,
                        //MaterielNum = materielNum,
                        // 料箱数量
                        SumTakeStockCount = sumCount,
                        // 盘点状态
                        TakeStockStatus = 2,
                        TakeStockDiffCount = diffCount,
                        TakeStockCount = takestockcount,
                        // 盘点类别
                        TakeStockType = 0,
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

                    EG_WMS_TakeStock eGTakeStock = new EG_WMS_TakeStock()
                    {
                        TakeStockNum = takestocknum,
                        //MaterielNum = materielNum,
                        // 料箱数量
                        SumTakeStockCount = sumCount,
                        // 盘点状态
                        TakeStockStatus = 1,
                        TakeStockDiffCount = absDiffcount,
                        TakeStockCount = takestockcount,
                        // 盘点类别
                        TakeStockType = 0,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now,
                    };
                    await _rep.InsertAsync(eGTakeStock);
                }
            }
        }
    }

    #endregion

    #region （根据物料）输入盘点数量改变未盘点的状态

    /// <summary>
    /// （根据物料）输入盘点数量改变未盘点的状态
    /// </summary>
    /// <param name="takestocknum">盘点编号</param>
    /// <param name="takestockcount">盘点数量</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPut]
    [ApiDescriptionSettings(Name = "UpdateTakeStockMaterielNumMessageStatus")]
    public async Task UpdateTakeStockMaterielNumMessageStatus(string takestocknum, int takestockcount)
    {
        // 根据用户输入的盘点编号查找相应记录
        List<EG_WMS_TakeStock> is_vaild = await _rep.GetListAsync(u => u.TakeStockNum == takestocknum);
        if (is_vaild.Count == 0)
        {
            throw new Exception("为找到此条盘点记录");
        }
        else
        {
            // 得到盘点表的料箱数量
            int sumCount = (int)is_vaild[0].SumTakeStockCount;
            // 差值数量
            int diffCount = sumCount - takestockcount;
            // 盘亏
            if (diffCount > 0)
            {
                // 将盘点表中的盘点状态改编成盘亏
                _rep.AsUpdateable()
               .AS("EGTakeStock")
               .SetColumns(it => new EG_WMS_TakeStock
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
               .SetColumns(it => new EG_WMS_TakeStock
               { TakeStockStatus = 1, TakeStockCount = takestockcount, TakeStockDiffCount = absDiffcount })
               .Where(u => u.TakeStockNum == takestocknum)
               .ExecuteCommand();
            }

        }

    }
    #endregion

    /// <summary>
    /// 料箱数据实体
    /// </summary>
    public class ViewTaskStock
    {

        /// <summary>
        /// 料箱编号
        /// </summary>
        public string WorkBinNum { get; set; }

        /// <summary>
        /// 物料总数
        /// </summary>
        public int ICountAll { get; set; }

        /// <summary>
        /// 物料编号
        /// </summary>
        public string MaterielNum { get; set; }

        /// <summary>
        /// 生产批次
        /// </summary>
        public string ProductionLot { get; set; }

        /// <summary>
        /// 库位编号
        /// </summary>
        public string StorageNum { get; set; }

        /// <summary>
        /// 仓库编号
        /// </summary>
        public string WHNum { get; set; }

        /// <summary>
        /// 区域编号
        /// </summary>
        public string RegionNum { get; set; }
    }

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

    #region 获取盘点信息（联表查询）<已添加错误异常处理器>

    /// <summary>
    /// 获取盘点信息（联表查询）
    /// </summary>
    /// <returns></returns>
    //[HttpGet]
    //[ApiDescriptionSettings(Name = "GetTakeStockMessage")]
    //public async Task<List<EGTakeStockData>> GetTakeStockMessage()
    //{
    //    List<EGTakeStock> data = null;
    //    List<string> materielNums = new List<string>();
    //    List<EGTakeStockData> takeStockDataList = new List<EGTakeStockData>();

    //    try
    //    {
    //        data = await _rep.GetListAsync();
    //        // 循环遍历得到物料编号  
    //        foreach (var item in data)
    //        {
    //            materielNums.Add(item.MaterielNum);
    //        }

    //        // 将每一条数据得到，并且添加到takeStockDataList集合中  
    //        for (int i = 0; i < materielNums.Count; i++)
    //        {
    //            string num = materielNums[i];
    //            var materielData = await _db.GetFirstAsync(u => u.MaterielNum == num);
    //            var inventoryData = await _model.GetFirstAsync(u => u.MaterielNum == num);
    //            var takeStockData = await _rep.GetFirstAsync(u => u.MaterielNum == num);
    //            var materielnum = materielData.MaterielNum;
    //            var materielname = materielData.MaterielName;
    //            var materielspecs = materielData.MaterielSpecs;
    //            var icountAll = inventoryData.ICountAll;
    //            // 盘点状态  
    //            var takestockstatus = takeStockData.TakeStockStatus;
    //            // 盘点类别
    //            var takestocktype = takeStockData.TakeStockType;
    //            var takestockcount = takeStockData.TakeStockCount;
    //            var diffCount = takeStockData.TakeStockDiffCount;

    //            EGTakeStockData Data = new EGTakeStockData()
    //            {
    //                MaterielNum = materielnum,
    //                MaterielName = materielname,
    //                MaterielSpecs = materielspecs,
    //                TakeStockStatus = (int)takestockstatus,
    //                ICountAll = (int)icountAll,
    //                // 盘点类别
    //                TakeStockType = takestocktype,
    //                TakeStockCount = (int?)takestockcount,
    //                DiffCount = (int?)diffCount
    //            };
    //            takeStockDataList.Add(Data);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        // 这里处理你的异常，例如记录日志，抛出异常等。 
    //        Console.WriteLine(ex.ToString());
    //    }
    //    return takeStockDataList;
    //}

    #endregion

    #region （根据库位）生成盘点信息 （一个库位上暂时不会有多个物料）

    /// <summary>
    /// （根据库位）生成盘点信息 （一个库位上暂时不会有多个物料）
    /// </summary>
    /// <param name="storagenum">库位编号</param>
    /// <param name="takestockcount">盘点数量</param>
    /// <param name="takestocknum">盘点编号</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>

    //[HttpPost]
    //[ApiDescriptionSettings(Name = "ReasonStorageTakeStockMessage")]
    //public async Task ReasonStorageTakeStockMessage(string takestocknum, string storagenum, int? takestockcount)
    //{
    //    // 查询库存详情表中库位编号相等的
    //    List<EGInventoryDetail> inventoryDetailData = await _InventoryDetail.GetListAsync(u => u.StorageNum == storagenum);
    //    if (inventoryDetailData.Count == 0)
    //    {
    //        throw new Exception("此库位不存在!");
    //    }
    //    else
    //    {
    //        // 得到库位编号相等的物料编号（一个库位上暂时不会有多个物料）
    //        string materielnum = inventoryDetailData[0].MaterielNum;
    //        int sumCount = 0;
    //        // 获得物料编号相同的物料信息
    //        List<EGInventory> InventoryData = await _model.GetListAsync(u => u.MaterielNum == materielnum);
    //        // 得到库位上所有物料的数量
    //        for (int i = 0; i < InventoryData.Count; i++)
    //        {
    //            int icountAll = (int)InventoryData[i].ICountAll;
    //            sumCount += icountAll;
    //        }

    //        // 没有输入盘点数量
    //        if (takestockcount == null)
    //        {
    //            EGTakeStock eGTakeStock = new EGTakeStock()
    //            {
    //                TakeStockNum = takestocknum,
    //                MaterielNum = materielnum,
    //                // 盘点状态
    //                TakeStockStatus = 0,
    //                TakeStockDiffCount = null,
    //                TakeStockCount = null,
    //                // 盘点类别
    //                TakeStockType = 1,
    //                CreateTime = DateTime.Now,
    //                UpdateTime = DateTime.Now,
    //            };
    //            await _rep.InsertAsync(eGTakeStock);
    //        }
    //        else
    //        {
    //            // 差值数量
    //            int diffCount = (int)(sumCount - takestockcount);

    //            // 盘亏
    //            if (diffCount > 0)
    //            {
    //                EGTakeStock eGTakeStock = new EGTakeStock()
    //                {
    //                    TakeStockNum = takestocknum,
    //                    MaterielNum = materielnum,
    //                    // 盘点状态
    //                    TakeStockStatus = 2,
    //                    TakeStockDiffCount = diffCount,
    //                    TakeStockCount = takestockcount,
    //                    // 盘点类别
    //                    TakeStockType = 1,
    //                    CreateTime = DateTime.Now,
    //                    UpdateTime = DateTime.Now,
    //                };
    //                await _rep.InsertAsync(eGTakeStock);
    //            }
    //            // 盘赢
    //            else if (diffCount <= 0)
    //            {
    //                // 转换为绝对值
    //                int absDiffcount = Math.Abs(diffCount);

    //                EGTakeStock eGTakeStock = new EGTakeStock()
    //                {
    //                    TakeStockNum = takestocknum,
    //                    MaterielNum = materielnum,
    //                    // 盘点状态
    //                    TakeStockStatus = 1,
    //                    TakeStockDiffCount = absDiffcount,
    //                    TakeStockCount = takestockcount,
    //                    // 盘点类别
    //                    TakeStockType = 1,
    //                    CreateTime = DateTime.Now,
    //                    UpdateTime = DateTime.Now,
    //                };
    //                await _rep.InsertAsync(eGTakeStock);
    //            }

    //        }
    //    }
    //}
    #endregion

    #region （根据库位）输入盘点数量改变未盘点的状态

    /// <summary>
    ///（根据库位）输入盘点数量改变未盘点的状态
    /// </summary>
    /// <param name="takestocknum">盘点编号</param>
    /// <param name="takestockcount">盘点数量</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    //[HttpPut]
    //[ApiDescriptionSettings(Name = "UpdateTakeStockStorageNumMessageStatus")]
    //public async Task UpdateTakeStockStorageNumMessageStatus(string takestocknum, int takestockcount)
    //{
    //    // 根据用户输入的盘点编号查找相应记录
    //    List<EGTakeStock> is_vaild = await _rep.GetListAsync(u => u.TakeStockNum == takestocknum);
    //    if (is_vaild.Count == 0)
    //    {
    //        throw new Exception("为找到此条盘点记录");
    //    }
    //    else
    //    {
    //        string materielNum = is_vaild[0].MaterielNum;
    //        //List<EGInventory> data = await _model.GetListAsync(u => u.MaterielNum == materielNum);
    //        //int icountAll = (int)data[0].ICountAll;

    //        int sumCount = 0;
    //        // 获得物料编号相同的物料信息
    //        List<EGInventory> InventoryData = await _model.GetListAsync(u => u.MaterielNum == materielNum);
    //        // 得到库位上所有物料的数量
    //        for (int i = 0; i < InventoryData.Count; i++)
    //        {
    //            int icountAll = (int)InventoryData[i].ICountAll;
    //            sumCount += icountAll;
    //        }

    //        // 差值数量
    //        int diffCount = sumCount - takestockcount;
    //        // 盘亏
    //        if (diffCount > 0)
    //        {
    //            // 将盘点表中的盘点状态改编成盘亏
    //            _rep.AsUpdateable()
    //           .AS("EGTakeStock")
    //           .SetColumns(it => new EGTakeStock
    //           { TakeStockStatus = 2, TakeStockCount = takestockcount, TakeStockDiffCount = diffCount })
    //           .Where(u => u.TakeStockNum == takestocknum)
    //           .ExecuteCommand();

    //        }
    //        // 盘赢
    //        else if (diffCount <= 0)
    //        {
    //            // 转换为绝对值
    //            int absDiffcount = Math.Abs(diffCount);

    //            // 将盘点表中的盘点状态改编成盘赢
    //            _rep.AsUpdateable()
    //           .AS("EGTakeStock")
    //           .SetColumns(it => new EGTakeStock
    //           { TakeStockStatus = 1, TakeStockCount = takestockcount, TakeStockDiffCount = absDiffcount })
    //           .Where(u => u.TakeStockNum == takestocknum)
    //           .ExecuteCommand();
    //        }

    //    }

    //}

    #endregion

    #region （根据物料）生成盘点信息（可以输入数量，生成盘点记录；也可以不输入数量，生成未盘点记录）

    /// <summary>
    /// （根据物料）生成盘点信息（可以输入数量，生成盘点记录；也可以不输入数量，生成未盘点记录）
    /// </summary>
    /// <param name="materielnum">物料编号</param>
    /// <param name="takestockcount">盘点数量</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>

    //[HttpPost]
    //[ApiDescriptionSettings(Name = "AddTakeStockMessage")]
    //public async Task AddTakeStockMessage(string materielnum, int? takestockcount)
    //{

    //    // 自动生成时间戳
    //    string timesStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();

    //    string takestocknum = "EGPD" + timesStamp;

    //    List<EGInventory> is_valid = await _model.GetListAsync(u => u.MaterielNum == materielnum);
    //    if (is_valid.Count == 0)
    //    {
    //        throw new Exception("此条物料编号不存在！");
    //    }
    //    else
    //    {
    //        // 物料编号
    //        string materielNum = is_valid[0].MaterielNum;
    //        // 物料存量
    //        int icountAll = (int)is_valid[0].ICountAll;
    //        // 没有输入盘点数量
    //        if (takestockcount == null)
    //        {
    //            EGTakeStock eGTakeStock = new EGTakeStock()
    //            {
    //                TakeStockNum = takestocknum,
    //                MaterielNum = materielNum,
    //                // 盘点状态
    //                TakeStockStatus = 0,
    //                TakeStockDiffCount = null,
    //                TakeStockCount = null,
    //                // 盘点类别
    //                TakeStockType = 0,
    //                CreateTime = DateTime.Now,
    //                UpdateTime = DateTime.Now,
    //            };
    //            await _rep.InsertAsync(eGTakeStock);
    //        }
    //        else
    //        {
    //            // 差值数量
    //            int diffCount = (int)(icountAll - takestockcount);
    //            // 盘亏
    //            if (diffCount > 0)
    //            {
    //                EGTakeStock eGTakeStock = new EGTakeStock()
    //                {
    //                    TakeStockNum = takestocknum,
    //                    MaterielNum = materielNum,
    //                    // 盘点状态
    //                    TakeStockStatus = 2,
    //                    TakeStockDiffCount = diffCount,
    //                    TakeStockCount = takestockcount,
    //                    // 盘点类别
    //                    TakeStockType = 0,
    //                    CreateTime = DateTime.Now,
    //                    UpdateTime = DateTime.Now,
    //                };
    //                await _rep.InsertAsync(eGTakeStock);
    //            }
    //            // 盘赢
    //            else if (diffCount <= 0)
    //            {
    //                // 转换为绝对值
    //                int absDiffcount = Math.Abs(diffCount);

    //                EGTakeStock eGTakeStock = new EGTakeStock()
    //                {
    //                    TakeStockNum = takestocknum,
    //                    MaterielNum = materielNum,
    //                    // 盘点状态
    //                    TakeStockStatus = 1,
    //                    TakeStockDiffCount = absDiffcount,
    //                    TakeStockCount = takestockcount,
    //                    // 盘点类别
    //                    TakeStockType = 0,
    //                    CreateTime = DateTime.Now,
    //                    UpdateTime = DateTime.Now,
    //                };
    //                await _rep.InsertAsync(eGTakeStock);
    //            }
    //        }
    //    }
    //}

    #endregion

    #region （根据物料）输入盘点数量改变未盘点的状态

    /// <summary>
    /// （根据物料）输入盘点数量改变未盘点的状态
    /// </summary>
    /// <param name="takestocknum">盘点编号</param>
    /// <param name="takestockcount">盘点数量</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    //[HttpPut]
    //[ApiDescriptionSettings(Name = "UpdateTakeStockMaterielNumMessageStatus")]
    //public async Task UpdateTakeStockMaterielNumMessageStatus(string takestocknum, int takestockcount)
    //{
    //    // 根据用户输入的盘点编号查找相应记录
    //    List<EGTakeStock> is_vaild = await _rep.GetListAsync(u => u.TakeStockNum == takestocknum);
    //    if (is_vaild.Count == 0)
    //    {
    //        throw new Exception("为找到此条盘点记录");
    //    }
    //    else
    //    {
    //        string materielNum = is_vaild[0].MaterielNum;
    //        List<EGInventory> data = await _model.GetListAsync(u => u.MaterielNum == materielNum);
    //        int icountAll = (int)data[0].ICountAll;
    //        // 差值数量
    //        int diffCount = icountAll - takestockcount;
    //        // 盘亏
    //        if (diffCount > 0)
    //        {
    //            // 将盘点表中的盘点状态改编成盘亏
    //            _rep.AsUpdateable()
    //           .AS("EGTakeStock")
    //           .SetColumns(it => new EGTakeStock
    //           { TakeStockStatus = 2, TakeStockCount = takestockcount, TakeStockDiffCount = diffCount })
    //           .Where(u => u.TakeStockNum == takestocknum)
    //           .ExecuteCommand();

    //        }
    //        // 盘赢
    //        else if (diffCount <= 0)
    //        {
    //            // 转换为绝对值
    //            int absDiffcount = Math.Abs(diffCount);

    //            // 将盘点表中的盘点状态改编成盘赢
    //            _rep.AsUpdateable()
    //           .AS("EGTakeStock")
    //           .SetColumns(it => new EGTakeStock
    //           { TakeStockStatus = 1, TakeStockCount = takestockcount, TakeStockDiffCount = absDiffcount })
    //           .Where(u => u.TakeStockNum == takestocknum)
    //           .ExecuteCommand();
    //        }

    //    }

    //}



    #endregion
}

