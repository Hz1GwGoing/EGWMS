﻿namespace Admin.NET.Application;

/// <summary>
/// EG_WMS_InAndOutBound输出参数
/// </summary>
public class EG_WMS_InAndOutBoundOutput
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string MaterielNum { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string MaterielName { get; set; }

    /// <summary>
    /// 料箱编号
    /// </summary>
    public string WorkBinNum { get; set; }

    /// <summary>
    /// 出入库编号
    /// </summary>
    public string InAndOutBoundNum { get; set; }

    /// <summary>
    /// 出入库类型（0-入库，1-出库）
    /// </summary>
    public int? InAndOutBoundType { get; set; }

    /// <summary>
    /// 出入库状态（0-未入库，1-已入库，2-未出库，3-已出库）
    /// </summary>
    public int? InAndOutBoundStatus { get; set; }

    /// <summary>
    /// 出入库数量
    /// </summary>
    public int? InAndOutBoundCount { get; set; }

    /// <summary>
    /// 出入库人员
    /// </summary>
    public string? InAndOutBoundUser { get; set; }

    /// <summary>
    /// 出入库时间
    /// </summary>
    public DateTime? InAndOutBoundTime { get; set; }

    /// <summary>
    /// 出入库备注
    /// </summary>
    public string? InAndOutBoundRemake { get; set; }

    /// <summary>
    /// 是否成功（0-成功，1-失败）
    /// </summary>
    public int? SuccessOrNot { get; set; }

}

/// <summary>
/// InAndOutBoundAndAGV输出参数
/// </summary>
public class EG_WMS_InAndOutBoundOutputAGV
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string MaterielNum { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string MaterielName { get; set; }

    /// <summary>
    /// 料箱编号
    /// </summary>
    public string WorkBinNum { get; set; }

    /// <summary>
    /// 出入库编号
    /// </summary>
    public string InAndOutBoundNum { get; set; }

    /// <summary>
    /// 出入库类型（0-入库，1-出库）
    /// </summary>
    public int? InAndOutBoundType { get; set; }

    /// <summary>
    /// 出入库状态（0-未入库，1-已入库，2-未出库，3-已出库）
    /// </summary>
    public int? InAndOutBoundStatus { get; set; }

    /// <summary>
    /// 出入库数量
    /// </summary>
    public int? InAndOutBoundCount { get; set; }

    /// <summary>
    /// 出入库人员
    /// </summary>
    public string? InAndOutBoundUser { get; set; }

    /// <summary>
    /// 出入库时间
    /// </summary>
    public DateTime? InAndOutBoundTime { get; set; }

    /// <summary>
    /// 出入库备注
    /// </summary>
    public string? InAndOutBoundRemake { get; set; }

    /// <summary>
    /// 是否成功（0-成功，1-失败）
    /// </summary>
    public int? SuccessOrNot { get; set; }

    /// <summary>
    /// 任务编号
    /// </summary>
    public string TaskNo { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string TaskName { get; set; }

    /// <summary>
    /// AGV编号
    /// </summary>
    public string AGV { get; set; }

    /// <summary>
    /// 
    /// 任务状态
    /// </summary>
    public int TaskState { get; set; }

    /// <summary>
    /// 异常信息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 模板编号
    /// </summary>
    public string ModelNo { get; set; }

    /// <summary>
    /// 任务点集
    /// </summary>
    public string TaskPath { get; set; }

    /// <summary>
    /// 任务来源
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///  任务结束时间
    /// </summary>
    public DateTime ETime { get; set; }
}
