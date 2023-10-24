﻿namespace Admin.NET.Application;

/// <summary>
/// 出入库接口服务（agv、人工）
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class EG_WMS_InAndOutBoundService : IDynamicApiController, ITransient
{
    // agv接口
    TaskService taskService = new TaskService();

    #region 关系注入
    private readonly SqlSugarRepository<EG_WMS_InAndOutBound> _rep = App.GetService<SqlSugarRepository<EG_WMS_InAndOutBound>>();
    private readonly SqlSugarRepository<EG_WMS_InAndOutBoundDetail> _InAndOutBoundDetail = App.GetService<SqlSugarRepository<EG_WMS_InAndOutBoundDetail>>();
    private readonly SqlSugarRepository<EG_WMS_Inventory> _Inventory = App.GetService<SqlSugarRepository<EG_WMS_Inventory>>();
    private readonly SqlSugarRepository<EG_WMS_InventoryDetail> _InventoryDetail = App.GetService<SqlSugarRepository<EG_WMS_InventoryDetail>>();
    private readonly SqlSugarRepository<EG_WMS_Storage> _Storage = App.GetService<SqlSugarRepository<EG_WMS_Storage>>();
    private readonly SqlSugarRepository<EG_WMS_Region> _Region = App.GetService<SqlSugarRepository<EG_WMS_Region>>();
    private readonly SqlSugarRepository<EG_WMS_WorkBin> _workbin = App.GetService<SqlSugarRepository<EG_WMS_WorkBin>>();
    private readonly SqlSugarRepository<EG_WMS_Tem_Inventory> _InventoryTem = App.GetService<SqlSugarRepository<EG_WMS_Tem_Inventory>>();
    private readonly SqlSugarRepository<EG_WMS_Tem_InventoryDetail> _InventoryDetailTem = App.GetService<SqlSugarRepository<EG_WMS_Tem_InventoryDetail>>();


    #endregion

    #region 引用实体
    /// <summary>
    /// 出入库主表
    /// </summary>
    //private readonly SqlSugarRepository<EG_WMS_InAndOutBound> _rep;
    /// <summary>
    /// 出入库详细表
    /// </summary>
    //private readonly SqlSugarRepository<EG_WMS_InAndOutBoundDetail> _InAndOutBoundDetail;
    /// <summary>
    /// 库存表
    /// </summary>
    //private readonly SqlSugarRepository<EG_WMS_Inventory> _Inventory;
    /// <summary>
    /// 库存详情表
    /// </summary>
    //private readonly SqlSugarRepository<EG_WMS_InventoryDetail> _InventoryDetail;
    /// <summary>
    /// 库位表
    /// </summary>
    //private readonly SqlSugarRepository<EG_WMS_Storage> _Storage;
    /// <summary>
    /// 区域表
    /// </summary>
    //private readonly SqlSugarRepository<EG_WMS_Region> _Region;
    /// <summary>
    /// 料箱表
    /// </summary>
    //private readonly SqlSugarRepository<EG_WMS_WorkBin> _workbin;
    #endregion

    #region 关系注入

    public EG_WMS_InAndOutBoundService
        (
        //SqlSugarRepository<EG_WMS_InAndOutBound> rep,
        //SqlSugarRepository<EG_WMS_InAndOutBoundDetail> InAndOutBoundDetail,
        //SqlSugarRepository<EG_WMS_Inventory> Inventory,
        //SqlSugarRepository<EG_WMS_InventoryDetail> InventoryDetail,
        //SqlSugarRepository<EG_WMS_Storage> storage,
        //SqlSugarRepository<EG_WMS_Region> Region,
        //SqlSugarRepository<EG_WMS_WorkBin> WorkBin
        )
    {
        //_rep = rep;
        //_Inventory = Inventory;
        //_InventoryDetail = InventoryDetail;
        //_InAndOutBoundDetail = InAndOutBoundDetail;
        //_Storage = storage;
        //_Region = Region;
        //_workbin = WorkBin;
    }


    #endregion

    #region agv入库（两点位）（没有判断agv是否执行成功）（已完成）

    /// <summary>
    /// agv入库（两点位）（没有判断agv是否执行成功）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>

    [HttpPost]
    [ApiDescriptionSettings(Name = "AgvJoinBound")]

    public async Task AgvJoinBound(AgvJoinDto input)
    {
        // 生成当前时间时间戳
        string timesstamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        string joinboundnum = "EGRK" + timesstamp;

        #region 判断有无传入任务编号
        // 判断有无传入任务编号
        //string taskno;
        //if (input == null || input.TaskNo == null)
        //{
        //    var id = SnowFlakeSingle.instance.NextId().ToString();
        //    taskno = id;
        //}
        //else
        //{
        //    taskno = input.TaskNo;
        //}

        //var db = new SqlSugarClient(new ConnectionConfig
        //{
        //    ConnectionString = "server=127.0.0.1;Database=iwms;Uid=root;Pwd=123456;AllowLoadLocalInfile=true;min pool size=1",
        //    DbType = DbType.MySql,
        //    IsAutoCloseConnection = true,
        //});
        #endregion

        try
        {
            // 开启一个事务
            //db.BeginTran();

            // 起始点
            string startpoint = input.StartPoint;
            if (startpoint == null)
            {
                throw Oops.Oh("起始点不可为空");
            }

            // 目标点
            if (input.EndPoint == null)
            {
                // 根据策略推荐（修改）
            }

            string endpoint = input.EndPoint;

            #region 生成入库
            // 生成入库单
            EG_WMS_InAndOutBound joinbound = new EG_WMS_InAndOutBound
            {
                // 编号
                InAndOutBoundNum = joinboundnum,
                // 出入库类型（入库还是出库）
                InAndOutBoundType = 0,
                // 时间
                InAndOutBoundTime = DateTime.Now,
                // 操作人
                InAndOutBoundUser = input.AddName,
                // 备注
                InAndOutBoundRemake = input.InAndOutBoundRemake,
                // 创建时间
                CreateTime = DateTime.Now,
                // 起始点
                StartPoint = startpoint,
                // 目标点
                EndPoint = endpoint,
            };

            // 查询库位编号所在的区域编号
            var _storagelistdata = _Storage.AsQueryable()
                                   .Where(u => u.StorageNum == endpoint)
                                   .Select(f => new
                                   {
                                       f.RegionNum
                                   })
                                   .ToList();

            if (_storagelistdata.Count == 0 || string.IsNullOrEmpty(_storagelistdata[0].RegionNum))
            {
                throw Oops.Oh("没有查询到这个库位");
            }

            string regionnum = _storagelistdata[0].RegionNum;

            // 通过查询出来的区域得到仓库编号

            var _regionlistdata = _Region.AsQueryable()
                    .Where(u => u.RegionNum == regionnum)
                    .Select(f => new
                    {
                        f.WHNum
                    })
                    .ToList();

            if (_regionlistdata.Count == 0 || string.IsNullOrEmpty(_regionlistdata[0].WHNum))
            {
                throw Oops.Oh("没有查询到这个仓库");
            }

            // 生成入库详单

            EG_WMS_InAndOutBoundDetail joinbounddetail = new EG_WMS_InAndOutBoundDetail()
            {
                // 出入库编号
                InAndOutBoundNum = joinboundnum,
                CreateTime = DateTime.Now,
                // 区域编号
                RegionNum = _storagelistdata[0].RegionNum,
                // 目标点就是存储的点位即库位编号
                StorageNum = input.EndPoint,
            };

            await _rep.InsertAsync(joinbound);
            await _InAndOutBoundDetail.InsertAsync(joinbounddetail);
            #endregion

            // 任务点集
            var positions = startpoint + "," + endpoint;

            TaskEntity taskEntity = input.Adapt<TaskEntity>();
            taskEntity.TaskPath = positions;

            // 下达agv任务

            DHMessage item = await taskService.AddAsync(taskEntity);

            // 判断agv任务是否成功（修改）



            // 判断agv下达任务是否成功
            if (item.code == 1000)
            {

                // 得到入库的数据
                List<MaterielWorkBin> list = input.materielWorkBins;
                string wbnum = "";
                string wlnum = "";
                int sumcount = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    // 雪花ID
                    var idone = SnowFlakeSingle.instance.NextId();
                    var idtwo = SnowFlakeSingle.instance.NextId();
                    // 库存编号（主表和详细表）
                    string inventorynum = $"{i}EGKC" + timesstamp;
                    // 料箱编号（详细表、料箱表）
                    string workbinnum = list[i].WorkBinNum;
                    // 物料编号（主表）
                    string materienum = list[i].MaterielNum;
                    // 物料的数量（主表、料箱表）
                    int productcount = list[i].ProductCount;
                    // 生产日期（料箱表）
                    DateTime productiondate = list[i].ProductionDate;
                    // 生产批次（详细表、料箱表）
                    string productionlot = list[i].ProductionLot;

                    // 总数
                    sumcount += productcount;

                    // 将得到的数据，保存在库存主表和详细表中

                    // 库存主表
                    EG_WMS_Inventory addInventory = new EG_WMS_Inventory()
                    {
                        // 雪花id
                        Id = idone,
                        // 库存编号
                        InventoryNum = inventorynum,
                        // 物料编号
                        MaterielNum = materienum,
                        // 库存总数
                        ICountAll = productcount,
                        // 创建时间
                        CreateTime = DateTime.Now,
                        // 入库编号
                        InAndOutBoundNum = joinboundnum,
                        // 是否删除
                        IsDelete = false,
                        // 是否出库
                        OutboundStatus = 0,
                    };

                    // 详细表
                    EG_WMS_InventoryDetail addInventoryDetail = new EG_WMS_InventoryDetail()
                    {
                        // 雪花id
                        Id = idtwo,
                        // 库存编号
                        InventoryNum = inventorynum,
                        // 料箱编号
                        WorkBinNum = workbinnum,
                        // 生产批次
                        ProductionLot = productionlot,
                        // 创建时间
                        CreateTime = DateTime.Now,
                        // 库位编号
                        StorageNum = input.EndPoint,
                        // 区域编号
                        RegionNum = _storagelistdata[0].RegionNum,
                        // 仓库编号
                        WHNum = _regionlistdata[0].WHNum,
                        // 是否删除
                        IsDelete = false,

                    };

                    // 料箱表 将料箱内容保存到料箱表中
                    EG_WMS_WorkBin addWorkBin = new EG_WMS_WorkBin()
                    {
                        // 编号
                        WorkBinNum = workbinnum,
                        // 产品数量
                        ProductCount = productcount,
                        // 生产批次
                        ProductionLot = productionlot,
                        CreateTime = DateTime.Now,
                        // 生产日期
                        ProductionDate = productiondate,
                        WorkBinStatus = 0,
                        MaterielNum = materienum,
                        // 库位编号
                        StorageNum = input.EndPoint,
                    };

                    await _Inventory.InsertAsync(addInventory);
                    await _InventoryDetail.InsertAsync(addInventoryDetail);
                    await _workbin.InsertOrUpdateAsync(addWorkBin);

                    // 得到每个料箱编号
                    if (list.Count > 1)
                    {
                        wbnum = workbinnum + "," + wbnum;
                        wlnum = materienum + "," + wlnum;
                    }
                    else
                    {
                        wbnum = workbinnum;
                        wlnum = materienum;
                    }

                    // 提交事务
                    //db.CommitTran();
                }

                // 修改入库详情表里面的料箱编号和物料编号

                await _InAndOutBoundDetail.AsUpdateable()
                                     .AS("EG_WMS_InAndOutBoundDetail")
                                     .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                                     {
                                         WHNum = _regionlistdata[0].WHNum,
                                         WorkBinNum = wbnum,
                                         MaterielNum = wlnum,
                                     })
                                     .Where(u => u.InAndOutBoundNum == joinboundnum)
                                     .ExecuteCommandAsync();

                // 改变入库状态
                await _rep.AsUpdateable()
                     .AS("EG_WMS_InAndOutBound")
                     .SetColumns(it => new EG_WMS_InAndOutBound
                     {
                         InAndOutBoundCount = sumcount,
                         InAndOutBoundStatus = 1,
                         SuccessOrNot = 0,
                     })
                     .Where(u => u.InAndOutBoundNum == joinboundnum)
                     .ExecuteCommandAsync();

            }
            else
            {
                // 失败时，将生成的入库单修改为失败
                await _rep.AsUpdateable()
                     .AS("EG_WMS_InAndOutBound")
                     .SetColumns(it => new EG_WMS_InAndOutBound
                     {
                         // 入库失败
                         SuccessOrNot = 1,
                         // 未入库
                         InAndOutBoundStatus = 0,
                     })
                     .Where(u => u.InAndOutBoundNum == joinboundnum)
                     .ExecuteCommandAsync();

                throw new Exception("agv任务执行失败");
            }

        }
        catch (Exception ex)
        {
            // 回滚事务
            //db.RollbackTran();
            throw new Exception(ex.Message);
        }
    }

    #endregion

    #region agv入库（完整）（已完成）
    /// <summary>
    /// agv入库（两点位）（完整）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "AgvJoinBoundTask")]
    public async Task AgvJoinBoundTask(AgvJoinDto input)
    {
        // 生成当前时间时间戳
        string timesstamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        string joinboundnum = "EGRK" + timesstamp;

        try
        {
            // 起始点
            string startpoint = input.StartPoint;
            if (startpoint == null)
            {
                throw Oops.Oh("起始点不可为空");
            }

            // 目标点
            if (input.EndPoint == null)
            {
                // 根据策略推荐（修改）
            }

            string endpoint = input.EndPoint;

            // 任务点集
            var positions = startpoint + "," + endpoint;

            TaskEntity taskEntity = input.Adapt<TaskEntity>();
            taskEntity.TaskPath = positions;
            taskEntity.InAndOutBoundNum = joinboundnum;

            // 下达agv任务

            DHMessage item = await taskService.AddAsync(taskEntity);

            // 下达agv任务成功
            if (item.code == 1000)
            {
                //使用Task.Delay方法让当前任务暂停执行1秒钟（1000毫秒）。await关键字表示等待异步操作完成。
                //await Task.Delay(1000);

                try
                {
                    #region 入库操作

                    // 生成入库单
                    EG_WMS_InAndOutBound joinbound = new EG_WMS_InAndOutBound
                    {
                        // 编号
                        InAndOutBoundNum = joinboundnum,
                        // 出入库类型（入库还是出库）
                        InAndOutBoundType = 0,
                        // 时间
                        InAndOutBoundTime = DateTime.Now,
                        // 操作人
                        InAndOutBoundUser = input.AddName,
                        // 备注
                        InAndOutBoundRemake = input.InAndOutBoundRemake,
                        // 创建时间
                        CreateTime = DateTime.Now,
                        // 起始点
                        StartPoint = input.StartPoint,
                        // 目标点
                        EndPoint = input.EndPoint,
                    };
                    // 查询库位编号所在的区域编号
                    var _storagelistdata = _Storage.AsQueryable()
                                           .Where(u => u.StorageNum == input.EndPoint)
                                           .Select(f => new
                                           {
                                               f.RegionNum
                                           })
                                           .ToList();

                    if (_storagelistdata.Count == 0 || string.IsNullOrEmpty(_storagelistdata[0].RegionNum))
                    {
                        throw Oops.Oh("没有查询到这个库位");
                    }

                    string regionnum = _storagelistdata[0].RegionNum;

                    // 通过查询出来的区域得到仓库编号

                    var _regionlistdata = _Region.AsQueryable()
                            .Where(u => u.RegionNum == regionnum)
                            .Select(f => new
                            {
                                f.WHNum
                            })
                            .ToList();

                    if (_regionlistdata.Count == 0 || string.IsNullOrEmpty(_regionlistdata[0].WHNum))
                    {
                        throw Oops.Oh("没有查询到这个仓库");
                    }

                    // 生成入库详单

                    EG_WMS_InAndOutBoundDetail joinbounddetail = new EG_WMS_InAndOutBoundDetail()
                    {
                        // 出入库编号
                        InAndOutBoundNum = joinboundnum,
                        CreateTime = DateTime.Now,
                        // 区域编号
                        RegionNum = _storagelistdata[0].RegionNum,
                        // 目标点就是存储的点位即库位编号
                        StorageNum = input.EndPoint,
                    };
                    #endregion

                    await _rep.InsertAsync(joinbound);
                    await _InAndOutBoundDetail.InsertAsync(joinbounddetail);

                    // 得到入库的数据
                    List<MaterielWorkBin> list = input.materielWorkBins;
                    string wbnum = "";
                    string wlnum = "";
                    int sumcount = 0;
                    for (int i = 0; i < list.Count; i++)
                    {
                        // 雪花ID
                        //var idone = SnowFlakeSingle.instance.NextId();
                        //var idtwo = SnowFlakeSingle.instance.NextId();
                        // 库存编号（主表和详细表）
                        string inventorynum = $"{i}EGKC" + timesstamp;
                        // 料箱编号（详细表、料箱表）
                        string workbinnum = list[i].WorkBinNum;
                        // 物料编号（主表）
                        string materienum = list[i].MaterielNum;
                        // 物料的数量（主表、料箱表）
                        int productcount = list[i].ProductCount;
                        // 生产日期（料箱表）
                        DateTime productiondate = list[i].ProductionDate;
                        // 生产批次（详细表、料箱表）
                        string productionlot = list[i].ProductionLot;

                        // 总数
                        sumcount += productcount;

                        // 将得到的数据，保存在临时的库存主表和详细表中

                        // 临时库存主表
                        EG_WMS_Tem_Inventory addInventory = new EG_WMS_Tem_Inventory()
                        {
                            // 雪花id
                            //Id = idone,
                            // 库存编号
                            InventoryNum = inventorynum,
                            // 物料编号
                            MaterielNum = materienum,
                            // 库存总数
                            ICountAll = productcount,
                            // 创建时间
                            CreateTime = DateTime.Now,
                            // 入库编号
                            InAndOutBoundNum = joinboundnum,
                            // 是否删除
                            IsDelete = false,
                            // 是否出库
                            OutboundStatus = 0,
                        };

                        // 临时详细表
                        EG_WMS_Tem_InventoryDetail addInventoryDetail = new EG_WMS_Tem_InventoryDetail()
                        {
                            // 雪花id
                            //Id = idtwo,
                            // 库存编号
                            InventoryNum = inventorynum,
                            // 料箱编号
                            WorkBinNum = workbinnum,
                            // 生产批次
                            ProductionLot = productionlot,
                            // 创建时间
                            CreateTime = DateTime.Now,
                            // 库位编号
                            StorageNum = input.EndPoint,
                            // 区域编号
                            RegionNum = _storagelistdata[0].RegionNum,
                            // 仓库编号
                            WHNum = _regionlistdata[0].WHNum,
                            // 是否删除
                            IsDelete = false,

                        };

                        // 料箱表 将料箱内容保存到料箱表中
                        EG_WMS_WorkBin addWorkBin = new EG_WMS_WorkBin()
                        {
                            // 编号
                            WorkBinNum = workbinnum,
                            // 产品数量
                            ProductCount = productcount,
                            // 生产批次
                            ProductionLot = productionlot,
                            CreateTime = DateTime.Now,
                            // 生产日期
                            ProductionDate = productiondate,
                            WorkBinStatus = 0,
                            MaterielNum = materienum,
                            // 库位编号
                            StorageNum = input.EndPoint,
                        };

                        // 将数据保存到临时表中
                        await _InventoryTem.InsertAsync(addInventory);
                        await _InventoryDetailTem.InsertAsync(addInventoryDetail);
                        await _workbin.InsertOrUpdateAsync(addWorkBin);


                        // 得到每个料箱编号
                        if (list.Count > 1)
                        {
                            wbnum = workbinnum + "," + wbnum;
                            wlnum = materienum + "," + wlnum;
                        }
                        else
                        {
                            wbnum = workbinnum;
                            wlnum = materienum;
                        }
                    }
                    // 修改入库详情表里面的料箱编号和物料编号

                    await _InAndOutBoundDetail.AsUpdateable()
                                         .AS("EG_WMS_InAndOutBoundDetail")
                                         .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                                         {
                                             WHNum = _regionlistdata[0].WHNum,
                                             WorkBinNum = wbnum,
                                             MaterielNum = wlnum,
                                         })
                                         .Where(u => u.InAndOutBoundNum == joinboundnum)
                                         .ExecuteCommandAsync();

                    // 改变入库状态
                    await _rep.AsUpdateable()
                         .AS("EG_WMS_InAndOutBound")
                         .SetColumns(it => new EG_WMS_InAndOutBound
                         {
                             InAndOutBoundCount = sumcount,
                             // 入库中
                             InAndOutBoundStatus = 4,
                         })
                         .Where(u => u.InAndOutBoundNum == joinboundnum)
                         .ExecuteCommandAsync();

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                throw new Exception("下达AGV任务失败");

            }
        }
        catch (Exception ex)
        {

            throw new Exception(ex.Message);
        }

    }

    #endregion

    #region agv出库（两点位）（没有判断agv是否执行成功）（已完成）

    /// <summary>
    /// agv出库（没有判断agv是否执行成功）（两点位）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>

    [HttpPost]
    [ApiDescriptionSettings(Name = "AgvOutBound")]

    public async Task AgvOutBound(AgvJoinDto input)
    {
        // 生成当前时间时间戳
        string timesstamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        string outboundnum = "EGCK" + timesstamp;

        try
        {
            // 开启一个事务
            //db.BeginTran();

            // 起始点
            string startpoint = input.StartPoint;
            if (startpoint == null)
            {
                throw Oops.Oh("起始点不可为空");
            }

            // 目标点
            string endpoint = input.EndPoint;
            if (endpoint == null)
            {
                // 根据策略推荐
            }

            #region 生成出库

            // 生成出库单
            EG_WMS_InAndOutBound outbound = new EG_WMS_InAndOutBound
            {
                // 编号
                InAndOutBoundNum = outboundnum,
                // 出入库类型（入库还是出库）
                InAndOutBoundType = 1,
                // 时间
                InAndOutBoundTime = DateTime.Now,
                // 操作人
                InAndOutBoundUser = input.AddName,
                // 备注
                InAndOutBoundRemake = input.InAndOutBoundRemake,
                // 创建时间
                CreateTime = DateTime.Now,
                // 起始点
                StartPoint = startpoint,
                // 目标点
                EndPoint = endpoint,
            };

            // 查询库位编号所在的区域编号
            var _storagelistdata = _Storage.AsQueryable()
                                   .Where(u => u.StorageNum == endpoint)
                                   .Select(f => new
                                   {
                                       f.RegionNum
                                   })
                                   .ToList();

            if (_storagelistdata.Count == 0 || string.IsNullOrEmpty(_storagelistdata[0].RegionNum))
            {
                throw Oops.Oh("没有查询到这个库位");
            }

            string regionnum = _storagelistdata[0].RegionNum;

            // 通过查询出来的区域得到仓库编号

            var _regionlistdata = _Region.AsQueryable()
                    .Where(u => u.RegionNum == regionnum)
                    .Select(f => new
                    {
                        f.WHNum
                    })
                    .ToList();

            if (_regionlistdata.Count == 0 || string.IsNullOrEmpty(_regionlistdata[0].WHNum))
            {
                throw Oops.Oh("没有查询到这个仓库");
            }

            // 生成出库详单

            EG_WMS_InAndOutBoundDetail outbounddetail = new EG_WMS_InAndOutBoundDetail()
            {
                // 出入库编号
                InAndOutBoundNum = outboundnum,
                CreateTime = DateTime.Now,
                // 区域编号
                RegionNum = _storagelistdata[0].RegionNum,
                // 目标点就是存储的点位即库位编号
                StorageNum = input.EndPoint,
            };

            await _rep.InsertAsync(outbound);
            await _InAndOutBoundDetail.InsertAsync(outbounddetail);
            #endregion


            // 任务点集
            var positions = startpoint + "," + endpoint;



            TaskEntity taskEntity = input.Adapt<TaskEntity>();
            taskEntity.TaskPath = positions;

            // 下达agv任务

            DHMessage item = await taskService.AddAsync(taskEntity);

            // 判断agv入库是否成功

            if (item.code == 1000)
            {

                // 得到出库的数据
                List<MaterielWorkBin> list = input.materielWorkBins;
                string wbnum = "";
                string wlnum = "";
                int sumcount = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    // 查询出出库数据的料箱编号
                    var inum = _InventoryDetail.AsQueryable()
                                     .Where(u => u.WorkBinNum == list[i].WorkBinNum)
                                     .Select(f => new
                                     {
                                         f.InventoryNum
                                     })
                                     .ToList();


                    // 库存编号（主表和详细表）
                    string inventorynum = inum[0].InventoryNum;
                    // 料箱编号（详细表）
                    string workbinnum = list[i].WorkBinNum;
                    // 物料编号（主表）
                    string materienum = list[i].MaterielNum;
                    // 物料的数量（主表）
                    int productcount = list[i].ProductCount;
                    // 生产日期（）
                    DateTime productiondate = list[i].ProductionDate;
                    // 生产批次（详细表）
                    string productionlot = list[i].ProductionLot;

                    // 总数
                    sumcount += productcount;

                    // 将得到的数据，修改在库存主表和详细表中
                    // 库存主表

                    await _Inventory.AsUpdateable()
                     .AS("EG_WMS_Inventory")
                     .SetColumns(it => new EG_WMS_Inventory
                     {
                         // 改变出库状态
                         OutboundStatus = 1,

                     })
                     // 库存编号相同
                     .Where(u => u.InventoryNum == inventorynum)
                     .ExecuteCommandAsync();

                    // 得到每个料箱编号
                    if (list.Count > 1)
                    {
                        wbnum = workbinnum + "," + wbnum;
                        wlnum = materienum + "," + wlnum;
                    }
                    else
                    {
                        wbnum = workbinnum;
                        wlnum = materienum;
                    }
                    // 提交事务
                    //db.CommitTran();
                }

                // 修改出库详情表里面的料箱编号和物料编号

                await _InAndOutBoundDetail.AsUpdateable()
                                     .AS("EG_WMS_InAndOutBoundDetail")
                                     .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                                     {
                                         WHNum = _regionlistdata[0].WHNum,
                                         WorkBinNum = wbnum,
                                         MaterielNum = wlnum,
                                     })
                                     .Where(u => u.InAndOutBoundNum == outboundnum)
                                     .ExecuteCommandAsync();

                // 改变出库状态
                await _rep.AsUpdateable()
                     .AS("EG_WMS_InAndOutBound")
                     .SetColumns(it => new EG_WMS_InAndOutBound
                     {
                         // 总数
                         InAndOutBoundCount = sumcount,
                         InAndOutBoundStatus = 3,
                         SuccessOrNot = 0,
                     })
                     .Where(u => u.InAndOutBoundNum == outboundnum)
                     .ExecuteCommandAsync();

            }
            else
            {

                // 失败时，将生成的出库单修改为失败
                await _rep.AsUpdateable()
                     .AS("EG_WMS_InAndOutBound")
                     .SetColumns(it => new EG_WMS_InAndOutBound
                     {
                         // 出库失败
                         SuccessOrNot = 1,
                         // 未出库
                         InAndOutBoundStatus = 2,
                     })
                     .Where(u => u.InAndOutBoundNum == outboundnum)
                     .ExecuteCommandAsync();

                throw new Exception("agv任务执行失败");
            }

        }
        catch (Exception ex)
        {
            // 回滚事务
            //db.RollbackTran();
            throw new Exception(ex.Message);
        }




    }
    #endregion

    #region agv出库（完整）（已完成）

    /// <summary>
    /// agv出库（两点位）（完整）
    /// </summary>
    /// <returns></returns>
    public async Task AgvOutBoundTask(AgvJoinDto input)
    {
        // 生成当前时间时间戳
        string timesstamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        string outboundnum = "EGCK" + timesstamp;

        try
        {
            // 起始点
            string startpoint = input.StartPoint;
            if (startpoint == null)
            {
                throw Oops.Oh("起始点不可为空");
            }

            // 目标点
            string endpoint = input.EndPoint;
            if (endpoint == null)
            {
                // 根据策略推荐
            }

            // 任务点集
            var positions = startpoint + "," + endpoint;

            TaskEntity taskEntity = input.Adapt<TaskEntity>();
            taskEntity.TaskPath = positions;
            taskEntity.InAndOutBoundNum = outboundnum;

            // 下达agv任务

            DHMessage item = await taskService.AddAsync(taskEntity);

            // 判断agv入库是否成功

            if (item.code == 1000)
            {

                #region 生成出库

                // 生成出库单
                EG_WMS_InAndOutBound outbound = new EG_WMS_InAndOutBound
                {
                    // 编号
                    InAndOutBoundNum = outboundnum,
                    // 出入库类型（入库还是出库）
                    InAndOutBoundType = 1,
                    // 时间
                    InAndOutBoundTime = DateTime.Now,
                    // 操作人
                    InAndOutBoundUser = input.AddName,
                    // 备注
                    InAndOutBoundRemake = input.InAndOutBoundRemake,
                    // 创建时间
                    CreateTime = DateTime.Now,
                    // 起始点
                    StartPoint = startpoint,
                    // 目标点
                    EndPoint = endpoint,
                };

                // 查询库位编号所在的区域编号
                var _storagelistdata = _Storage.AsQueryable()
                                       .Where(u => u.StorageNum == endpoint)
                                       .Select(f => new
                                       {
                                           f.RegionNum
                                       })
                                       .ToList();

                if (_storagelistdata.Count == 0 || string.IsNullOrEmpty(_storagelistdata[0].RegionNum))
                {
                    throw Oops.Oh("没有查询到这个库位");
                }

                string regionnum = _storagelistdata[0].RegionNum;

                // 通过查询出来的区域得到仓库编号

                var _regionlistdata = _Region.AsQueryable()
                        .Where(u => u.RegionNum == regionnum)
                        .Select(f => new
                        {
                            f.WHNum
                        })
                        .ToList();

                if (_regionlistdata.Count == 0 || string.IsNullOrEmpty(_regionlistdata[0].WHNum))
                {
                    throw Oops.Oh("没有查询到这个仓库");
                }

                // 生成出库详单

                EG_WMS_InAndOutBoundDetail outbounddetail = new EG_WMS_InAndOutBoundDetail()
                {
                    // 出入库编号
                    InAndOutBoundNum = outboundnum,
                    CreateTime = DateTime.Now,
                    // 区域编号
                    RegionNum = _storagelistdata[0].RegionNum,
                    // 目标点就是存储的点位即库位编号
                    StorageNum = input.EndPoint,
                };

                await _rep.InsertAsync(outbound);
                await _InAndOutBoundDetail.InsertAsync(outbounddetail);
                #endregion

                // 得到出库的数据
                List<MaterielWorkBin> list = input.materielWorkBins;
                string wbnum = "";
                string wlnum = "";
                int sumcount = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    // 查询出出库数据的料箱编号
                    var inum = _InventoryDetail.AsQueryable()
                                     .Where(u => u.WorkBinNum == list[i].WorkBinNum)
                                     .Select(f => new
                                     {
                                         f.InventoryNum
                                     })
                                     .ToList();

                    // 库存编号（主表和详细表）
                    string inventorynum = inum[0].InventoryNum;
                    // 料箱编号（详细表）
                    string workbinnum = list[i].WorkBinNum;
                    // 物料编号（主表）
                    string materienum = list[i].MaterielNum;
                    // 物料的数量（主表）
                    int productcount = list[i].ProductCount;
                    // 生产日期（）
                    DateTime productiondate = list[i].ProductionDate;
                    // 生产批次（详细表）
                    string productionlot = list[i].ProductionLot;

                    // 总数
                    sumcount += productcount;

                    // 将得到的数据，修改先修改到临时库存主表和详细表中
                    // 临时库存主表

                    await _InventoryTem.AsUpdateable()
                     .AS("EG_WMS_Inventory")
                     .SetColumns(it => new EG_WMS_Tem_Inventory
                     {
                         // 改变出库状态
                         OutboundStatus = 1,

                     })
                     // 库存编号相同
                     .Where(u => u.InventoryNum == inventorynum)
                     .ExecuteCommandAsync();

                    // 得到每个料箱编号
                    if (list.Count > 1)
                    {
                        wbnum = workbinnum + "," + wbnum;
                        wlnum = materienum + "," + wlnum;
                    }
                    else
                    {
                        wbnum = workbinnum;
                        wlnum = materienum;
                    }
                }

                // 修改出库详情表里面的料箱编号和物料编号
                await _InAndOutBoundDetail.AsUpdateable()
                                     .AS("EG_WMS_InAndOutBoundDetail")
                                     .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                                     {
                                         WHNum = _regionlistdata[0].WHNum,
                                         WorkBinNum = wbnum,
                                         MaterielNum = wlnum,
                                     })
                                     .Where(u => u.InAndOutBoundNum == outboundnum)
                                     .ExecuteCommandAsync();

                // 改变出库状态
                await _rep.AsUpdateable()
                     .AS("EG_WMS_InAndOutBound")
                     .SetColumns(it => new EG_WMS_InAndOutBound
                     {
                         // 总数
                         InAndOutBoundCount = sumcount,
                         // 出库中
                         InAndOutBoundStatus = 5,
                     })
                     .Where(u => u.InAndOutBoundNum == outboundnum)
                     .ExecuteCommandAsync();

            }
            else
            {
                throw new Exception("下达agv任务失败");
            }

        }
        catch (Exception ex)
        {

            throw new Exception(ex.Message);
        }

    }




    #endregion

    #region 人工入库（已完成）
    /// <summary>
    /// 人工入库
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>

    [HttpPost]
    [ApiDescriptionSettings(Name = "ArtificialJoinBoundAdd")]
    public async Task ArtificialJoinBoundAdd(EGInBoundDto input)
    {
        // 生成当前时间时间戳
        string timesstamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        // 自动生成入库编号
        string joinboundnum = "EGRK" + timesstamp;

        try
        {
            // 生成入库单
            EG_WMS_InAndOutBound inandoutbound = new EG_WMS_InAndOutBound
            {
                // 入库编号
                InAndOutBoundNum = joinboundnum,
                // 类型
                InAndOutBoundType = 0,
                // 时间
                InAndOutBoundTime = DateTime.Now,
                // 操作人（修改）
                InAndOutBoundUser = input.InAndOutBoundUser,
                // 入库备注
                InAndOutBoundRemake = input.InAndOutBoundRemake,
                // 入库目标点
                EndPoint = input.EndPoint,

            };

            #region 根据库位查询区域、仓库

            // 查询库位编号所在的区域编号
            var _storagelistdata = _Storage.AsQueryable()
                                   .Where(u => u.StorageNum == input.EndPoint)
                                   .Select(f => new
                                   {
                                       f.RegionNum
                                   })
                                   .ToList();

            if (_storagelistdata.Count == 0 || string.IsNullOrEmpty(_storagelistdata[0].RegionNum))
            {
                throw Oops.Oh("没有查询到这个库位");
            }

            string regionnum = _storagelistdata[0].RegionNum;

            // 通过查询出来的区域得到仓库编号

            var _regionlistdata = _Region.AsQueryable()
                    .Where(u => u.RegionNum == regionnum)
                    .Select(f => new
                    {
                        f.WHNum
                    })
                    .ToList();

            if (_regionlistdata.Count == 0 || string.IsNullOrEmpty(_regionlistdata[0].WHNum))
            {
                throw Oops.Oh("没有查询到这个仓库");
            }
            #endregion

            // 生成入库详单
            EG_WMS_InAndOutBoundDetail inandoutbounddetail = new EG_WMS_InAndOutBoundDetail
            {
                // 入库编号
                InAndOutBoundNum = joinboundnum,
                // 库位编号
                StorageNum = input.EndPoint,
                // 区域编号
                RegionNum = regionnum,
                // 仓库编号
                WHNum = _regionlistdata[0].WHNum,
                // 时间
                CreateTime = DateTime.Now,
            };


            // 保存到数据库中
            await _rep.InsertAsync(inandoutbound);
            await _InAndOutBoundDetail.InsertAsync(inandoutbounddetail);

            // 得到入库的数据
            List<MaterielWorkBin> item = input.materielWorkBins;

            // 循环遍历，一共有多少个需要入库的料箱
            string wbnum = "";
            string wlnum = "";
            int sumcount = 0;
            for (int i = 0; i < item.Count; i++)
            {
                // 雪花ID
                var idone = SnowFlakeSingle.instance.NextId();
                var idtwo = SnowFlakeSingle.instance.NextId();
                // 库存编号（主表和详细表）
                string inventorynum = $"{i}EGKC" + timesstamp;
                // 料箱编号（详细表）
                string workbinnum = item[i].WorkBinNum;
                // 物料编号（主表）
                string materienum = item[i].MaterielNum;
                // 物料的数量（主表）
                int productcount = item[i].ProductCount;
                // 生产日期（料箱表）
                DateTime productiondate = item[i].ProductionDate;
                // 生产批次（详细表）
                string productionlot = item[i].ProductionLot;

                sumcount += productcount;
                // 将得到的数据，保存在库存主表和详细表中

                // 库存主表
                EG_WMS_Inventory addInventory = new EG_WMS_Inventory()
                {
                    // 雪花id
                    Id = idone,
                    // 库存编号
                    InventoryNum = inventorynum,
                    // 物料编号
                    MaterielNum = materienum,
                    // 库存总数
                    ICountAll = productcount,
                    // 创建时间
                    CreateTime = DateTime.Now,
                    // 入库编号
                    InAndOutBoundNum = joinboundnum,
                    // 是否删除
                    IsDelete = false,
                    // 是否出库
                    OutboundStatus = 0,
                };

                // 详细表
                EG_WMS_InventoryDetail addInventoryDetail = new EG_WMS_InventoryDetail()
                {
                    // 雪花id
                    Id = idtwo,
                    // 库存编号
                    InventoryNum = inventorynum,
                    // 料箱编号
                    WorkBinNum = workbinnum,
                    // 生产批次
                    ProductionLot = productionlot,
                    // 创建时间
                    CreateTime = DateTime.Now,
                    // 库位编号
                    StorageNum = input.EndPoint,
                    // 区域编号
                    RegionNum = regionnum,
                    // 仓库编号
                    WHNum = _regionlistdata[0].WHNum,
                    // 是否删除
                    IsDelete = false,

                };

                // 料箱表 将料箱内容保存到料箱表中（生成新料箱或修改）
                EG_WMS_WorkBin addWorkBin = new EG_WMS_WorkBin()
                {
                    // 编号
                    WorkBinNum = workbinnum,
                    // 产品数量
                    ProductCount = productcount,
                    // 生产批次
                    ProductionLot = productionlot,
                    CreateTime = DateTime.Now,
                    // 生产日期
                    ProductionDate = productiondate,
                    WorkBinStatus = 0,
                    MaterielNum = materienum,
                    // 库位编号
                    StorageNum = input.EndPoint,
                };

                await _Inventory.InsertAsync(addInventory);
                await _InventoryDetail.InsertAsync(addInventoryDetail);
                await _workbin.InsertOrUpdateAsync(addWorkBin);

                // 得到每个料箱编号
                if (item.Count > 1)
                {
                    wbnum = workbinnum + "," + wbnum;
                    wlnum = materienum + "," + wlnum;
                }
                else
                {
                    wbnum = workbinnum;
                    wlnum = materienum;
                }
            }

            // 修改入库详情表里面的料箱编号

            await _InAndOutBoundDetail.AsUpdateable()
                                .AS("EG_WMS_InAndOutBoundDetail")
                                .SetColumns(it => new EG_WMS_InAndOutBoundDetail
                                {
                                    MaterielNum = wlnum,
                                    WorkBinNum = wbnum,
                                })
                                .Where(u => u.InAndOutBoundNum == joinboundnum)
                                .ExecuteCommandAsync();

            // 改变入库状态
            await _rep.AsUpdateable()
                  .AS("EG_WMS_InAndOutBound")
                  .SetColumns(it => new EG_WMS_InAndOutBound
                  {
                      // 总数
                      InAndOutBoundCount = sumcount,
                      InAndOutBoundStatus = 1,
                      SuccessOrNot = 0,
                  })
                  .Where(u => u.InAndOutBoundNum == joinboundnum)
                  .ExecuteCommandAsync();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }



    #endregion

    #region 人工出库（已完成）（人工扫描库位库位上所有的料箱都出）

    /// <summary>
    /// 人工出库（人工扫描库位库位上所有的料箱都出）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>

    [HttpPost]
    [ApiDescriptionSettings(Name = "ArtificialOutBoundAdd")]
    public async Task ArtificialOutBoundAdd(EGOutBoundDto input)
    {

        // 生成当前时间时间戳
        string timesstamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        // 自动生成出库编号
        string Outboundnum = "EGCK" + timesstamp;

        try
        {
            // 人拿着pda扫
            // 扫库位，得到这个库位上所有的料箱，所有的料箱全部都出
            // 得到pda扫到的库位上所有的料箱（根据库位得到这个库位上所有的料箱(从库存中得到)）

            #region 通过库位查询得到区域编号、仓库编号

            // 查询库位编号所在的区域编号
            var _storagelistdata = _Storage.AsQueryable()
                                   .Where(u => u.StorageNum == input.StorageNum)
                                   .Select(f => new
                                   {
                                       f.RegionNum
                                   })
                                   .ToList();

            if (_storagelistdata.Count == 0 || string.IsNullOrEmpty(_storagelistdata[0].RegionNum))
            {
                throw Oops.Oh("没有查询到这个库位");
            }

            string regionnum = _storagelistdata[0].RegionNum;

            // 通过查询出来的区域得到仓库编号

            var _regionlistdata = _Region.AsQueryable()
                    .Where(u => u.RegionNum == regionnum)
                    .Select(f => new
                    {
                        f.WHNum
                    })
                    .ToList();

            if (_regionlistdata.Count == 0 || string.IsNullOrEmpty(_regionlistdata[0].WHNum))
            {
                throw Oops.Oh("没有查询到这个仓库");
            }
            #endregion

            // 1.得到这个库位上所有的数据

            List<EG_WMS_InventoryDetail> dataList =
            _InventoryDetail.AsQueryable()
            .InnerJoin<EG_WMS_Inventory>((a, b) => a.InventoryNum == b.InventoryNum)
            .Where((a, b) => a.StorageNum == input.StorageNum && b.OutboundStatus == 0)
            .ToList();

            if (dataList.Count == 0)
            {
                throw Oops.Oh("此库位上没有数据");
            }

            // 2.得到这个库位上所有的库存

            // 所有料箱的总数
            int? sumcount = 0;

            // 生成出库单
            EG_WMS_InAndOutBound inandoutbound = new EG_WMS_InAndOutBound
            {
                // 出库编号
                InAndOutBoundNum = Outboundnum,
                // 类型
                InAndOutBoundType = 1,
                // 时间
                InAndOutBoundTime = DateTime.Now,
                // 出库备注
                InAndOutBoundRemake = input.Remake,
                CreateTime = DateTime.Now,
            };

            // 生成出库详单
            EG_WMS_InAndOutBoundDetail inandoutbounddetail = new EG_WMS_InAndOutBoundDetail
            {
                // 出库编号
                InAndOutBoundNum = Outboundnum,
                // 库位编号
                StorageNum = input.StorageNum,
                // 区域编号
                RegionNum = regionnum,
                // 仓库编号
                WHNum = _regionlistdata[0].WHNum,
                // 时间
                CreateTime = DateTime.Now,
            };

            // 保存到数据库中
            await _rep.InsertAsync(inandoutbound);
            await _InAndOutBoundDetail.InsertAsync(inandoutbounddetail);

            string wbnum = "";
            string wlnum = "";
            for (int i = 0; i < dataList.Count; i++)
            {
                // 得到库存表中的每个物料和料箱编号
                string workbinnum = dataList[i].WorkBinNum;
                string materienum = dataList[i].MaterielNum;
                if (dataList.Count > 1)
                {
                    wbnum += workbinnum + ",";
                    wlnum += materienum + ",";
                }
                else
                {
                    wbnum = workbinnum;
                    wlnum = materienum;
                }
                // 得到每一个料箱里面的数量
                var _incountall = _Inventory.AsQueryable()
                           .Where(u => u.InventoryNum == dataList[i].InventoryNum)
                           .Select(f => new
                           {
                               f.ICountAll
                           })
                           .ToList();

                sumcount += _incountall[0].ICountAll;

                // 将库存表中的出库状态改变
                await _Inventory.AsUpdateable()
                           .AS("EG_WMS_Inventory")
                           .SetColumns(u => new EG_WMS_Inventory
                           {
                               OutboundStatus = 1,
                           })
                           .Where(it => it.InventoryNum == dataList[i].InventoryNum)
                           .ExecuteCommandAsync();


            }

            // 修改出入库主表
            await _rep.AsUpdateable()
                 .AS("EG_WMS_InAndOutBound")
                 .SetColumns(u => new EG_WMS_InAndOutBound
                 {
                     InAndOutBoundStatus = 3,
                     // 出入库数量
                     InAndOutBoundCount = sumcount,

                 })
                 .Where(it => it.InAndOutBoundNum == Outboundnum)
                 .ExecuteCommandAsync();


            // 修改出入库详情表
            await _InAndOutBoundDetail.AsUpdateable()
                                      .AS("EG_WMS_InAndOutBoundDetail")
                                      .SetColumns(u => new EG_WMS_InAndOutBoundDetail
                                      {
                                          MaterielNum = wlnum,
                                          WorkBinNum = wbnum,

                                      })
                                      .ExecuteCommandAsync();


        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }


    #endregion

    #region 分页查询出入库信息
    /// <summary>
    /// 分页查询出入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<EG_WMS_InAndOutBoundOutput>> Page(EG_WMS_InAndOutBoundInput input)
    {
        var query = _rep.AsQueryable()
                    .WhereIF(!string.IsNullOrWhiteSpace(input.InAndOutBoundNum), u => u.InAndOutBoundNum.Contains(input.InAndOutBoundNum.Trim()))
                    .WhereIF(input.InAndOutBoundType > 0, u => u.InAndOutBoundType == input.InAndOutBoundType)
                    .WhereIF(input.InAndOutBoundStatus > 0, u => u.InAndOutBoundStatus == input.InAndOutBoundStatus)
                    .WhereIF(input.InAndOutBoundCount > 0, u => u.InAndOutBoundCount == input.InAndOutBoundCount)
                    .WhereIF(!string.IsNullOrWhiteSpace(input.InAndOutBoundUser), u => u.InAndOutBoundUser.Contains(input.InAndOutBoundUser.Trim()))
                    .WhereIF(!string.IsNullOrWhiteSpace(input.InAndOutBoundRemake), u => u.InAndOutBoundRemake.Contains(input.InAndOutBoundRemake.Trim()))

                    // 倒序
                    .OrderBy(it => it.CreateTime, OrderByType.Desc)
                    .Select<EG_WMS_InAndOutBoundOutput>();

        // 日期查询
        if (input.InAndOutBoundTimeRange != null && input.InAndOutBoundTimeRange.Count > 0)
        {
            DateTime? start = input.InAndOutBoundTimeRange[0];
            query = query.WhereIF(start.HasValue, u => u.InAndOutBoundTime > start);
            if (input.InAndOutBoundTimeRange.Count > 1 && input.InAndOutBoundTimeRange[1].HasValue)
            {
                var end = input.InAndOutBoundTimeRange[1].Value.AddDays(1);
                query = query.Where(u => u.InAndOutBoundTime < end);
            }
        }
        query = query.OrderBuilder(input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }
    #endregion

    #region 删除出入库信息
    /// <summary>
    /// 删除出入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteEG_WMS_InAndOutBoundInput input)
    {
        var entity = await _rep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _rep.FakeDeleteAsync(entity);   //假删除
    }

    #endregion

    #region 获取出入库信息
    /// <summary>
    /// 获取出入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<EG_WMS_InAndOutBound> Get([FromQuery] QueryByIdEG_WMS_InAndOutBoundInput input)
    {
        return await _rep.GetFirstAsync(u => u.Id == input.Id);
    }
    #endregion

    #region 获取出入库信息列表
    /// <summary>
    /// 获取出入库信息列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpGet]
    [ApiDescriptionSettings(Name = "List")]
    public async Task<List<EG_WMS_InAndOutBoundOutput>> List([FromQuery] EG_WMS_InAndOutBoundInput input)
    {
        return await _rep.AsQueryable().Select<EG_WMS_InAndOutBoundOutput>().ToListAsync();
    }
    #endregion

    //-------------------------------------//-------------------------------------//

    #region AddDTO
    //AddDTO addDTO = new AddDTO()
    //{
    //    // 任务编号
    //    //TaskNo = taskno,
    //    // 任务名称
    //    TaskName = input.TaskName,
    //    // 优先级
    //    Priority = input.Priority,
    //    // 来源
    //    Source = input.Source,
    //    // 模板编号
    //    ModelNo = input.ModelNo,
    //    // 任务下达人
    //    AddName = input.AddName,
    //    // 是否追加任务
    //    IsAdd = input.IsAdd,

    //    TaskDetail = new List<TaskDetailDTO> { new TaskDetailDTO()
    //    {
    //        // 任务点集
    //        Positions = positions
    //    }}
    //};
    #endregion

    #region MyRegion
    //list<egworkbin> data = new list<egworkbin>();

    //foreach (var workbindetail in input.workbindetaildto)
    //{
    //    // 这边应该是有多个料箱信息，需要foreach循环，得到每一条数据
    //    string workbinnum = "eglx" + timesstamp;

    //    list<egworkbin> workbin = (list<egworkbin>)input.workbindetaildto.select(workbin => new egworkbin
    //    {
    //        workbinnum = workbinnum,
    //        materielnum = input.materielnum,
    //        productcount = workbin.productcount,
    //        productiondate = datetime.now,
    //        productionlot = workbin.productionlot,

    //    });

    //    data.add(workbin); // 将每个料箱添加到 data 列表中

    //}
    #endregion

    #region 添加入库信息（agv）

    /// <summary>
    /// 生成入库单，调用agv接口，实现入库
    /// 还未实现：根据策略自动推荐库位
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>

    //[HttpPost]
    //public async Task AGVJoinBoundAdd(EGInBoundDto input)
    //{
    //    // 生成当前时间时间戳
    //    string timesstamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
    //    // 自动生成入库编号
    //    string joinboundnum = "EGRK" + timesstamp;

    //    // 生成入库单
    //    EG_WMS_InAndOutBound joinbound = new EG_WMS_InAndOutBound
    //    {
    //        // 编号
    //        InAndOutBoundNum = joinboundnum,
    //        // 出入库类型（入库还是出库）
    //        InAndOutBoundType = 0,
    //        // 时间
    //        InAndOutBoundTime = DateTime.Now,
    //        // 操作人
    //        InAndOutBoundUser = input.InAndOutBoundUser,
    //        // 备注
    //        InAndOutBoundRemake = input.InAndOutBoundRemake,
    //        // 创建时间
    //        CreateTime = DateTime.Now,
    //        // 栈板编号，根据宇翔项目不使用
    //        //palletnum = input.palletnum,
    //    };

    //    await _rep.InsertAsync(joinbound);

    //    // 调用AGV的接口
    //    //TaskService.AddAsync();


    //    // 调用agv接口，任务下发
    //    // 先查询agv需要将货物运到库位上是否正在进行的任务
    //    // 查询agv任务，里面正在进行的任务点位，以及送到那个库位上
    //    // 思路：
    //    //      1、查询正在进行任务的agv
    //    //      2、查询得到agv现在正在送到那个库位上    
    //    //      3、将属于这个库位上的一组全部给暂时封锁，不允许其他的agv进来进行任务
    //    //      （一种解决方法，去查询agv任务表，查询里面的任务状态，
    //    //       只查询正在进行的任务，然后在根据任务编号，联表查询出，
    //    //       正在进行agv任务的点位库位，有查询到和当前任务点位库位相一致的话，
    //    //       就不允许下发任务）



    //    // 检查目标库位是否已被占用  
    //    //if (targetDock != null && ongoingTasks.Any(t => t.Dock == targetDock))
    //    //{
    //    //    // 库位已被占用，通知AGV等待  
    //    //    await SendAGVWaitInstruction(task.AGVId);
    //    //    return false;
    //    //}
    //    // 下发AGV任务  
    //    //await AddAGVTaskToDatabase(task);
    //    //return true;
    //    //}
    //    //      4、agv任务完成后，将agv所携带的库存保存，并且解锁这一组库位


    //    try
    //    {
    //        // 这里需要判断agv是否执行成功操作
    //        // Agv任务执行成功
    //        if (true)
    //        {
    //            // 执行成功将agv所携带的库存信息，添加到库存表中
    //            var item = input.materielWorkBins;
    //            EG_WMS_Inventory eG_WMS_Inventory = new();
    //            EG_WMS_InventoryDetail eG_WMS_InventoryDetail = new();

    //            for (int i = 0; i < item.Count; i++)
    //            {
    //                string inventorynum = $"{i}EGRK" + timesstamp;
    //                // 物料编号
    //                string materielnum = item[i].MaterielNum;
    //                // 数量
    //                int productcount = item[i].ProductCount;
    //                // 生产日期
    //                DateTime dateTime = item[i].ProductionDate;
    //                // 生产批次
    //                string productionlot = item[i].ProductionLot;
    //                // 料箱编号
    //                string workbinnum = item[i].WorkBinNum;

    //                // 库存主表
    //                _Inventory.AsInsertable(eG_WMS_Inventory)
    //                          .InsertColumns(u => new EG_WMS_Inventory
    //                          {
    //                              // 库存编号
    //                              InventoryNum = inventorynum,
    //                              // 物料编号
    //                              MaterielNum = materielnum,
    //                              // 创建时间
    //                              CreateTime = DateTime.Now,
    //                              // 入库编号
    //                              InAndOutBoundNum = joinboundnum,
    //                              // 库存总数
    //                              ICountAll = productcount,
    //                              // 未出库
    //                              OutboundStatus = 0,
    //                          })
    //                          .ExecuteCommand();

    //                // 库存详细表
    //                _InventoryDetail.AsInsertable(eG_WMS_InventoryDetail)
    //                                .InsertColumns(u => new EG_WMS_InventoryDetail
    //                                {
    //                                    // 库存编号(需要和库存主表相匹配)
    //                                    InventoryNum = inventorynum,
    //                                    // 创建时间
    //                                    CreateTime = DateTime.Now,
    //                                    // 料箱编号
    //                                    WorkBinNum = workbinnum,
    //                                })
    //                                .ExecuteCommand();
    //            }
    //            // 等待所有的库存都保存成功后在修改
    //            // 修改出入库主表里面的状态字段
    //            _rep.AsUpdateable()
    //                .AS("EG_WMS_InAndOutBound")
    //                .SetColumns(it => new EG_WMS_InAndOutBound
    //                {
    //                    InAndOutBoundStatus = 1,
    //                    SuccessOrNot = 0,

    //                })
    //                .ExecuteCommand();
    //        }
    //        // Agv任务执行失败
    //        else
    //        {
    //            // 将当前任务单的是否执行成功状态改变成为执行失败
    //            _rep.AsUpdateable()
    //                .AS("EG_WMS_InAndOutBound")
    //                .SetColumns(it => new EG_WMS_InAndOutBound
    //                { SuccessOrNot = 1 })
    //                .Where(u => u.InAndOutBoundNum == joinboundnum)
    //                .ExecuteCommand();
    //        }
    //    }
    //    catch (Exception)
    //    {
    //        throw new Exception("异常！");
    //    }



    //}
    #endregion

    #region 增加出入库信息
    /// <summary>
    /// 增加出入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    //[HttpPost]
    //[ApiDescriptionSettings(Name = "Add")]
    //public async Task Add(AddEG_WMS_InAndOutBoundInput input)
    //{
    //    var entity = input.Adapt<EG_WMS_InAndOutBound>();
    //    await _rep.InsertAsync(entity);
    //}
    #endregion

    #region 更新出入库信息
    /// <summary>
    /// 更新出入库信息
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    //[HttpPost]
    //[ApiDescriptionSettings(Name = "Update")]
    //public async Task Update(UpdateEG_WMS_InAndOutBoundInput input)
    //{
    //    var entity = input.Adapt<EG_WMS_InAndOutBound>();
    //    await _rep.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    //}
    #endregion

    #region 插入指定库存主表
    // 库存主表
    //await _Inventory.AsInsertable(inventory)
    //           .InsertColumns(u => new EG_WMS_Inventory
    //           {
    //               // 雪花id
    //               Id = idone,
    //               // 库存编号
    //               InventoryNum = inventorynum,
    //               // 物料编号
    //               MaterielNum = materienum,
    //               // 库存总数
    //               ICountAll = productcount,
    //               // 创建时间
    //               CreateTime = DateTime.Now,
    //               // 入库编号
    //               InAndOutBoundNum = joinboundnum,
    //               // 是否删除
    //               IsDelete = false,
    //           })
    //           .ExecuteCommandAsync();
    #endregion

    #region 插入指定库存详情表
    //await _InventoryDetail.AsInsertable(InventoryDetail)
    // .InsertColumns(u => new EG_WMS_InventoryDetail
    // {
    //     // 雪花id
    //     Id = idtwo,
    //     // 库存编号
    //     InventoryNum = inventorynum,
    //     // 料箱编号
    //     WorkBinNum = workbinnum,
    //     // 生产批次
    //     ProductionLot = productionlot,
    //     // 创建时间
    //     CreateTime = DateTime.Now,
    //     // 库位编号
    //     StorageNum = input.StorageNum,
    //     // 仓库编号
    //     WHNum = input.WHNum,
    //     // 是否删除
    //     IsDelete = false,
    // })
    // .ExecuteCommandAsync();
    #endregion
}