namespace Admin.NET.Application;

/// <summary>
/// 移库信息接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGRelocationService : IDynamicApiController, ITransient
{

    #region 引用实体
    private readonly SqlSugarRepository<EG_WMS_Relocation> _Relocation;
    private readonly SqlSugarRepository<EG_WMS_InventoryDetail> _InventoryDetail;
    private readonly SqlSugarRepository<EG_WMS_WorkBin> _WorkBin;
    private readonly SqlSugarRepository<EG_WMS_Tem_Inventory> _InventoryTem;
    private readonly SqlSugarRepository<EG_WMS_Tem_InventoryDetail> _InventoryDetailTem;
    private readonly SqlSugarRepository<EG_WMS_Storage> _Storage;

    #endregion

    #region 关系注入
    public EGRelocationService
        (
         SqlSugarRepository<EG_WMS_Relocation> Relocation,
         SqlSugarRepository<EG_WMS_InventoryDetail> InventoryDetail,
         SqlSugarRepository<EG_WMS_WorkBin> WorkBin,
         SqlSugarRepository<EG_WMS_Tem_Inventory> InventoryTem,
         SqlSugarRepository<EG_WMS_Tem_InventoryDetail> InventoryDetailTem,
         SqlSugarRepository<EG_WMS_Storage> Storage
        )
    {
        _Relocation = Relocation;
        _InventoryDetail = InventoryDetail;
        _WorkBin = WorkBin;
        _InventoryTem = InventoryTem;
        _InventoryDetailTem = InventoryDetailTem;
        _Storage = Storage;
    }
    #endregion

    #region 移库操作

    /// <summary>
    /// 移库（料箱）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "PDARelocation")]
    public async Task PDARelocation(RelocationDto input)
    {
        // 生成当前时间时间戳
        string timesstamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        string relacationnum = "EGYK" + timesstamp;

        try
        {
            // 得到需要移库的数据（料箱的编号）
            // 将这个料箱里面有关于库位的数据给修改，给移动到具体的库位
            // 通过料箱编号从库存中得到这个料箱的数据
            List<ViewOrder> listData = _InventoryDetail.AsQueryable()
                                           .InnerJoin<EG_WMS_Inventory>((o, cus) => o.InventoryNum == cus.InventoryNum)
                                           .Where(o => o.WorkBinNum == input.WorkBinNum)
                                           .Select((o, cus) => new ViewOrder
                                           {
                                               MaterielNum = cus.MaterielNum,
                                               StorageNum = o.StorageNum,
                                               ICountAll = (int)cus.ICountAll,
                                           })
                                           .ToList();
            if (listData.Count == 0)
            {
                throw new Exception("没有找到这条料箱");
            }


            // 获得到这个料箱起始库位的库位编号
            string oldStorageNum = listData[0].StorageNum;

            if (oldStorageNum == input.GOStorageNum)
            {
                throw new Exception("移动的库位不能和当前库位相同");
            }

            // 生成一条移库记录
            EG_WMS_Relocation _relocation = new EG_WMS_Relocation()
            {
                // 移库编号
                RelocatioNum = relacationnum,
                // 旧库位
                OldStorageNum = oldStorageNum,
                // 新库位
                NewStorageNum = input.GOStorageNum,
                // 移库数量
                RelocationCount = listData[0].ICountAll,
                // 物料编号
                MaterielNum = listData[0].MaterielNum,
                // 移库料箱
                WorkBinNum = input.WorkBinNum,
                // 备注
                RelocationRemake = input.RelocationRemake,
                // 移库时间
                RelocationTime = DateTime.Now,
                // 创建时间
                CreateTime = DateTime.Now,
            };

            string remake;

            if (input.RelocationRemake == null || String.IsNullOrEmpty(input.RelocationRemake))
            {
                remake = $"此库存已从{oldStorageNum}库位，移动到当前库位";
            }
            else
            {
                remake = $"此库存已从{oldStorageNum}库位，移动到当前库位"
                          + "," + input.RelocationRemake;
            }
            await _Relocation.InsertAsync(_relocation);


            // 修改这个料箱的库存数据
            await _InventoryDetail.AsUpdateable()
                             .AS("EG_WMS_InventoryDetail")
                             .SetColumns(u => new EG_WMS_InventoryDetail
                             {
                                 StorageNum = input.GOStorageNum,
                                 UpdateTime = DateTime.Now,
                                 InventoryDetailRemake = remake,
                             })
                             .Where(it => it.WorkBinNum == input.WorkBinNum)
                             .ExecuteCommandAsync();

            // 修改料箱表里面的数据

            await _WorkBin.AsUpdateable()
                          .AS("EG_WMS_WorkBin")
                          .SetColumns(it => new EG_WMS_WorkBin
                          {
                              StorageNum = input.GOStorageNum,
                              UpdateTime = DateTime.Now,
                              WorkBinRemake = remake,
                          })
                          .Where(u => u.WorkBinNum == input.WorkBinNum)
                          .ExecuteCommandAsync();

        }
        catch (Exception ex)
        {

            throw new Exception(ex.Message);
        }



    }
    #endregion

    #region 得到移库表关联库位关系（分页查询）

    /// <summary>
    /// 得到移库表关联库位关系（分页查询）
    /// </summary>
    /// <param name="page">页数</param>
    /// <param name="pageSize">每页记录数</param>
    /// <returns></returns>

    [HttpPost]
    [ApiDescriptionSettings(Name = "GetAllRelocationsAndStorage")]
    public List<class1> GetAllRelocationsAndStorage(int page, int pageSize)
    {

        return _Relocation.AsQueryable()
                     .InnerJoin<EG_WMS_Materiel>((a, b) => a.MaterielNum == b.MaterielNum)
                     .Select((a, b) => new class1
                     {
                         RelocatioNum = a.RelocatioNum,
                         RelocationCount = (int)a.RelocationCount,
                         WorkBinNum = a.WorkBinNum,
                         RelocationTime = (DateTime)a.RelocationTime,
                         MaterielName = b.MaterielName,
                         MaterielSpecs = b.MaterielSpecs,
                         OldStorage = SqlFunc.Subqueryable<EG_WMS_Storage>().Where(s => s.StorageNum == a.OldStorageNum).Select(a => a.StorageName).ToString(),
                         NewStorage = SqlFunc.Subqueryable<EG_WMS_Storage>().Where(s => s.StorageNum == a.NewStorageNum).Select(a => a.StorageName).ToString()
                     })
                     .Skip((page - 1) * pageSize)
                     .Take(pageSize)
                     .ToList();

    }

    #endregion

    #region 得到移库表关联库位关系（分页查询）（时间范围）
    /// <summary>
    /// 得到移库表关联库位关系（分页查询）（时间范围）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "GetAllRelocationsAndStorageTime")]
    public List<class1> GetAllRelocationsAndStorageTime([FromBody] PagingTimeFrameBO input)
    {

        return _Relocation.AsQueryable()
                     .InnerJoin<EG_WMS_Materiel>((a, b) => a.MaterielNum == b.MaterielNum)
                     .Select((a, b) => new class1
                     {
                         RelocatioNum = a.RelocatioNum,
                         RelocationCount = (int)a.RelocationCount,
                         WorkBinNum = a.WorkBinNum,
                         RelocationTime = (DateTime)a.RelocationTime,
                         MaterielName = b.MaterielName,
                         MaterielSpecs = b.MaterielSpecs,
                         OldStorage = SqlFunc.Subqueryable<EG_WMS_Storage>().Where(s => s.StorageNum == a.OldStorageNum).Select(a => a.StorageName).ToString(),
                         NewStorage = SqlFunc.Subqueryable<EG_WMS_Storage>().Where(s => s.StorageNum == a.NewStorageNum).Select(a => a.StorageName).ToString()
                     })
                     .Where(a => a.RelocationTime > input.dateTimes[0] && a.RelocationTime < input.dateTimes[1])
                     .Skip((input.page - 1) * input.pageSize)
                     .Take(input.pageSize)
                     .ToList();

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
        var query = _Relocation.AsQueryable()
                     .InnerJoin<EG_WMS_Materiel>((a, b) => a.MaterielNum == b.MaterielNum)
                    // 获取创建日期
                    .WhereIF(input.CreateTime > DateTime.MinValue, u => u.CreateTime >= input.CreateTime)
                    .Select<EGRelocationOutput>();

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
        var entity = await _Relocation.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _Relocation.FakeDeleteAsync(entity);   //假删除
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
        var entity = input.Adapt<EG_WMS_Relocation>();
        await _Relocation.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
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
    public async Task<EG_WMS_Relocation> Get([FromQuery] QueryByIdEGRelocationInput input)
    {
        //return await _rep.GetFirstAsync(u => u.Id == input.Id);

        // 模糊查询
        return await _Relocation.GetFirstAsync(u => u.RelocatioNum.Contains(input.RelocatioNum));

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
        return await _Relocation.AsQueryable().Select<EGRelocationOutput>().ToListAsync();
    }



    #endregion

    #region 联表查询类

    /// <summary>
    /// 联表查询类
    /// </summary>
    private class ViewOrder
    {
        /// <summary>
        /// 库位编号
        /// </summary>
        public string StorageNum { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int ICountAll { get; set; }

        /// <summary>
        /// 物料编号
        /// </summary>
        public string MaterielNum { get; set; }

    }

    /// <summary>
    /// 移库查询类
    /// </summary>
    public class class1
    {
        public string RelocatioNum { get; set; }
        public int RelocationCount { get; set; }
        public string WorkBinNum { get; set; }
        public DateTime RelocationTime { get; set; }
        public string MaterielName { get; set; }
        public string MaterielSpecs { get; set; }
        public string OldStorage { get; set; }
        public string NewStorage { get; set; }
    }

    public class PagingTimeFrameBO
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public DateTime[] dateTimes { get; set; }
    }




    #endregion
}

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

#region 添加一条（移库记录）
/// <summary>
/// 添加一条移库记录
/// </summary>
/// <param name="input">移库记录字段</param>
/// <returns></returns>
/// <exception cref="Exception"></exception>
//[HttpPost]
//[ApiDescriptionSettings(Name = "InsertRepositoryRecords")]

//public async Task InsertRepositoryRecords(EG_WMS_Relocation input)
//{
//    // 查询是否有一致的记录
//    var is_vaild = await _rep.GetFirstAsync(u => u.RelocatioNum == input.RelocatioNum);
//    if (is_vaild != null)
//    {
//        throw new Exception("已存在此移库记录！");
//    }
//    else
//    {
//        // 获得当前时间时间戳
//        string timesStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
//        input.RelocatioNum = "EGYK" + timesStamp;
//        await _rep.InsertAsync(input);
//    }
//}

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
//    [HttpPut]
//    [ApiDescriptionSettings(Name = "LibraryTransferOperation")]

//    public async Task LibraryTransferOperation(string materielnum, string regionnum, string storagenum)
//    {
//        // 在移库记录表中根据物料编号查询是否有相同信息的数据
//        var is_vaild = await _rep.GetListAsync(u => u.MaterielNum == materielnum);
//        if (is_vaild == null)
//        {
//            throw new Exception("未找到需要移库的数据！");
//        }
//        else
//        {
//            //修改
//            _model.AsUpdateable()
//           .AS("EGInventoryDetail")
//           .SetColumns(it => new EG_WMS_InventoryDetail { RegionNum = regionnum, StorageNum = storagenum, InventoryDetailRemake = "此物料已被移动到当前库位" })
//           .Where(u => u.MaterielNum == materielnum)
//           .ExecuteCommand();

//            #region 不实现（可以根据里面的物料的数量进行移库操作）
//            // 判断用户输入的移库数量在库存中是否足够
//            // 先查询库存表中，相关物料的可用数量

//            //var issable = _db.AsQueryable().Where(u => u.MaterielNum == materielnum).Select(it => it.IUsable).ToInt();
//            //List<EGInventory> data = _db.AsQueryable().Where(u => u.MaterielNum == materielnum).ToList();
//            //int issable = 0;
//            //foreach (var item in data)
//            //{
//            //    issable = (int)item.IUsable;
//            //}
//            //var newcount = issable - userInputCount;

//            //// 如果用户输入的数量大于库存表中的数量
//            //if (newcount < 0)
//            //{
//            //    throw new Exception("移库的数量大于库存中的数量，无法移库！");
//            //}
//            //else
//            //{
//            //    // 修改
//            //    _model.AsUpdateable()
//            //   .AS("EGInventoryDetail")
//            //   .SetColumns(it => new EGInventoryDetail { RegionNum = regionnum, StorageNum = storagenum, })
//            //   .Where(u => u.MaterielNum == materielnum);

//            // 如果移库的数量等于库存中的数量
//            //if (userInputCount == issable)
//            //{
//            //    // 修改
//            //    _model.AsUpdateable()
//            //   .AS("EGInventoryDetail")
//            //   .SetColumns(it => new EGInventoryDetail { RegionNum = regionnum, StorageNum = storagenum, })
//            //   .Where(u => u.MaterielNum == materielnum);
//            //}
//            // 如果移库的数量小于库存中的数量
//            //if (issable > userInputCount)
//            //{
//            //    List<EGInventory> data = await _db.GetListAsync(u => u.MaterielNum == materielnum);
//            //    foreach (EGInventory item in data)
//            //    {
//            //        var newid = item.Id;
//            //        var newicountall = item.ICountAll;
//            //    }
//            //    // 将库存表中的库存减少
//            //}
//            #endregion
//        }
//    }
#endregion

#region agv移库

//public async Task a(AgvJoinDto input)
//{
//    // 生成当前时间时间戳
//    string timesstamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
//    string relocationdnum = "EGYK" + timesstamp;

//    // 起始点
//    string startpoint = input.StartPoint;
//    if (startpoint == null)
//    {
//        throw Oops.Oh("起始点不可为空");
//    }

//    // 目标点
//    if (input.EndPoint == null)
//    {
//        // 根据策略推荐（修改）
//    }

//    string endpoint = input.EndPoint;

//    // 任务点集
//    var positions = startpoint + "," + endpoint;

//    TaskEntity taskEntity = input.Adapt<TaskEntity>();
//    taskEntity.TaskPath = positions;
//    taskEntity.InAndOutBoundNum = relocationdnum;

//    // 下达agv任务

//    DHMessage item = await taskService.AddAsync(taskEntity);

//    // 下达任务成功
//    if (item.code == 1000)
//    {
//        // 得到移库的料箱
//        List<MaterielWorkBin> dataList = input.materielWorkBins;

//        // 遍历一共有多少个需要移库的料箱
//        for (int i = 0; i < dataList.Count; i++)
//        {
//            string workbin = dataList[i].WorkBinNum;
//            string materielnum = dataList[i].MaterielNum;
//            string productionlot = dataList[i].ProductionLot;

//            // 根据条件去修改临时库存主表里面的库位信息



//        }



//    }


//}


#endregion

#region 得到移库表关联库位关系

//[HttpGet]
//[ApiDescriptionSettings(Name = "GetAllRelocationsAndStorage")]
//public List<EG_WMS_Relocation> GetAllRelocationsAndStorage(int page, int pageSize)
//{
//    var data = _Relocation.AsQueryable()
//                .InnerJoin<EG_WMS_Materiel>((x, m) => x.MaterielNum == m.MaterielNum)
//                .Includes(x => x.NewStorage)
//                .Includes(x => x.OldStorage)
//                .OrderBy(x => x.Id)
//                .Skip((page - 1) * pageSize)
//                .Take(pageSize)
//                .ToList();

//    return data;
//}

#endregion