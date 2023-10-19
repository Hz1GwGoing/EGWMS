//namespace admin.net.application.service.egjoinboundnew;
//public class egjoinboundnewservice
//{

//    #region 引用实体
//    private readonly SqlSugarRepository<entity.egjoinbound> _joinbound;
//    private readonly SqlSugarRepository<entity.eginventory> _inventory;
//    private readonly SqlSugarRepository<entity.eginventorydetail> _inventorydetail;
//    private readonly SqlSugarRepository<entity.egworkbin> _workbin;
//    #endregion

//    #region 关系注入
//    public egjoinboundnewservice
//        (
//          sqlsugarrepository<entity.egjoinbound> joinbound,
//          sqlsugarrepository<entity.eginventory> inventory,
//          sqlsugarrepository<entity.eginventorydetail> inventorydetail,
//          SqlSugarRepository<entity.egworkbin> workbin)
//    {
//        _joinbound = joinbound;
//        _inventory = inventory;
//        _inventorydetail = inventorydetail;
//        _workbin = workbin;
//    }

//    #endregion



//    #region 测试
//    //[httppost("inbound")]
//    //public async task<iactionresult> inbound([frombody] eginboundrequest request)
//    //{
//    //    if (!request.)
//    //    {
//    //        return badrequest(modelstate);
//    //    }

//    //    // 这里，你可以对request对象进行进一步的处理，比如验证其中的数据，处理异常等。  
//    //    // 然后，你可以调用相应的服务或管理器来处理agv入库的逻辑。  

//    //    var result = await _inboundservice.inbound(request);
//    //    if (result.succeeded)
//    //    {
//    //        return ok(); // 返回200 ok，表示请求成功  
//    //    }
//    //    else
//    //    {
//    //        return statuscode(statuscodes.status500internalservererror); // 返回500 internal server error，表示服务器内部错误  
//    //    }
//    //}

//    //[httppost]
//    //public async task<iactionresult> joinboundnew([frombody] eginboundrequest request)
//    //{
//    //    // 先判断是否是有效的数据
//    //    if (!modelstate.isvalid)
//    //    {

//    //        return badrequest(modelstate);
//    //    }

//    //    if (request.isagv)
//    //    {
//    //        // 处理agv入库逻辑，生成入库单、入库详单，生成agv任务，锁定目标库位等
//    //        // 这里需要根据业务需求编写相应的逻辑
//    //    }
//    //    else
//    //    {
//    //        // 处理人工入库逻辑，直接添加至入库记录和库存表中
//    //        // 这里需要根据业务需求编写相应的逻辑
//    //    }

//    //    // 返回请求成功的响应
//    //    return ok();
//    //}
//    #endregion

//    /// <summary>
//    /// 入库（山东宇翔）
//    /// </summary>
//    /// <returns></returns>
//    //public async task joinboundadd([frombody] egjoinboundnewdto input)
//    //{
//    //    // 生成当前时间时间戳
//    //    string timesstamp = new datetimeoffset(datetime.utcnow).tounixtimeseconds().tostring();
//    //    // 自动生成入库编号
//    //    string joinboundnum = "egrk" + timesstamp;

//    //    entity.egjoinbound joinbound = new entity.egjoinbound
//    //    {
//    //        joinboundnum = joinboundnum,
//    //        whnum = input.whnum,
//    //        materielnum = input.materielnum,
//    //        workbinnum = input.workbinnum,
//    //        joinboundtype = input.joinboundtype,
//    //        joinboundcount = input.joinboundcount,
//    //        createtime = datetime.now,
//    //        joinboundtime = datetime.now,
//    //        // 栈板编号，根据宇翔项目不使用
//    //        //palletnum = input.palletnum,
//    //        joinboundremake = input.joinboundremake,
//    //        joinboundstatus = input.joinboundstatus,
//    //    };
//    //        // 这里需要判断agv是否执行成功操作

//    //    list<egworkbin> data = new list<egworkbin>();

//    //    foreach (var workbindetail in input.workbindetaildto)
//    //    {
//    //        // 这边应该是有多个料箱信息，需要foreach循环，得到每一条数据
//    //        string workbinnum = "eglx" + timesstamp;

//    //        list<egworkbin> workbin = (list<egworkbin>)input.workbindetaildto.select(workbin => new egworkbin
//    //        {
//    //            workbinnum = workbinnum,
//    //            materielnum = input.materielnum,
//    //            productcount = workbin.productcount,
//    //            productiondate = datetime.now,
//    //            productionlot = workbin.productionlot,

//    //        });

//    //        data.add(workbin); // 将每个料箱添加到 data 列表中

//    //    }


//}




//    //[httppost("createinboundrequest")]
//    //public iactionresult createinboundrequest([frombody] eginboundrequest inboundrequest)
//    //{
//    //    // 如果为agv入库
//    //    if (inboundrequest.isagv)
//    //    {
//    //        // 生成入库单和入库详情
//    //   
//    //        var inboundorder = generateinboundorder(inboundrequest);
//    //        var agvtask = generateagvtask(inboundrequest, inboundorder);
//    //        locktargetstoragelocation(inboundrequest.targetpoint);

//    //        // 执行agv任务和入库操作
//    //        executeagvtask(agvtask);
//    //        completeinbound(inboundorder);

//    //        // 返回成功响应
//    //        return ok("agv入库请求已发送并处理");
//    //    }
//    //    else
//    //    {
//    //        // 非agv入库的处理逻辑
//    //        // 可根据需求执行其他操作

//    //        // 返回成功响应
//    //        return ok("非agv入库请求已发送并处理");
//    //    }
//    //}

//    /// <summary>
//    /// 生成入库单和入库详情
//    /// </summary>
//    /// <param name="inboundrequest"></param>
//    /// <returns></returns>
//    //private async task generateinboundorder(eginboundrequest inboundrequest)
//    //{
//    //    // 生成入库单和入库详单逻辑
//    //    var inboundorder = new inboundorder
//    //    {
//    //        // 设置入库单信息
//    //        // ...

//    //        // 添加入库详单
//    //        inbounddetails = new list<inbounddetail>()
//    //    };

//    //    foreach (var materialinfo in inboundrequest.materials)
//    //    {
//    //        // 根据物料信息生成入库详单
//    //        var inbounddetail = new inbounddetail
//    //        {
//    //            // 设置入库详单信息
//    //            // ...
//    //        };

//    //        inboundorder.inbounddetails.add(inbounddetail);
//    //    }

//    //    // 保存入库单到数据库或其他存储位置
//    //    // ...

//    //    return inboundorder;
//    //}

//    //private agvtask generateagvtask(inboundrequest inboundrequest, inboundorder inboundorder)
//    //{
//    //    // 生成agv任务逻辑
//    //    var agvtask = new agvtask
//    //    {
//    //        // 设置agv任务信息
//    //        // ...

//    //        // 关联入库单
//    //        inboundorder = inboundorder
//    //    };

//    //    // 保存agv任务到数据库或其他存储位置
//    //    // ...

//    //    return agvtask;
//    //}

//    //private void locktargetstoragelocation(string targetpoint)
//    //{
//    //    // 锁定目标库位逻辑
//    //    // ...
//    //}

//    //private void executeagvtask(agvtask agvtask)
//    //{
//    //    // 执行agv任务逻辑
//    //    // ...
//    //}

//    //private void completeinbound(inboundorder inboundorder)
//    //{
//    //    // 完成入库操作逻辑
//    //    // ...
//    //}

//}
