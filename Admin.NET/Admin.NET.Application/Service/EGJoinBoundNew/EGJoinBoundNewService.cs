using Admin.NET.Application.Service.EGJoinBoundNew.Dto;

namespace Admin.NET.Application.Service.EGJoinBoundNew;
public class EGJoinBoundNewService
{



    #region 测试
    //[HttpPost("Inbound")]
    //public async Task<IActionResult> Inbound([FromBody] EGInboundRequest request)
    //{
    //    if (!request.)
    //    {
    //        return BadRequest(ModelState);
    //    }

    //    // 这里，你可以对request对象进行进一步的处理，比如验证其中的数据，处理异常等。  
    //    // 然后，你可以调用相应的服务或管理器来处理AGV入库的逻辑。  

    //    var result = await _inboundService.Inbound(request);
    //    if (result.Succeeded)
    //    {
    //        return Ok(); // 返回200 OK，表示请求成功  
    //    }
    //    else
    //    {
    //        return StatusCode(StatusCodes.Status500InternalServerError); // 返回500 Internal Server Error，表示服务器内部错误  
    //    }
    //}

    //[HttpPost]
    //public async Task<IActionResult> JoinboundNew([FromBody] EGInboundRequest request)
    //{
    //    // 先判断是否是有效的数据
    //    if (!ModelState.IsValid)
    //    {

    //        return BadRequest(ModelState);
    //    }

    //    if (request.IsAGV)
    //    {
    //        // 处理AGV入库逻辑，生成入库单、入库详单，生成AGV任务，锁定目标库位等
    //        // 这里需要根据业务需求编写相应的逻辑
    //    }
    //    else
    //    {
    //        // 处理人工入库逻辑，直接添加至入库记录和库存表中
    //        // 这里需要根据业务需求编写相应的逻辑
    //    }

    //    // 返回请求成功的响应
    //    return Ok();
    //}
    #endregion


    //[HttpPost("CreateInboundRequest")]
    //public IActionResult CreateInboundRequest([FromBody] EGInboundRequest inboundRequest)
    //{
    //    // 如果为AGV入库
    //    if (inboundRequest.IsAGV)
    //    {
    //        // 生成入库单和入库详情
    //        var inboundOrder = GenerateInboundOrder(inboundRequest);
    //        var agvTask = GenerateAGVTask(inboundRequest, inboundOrder);
    //        LockTargetStorageLocation(inboundRequest.TargetPoint);

    //        // 执行AGV任务和入库操作
    //        ExecuteAGVTask(agvTask);
    //        CompleteInbound(inboundOrder);

    //        // 返回成功响应
    //        return Ok("AGV入库请求已发送并处理");
    //    }
    //    else
    //    {
    //        // 非AGV入库的处理逻辑
    //        // 可根据需求执行其他操作

    //        // 返回成功响应
    //        return Ok("非AGV入库请求已发送并处理");
    //    }
    //}

    /// <summary>
    /// 生成入库单和入库详情
    /// </summary>
    /// <param name="inboundRequest"></param>
    /// <returns></returns>
    //private async Task GenerateInboundOrder(EGInboundRequest inboundRequest)
    //{
    //    // 生成入库单和入库详单逻辑
    //    var inboundOrder = new InboundOrder
    //    {
    //        // 设置入库单信息
    //        // ...

    //        // 添加入库详单
    //        InboundDetails = new List<InboundDetail>()
    //    };

    //    foreach (var materialInfo in inboundRequest.Materials)
    //    {
    //        // 根据物料信息生成入库详单
    //        var inboundDetail = new InboundDetail
    //        {
    //            // 设置入库详单信息
    //            // ...
    //        };

    //        inboundOrder.InboundDetails.Add(inboundDetail);
    //    }

    //    // 保存入库单到数据库或其他存储位置
    //    // ...

    //    return inboundOrder;
    //}

    //private AGVTask GenerateAGVTask(InboundRequest inboundRequest, InboundOrder inboundOrder)
    //{
    //    // 生成AGV任务逻辑
    //    var agvTask = new AGVTask
    //    {
    //        // 设置AGV任务信息
    //        // ...

    //        // 关联入库单
    //        InboundOrder = inboundOrder
    //    };

    //    // 保存AGV任务到数据库或其他存储位置
    //    // ...

    //    return agvTask;
    //}

    //private void LockTargetStorageLocation(string targetPoint)
    //{
    //    // 锁定目标库位逻辑
    //    // ...
    //}

    //private void ExecuteAGVTask(AGVTask agvTask)
    //{
    //    // 执行AGV任务逻辑
    //    // ...
    //}

    //private void CompleteInbound(InboundOrder inboundOrder)
    //{
    //    // 完成入库操作逻辑
    //    // ...
    //}

}
