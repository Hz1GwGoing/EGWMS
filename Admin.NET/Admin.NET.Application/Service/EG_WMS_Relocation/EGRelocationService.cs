﻿namespace Admin.NET.Application;

/// <summary>
/// 移库信息接口
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EGRelocationService : IDynamicApiController, ITransient
{
    private static readonly ToolTheCurrentTime currentTime = new ToolTheCurrentTime();
    private static readonly EG_WMS_InAndOutBoundMessage messageApplication = new EG_WMS_InAndOutBoundMessage();

    #region 引用实体
    private readonly SqlSugarRepository<EG_WMS_Relocation> _Relocation;
    private readonly SqlSugarRepository<EG_WMS_InventoryDetail> _InventoryDetail;
    private readonly SqlSugarRepository<EG_WMS_WorkBin> _WorkBin;
    private readonly SqlSugarRepository<EG_WMS_Tem_Inventory> _InventoryTem;
    private readonly SqlSugarRepository<EG_WMS_Inventory> _Inventory;
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
         SqlSugarRepository<EG_WMS_Inventory> Inventory,
         SqlSugarRepository<EG_WMS_Tem_InventoryDetail> InventoryDetailTem,
         SqlSugarRepository<EG_WMS_Storage> Storage
        )
    {
        _Relocation = Relocation;
        _InventoryDetail = InventoryDetail;
        _WorkBin = WorkBin;
        _InventoryTem = InventoryTem;
        _Inventory = Inventory;
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
        string relacationnum = currentTime.GetTheCurrentTimeTimeStamp("EGYK");

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
                throw Oops.Oh("没有找到这条料箱");
            }


            // 获得到这个料箱起始库位的库位编号
            string oldStorageNum = listData[0].StorageNum;

            if (oldStorageNum == input.GOStorageNum)
            {
                throw Oops.Oh("移动的库位不能和当前库位相同");
            }

            // 生成一条移库记录
            EG_WMS_Relocation _relocation = new EG_WMS_Relocation()
            {
                RelocatioNum = relacationnum,
                OldStorageNum = oldStorageNum,
                NewStorageNum = input.GOStorageNum,
                RelocationCount = listData[0].ICountAll,
                MaterielNum = listData[0].MaterielNum,
                WorkBinNum = input.WorkBinNum,
                RelocationRemake = input.RelocationRemake,
                RelocationTime = DateTime.Now,
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

            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    // 修改这个料箱的库存数据
                    await _InventoryDetail.AsUpdateable()
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
                                  .SetColumns(it => new EG_WMS_WorkBin
                                  {
                                      StorageNum = input.GOStorageNum,
                                      UpdateTime = DateTime.Now,
                                      WorkBinRemake = remake,
                                  })
                                  .Where(u => u.WorkBinNum == input.WorkBinNum)
                                  .ExecuteCommandAsync();

                    // 修改库位占用
                    // 旧库位取消占用（密集库单移料箱无意义）
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        catch (Exception ex)
        {

            throw Oops.Oh(ex.Message);
        }



    }
    #endregion

    #region 移动整个密集库库位，上面的料箱数据

    /// <summary>
    /// 移动整个密集库库位，上面的料箱数据
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "MoveTheEntireDenseLibrary")]
    public async Task MoveTheEntireDenseLibrary(RelocationBO input)
    {
        string relacationnum = currentTime.GetTheCurrentTimeTimeStamp("EGYK");

        // 查询当前和目标库位上有没有数据
        var inventoryStart = _Inventory.AsQueryable()
                               .InnerJoin<EG_WMS_InventoryDetail>((a, b) => a.InventoryNum == b.InventoryNum)
                               .Where((a, b) => b.StorageNum == input.StartPoint && a.OutboundStatus == 0)
                               .Select((a, b) => new
                               {
                                   a.ICountAll,
                                   a.InventoryRemake,
                                   a.InventoryNum,
                                   b.WorkBinNum,
                                   a.MaterielNum
                               })
                               .ToList();

        var inventoryEnd = _Inventory.AsQueryable()
                               .InnerJoin<EG_WMS_InventoryDetail>((a, b) => a.InventoryNum == b.InventoryNum)
                               .Where((a, b) => b.StorageNum == input.GoEndPoint && a.OutboundStatus == 0)
                               .Select((a, b) => new
                               {
                                   b.StorageNum
                               })
                               .ToList();

        if (inventoryStart.Count == 0 || inventoryEnd.Count != 0)
        {
            throw Oops.Oh("当前库位没有存放数据或目标库位上有数据！");
        }

        // 查询原始库位
        var storagePrimitive = await _Storage.GetFirstAsync(x => x.StorageNum == input.StartPoint);

        using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {

            try
            {
                // 生成一条移库记录
                EG_WMS_Relocation _relocation = new EG_WMS_Relocation()
                {
                    RelocatioNum = relacationnum,
                    OldStorageNum = input.StartPoint,
                    NewStorageNum = input.GoEndPoint,
                    RelocationTime = DateTime.Now,
                    CreateTime = DateTime.Now,
                };

                await _Relocation.InsertAsync(_relocation);
                string remake;
                if (input.Remake == null || String.IsNullOrEmpty(input.Remake))
                {
                    remake = $"此库存已从{input.StartPoint}库位，移动到当前库位";
                }
                else
                {
                    remake = $"此库存已从{input.StartPoint}库位，移动到当前库位"
                              + "," + input.Remake;
                }
                string newregionnum = messageApplication.GetStorageWhereRegion(input.GoEndPoint);
                string newwhnum = messageApplication.GetRegionWhereWHNum(newregionnum);
                int? sumcount = 0;

                for (int i = 0; i < inventoryStart.Count; i++)
                {
                    sumcount += inventoryStart[i].ICountAll;

                    // 临时表也需要操作
                    await _InventoryTem.AsUpdateable()
                                       .SetColumns(it => new EG_WMS_Tem_Inventory
                                       {
                                           InventoryRemake = (inventoryStart[i].InventoryRemake ?? "") + remake,
                                           UpdateTime = DateTime.Now,
                                       })
                                       .Where(x => x.InventoryNum == inventoryStart[i].InventoryNum)
                                       .ExecuteCommandAsync();

                    await _InventoryDetailTem.AsUpdateable()
                                         .SetColumns(it => new EG_WMS_Tem_InventoryDetail
                                         {
                                             RegionNum = newregionnum,
                                             WHNum = newwhnum,
                                             StorageNum = input.GoEndPoint,
                                             UpdateTime = DateTime.Now,
                                         })
                                         .Where(x => x.InventoryNum == inventoryStart[i].InventoryNum)
                                         .ExecuteCommandAsync();

                    await _Inventory.AsUpdateable()
                                    .SetColumns(it => new EG_WMS_Inventory
                                    {
                                        InventoryRemake = (inventoryStart[i].InventoryRemake ?? "") + remake,
                                        UpdateTime = DateTime.Now,
                                    })
                                    .Where(x => x.InventoryNum == inventoryStart[i].InventoryNum)
                                    .ExecuteCommandAsync();

                    await _InventoryDetail.AsUpdateable()
                                          .SetColumns(it => new EG_WMS_InventoryDetail
                                          {
                                              RegionNum = newregionnum,
                                              WHNum = newwhnum,
                                              StorageNum = input.GoEndPoint,
                                              UpdateTime = DateTime.Now,
                                          })
                                          .Where(x => x.InventoryNum == inventoryStart[i].InventoryNum)
                                          .ExecuteCommandAsync();

                    await _WorkBin.AsUpdateable()
                                  .SetColumns(it => new EG_WMS_WorkBin
                                  {
                                      StorageNum = input.GoEndPoint,
                                      UpdateTime = DateTime.Now,
                                      WorkBinRemake = remake,
                                  })
                                  .Where(u => u.WorkBinNum == inventoryStart[i].WorkBinNum)
                                  .ExecuteCommandAsync();
                }
                // 修改移库记录
                await _Relocation.AsUpdateable()
                            .SetColumns(it => new EG_WMS_Relocation
                            {
                                RelocationCount = sumcount,
                                MaterielNum = inventoryStart[0].MaterielNum,
                                RelocationRemake = input.Remake,
                            })
                            .Where(x => x.RelocatioNum == relacationnum)
                            .ExecuteCommandAsync();

                // 修改库位（原始）
                await _Storage.AsUpdateable()
                              .SetColumns(it => new EG_WMS_Storage
                              {
                                  StorageOccupy = 0,
                                  StorageProductionDate = null,
                                  UpdateTime = DateTime.Now
                              })
                              .Where(x => x.StorageNum == input.StartPoint)
                              .ExecuteCommandAsync();

                // 修改库位（移动后）
                await _Storage.AsUpdateable()
                              .SetColumns(it => new EG_WMS_Storage
                              {
                                  StorageOccupy = 1,
                                  StorageProductionDate = storagePrimitive.StorageProductionDate,
                                  UpdateTime = DateTime.Now,
                              })
                              .Where(x => x.StorageNum == input.GoEndPoint)
                              .ExecuteCommandAsync();

                scope.Complete();
            }
            catch (Exception ex)
            {
                scope.Dispose();
                throw Oops.Oh("错误：" + ex.Message);
            }
        }

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
    public async Task<SqlSugarPagedList<class1>> GetAllRelocationsAndStorageTime([FromBody] PagingTimeFrameBO input)
    {

        var data = _Relocation.AsQueryable()
                      .InnerJoin<EG_WMS_Materiel>((a, b) => a.MaterielNum == b.MaterielNum)
                      .WhereIF(input.dateTimes != null, a => a.RelocationTime > input.dateTimes[0] && a.RelocationTime < input.dateTimes[1])
                      .WhereIF(!string.IsNullOrWhiteSpace(input.materielName), (a, b) => b.MaterielName.Contains(input.materielName.Trim()))
                      .WhereIF(!string.IsNullOrWhiteSpace(input.materielSpecs), (a, b) => b.MaterielSpecs.Contains(input.materielSpecs.Trim()))
                      .Select((a, b) => new class1
                      {
                          id = a.Id,
                          RelocatioNum = a.RelocatioNum,
                          RelocationCount = (int)a.RelocationCount,
                          WorkBinNum = a.WorkBinNum,
                          RelocationTime = (DateTime)a.RelocationTime,
                          MaterielName = b.MaterielName,
                          MaterielSpecs = b.MaterielSpecs,
                          OldStorage = SqlFunc.Subqueryable<EG_WMS_Storage>().Where(s => s.StorageNum == a.OldStorageNum).Select(a => a.StorageName).ToString(),
                          NewStorage = SqlFunc.Subqueryable<EG_WMS_Storage>().Where(s => s.StorageNum == a.NewStorageNum).Select(a => a.StorageName).ToString()
                      });

        return await data.ToPagedListAsync(input.page, input.pageSize);

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
        public long id { get; set; }
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
        public string? materielName { get; set; }
        public string? materielSpecs { get; set; }
        public DateTime[]? dateTimes { get; set; }
    }




    #endregion
}

//-------------------------------------//-------------------------------------//

#region 移库操作

///// <summary>
///// 移库操作（更新详情表）
///// </summary>
///// <param name="materielnum">物料编号</param>
///// <param name="regionnum">区域编号</param>
///// <param name="storagenum">库位编号</param>
///// <returns></returns>
///// <exception cref="Exception"></exception>
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
