namespace Admin.NET.Application.Entity;

/// <summary>
/// 出入库主表实体
/// </summary>
[SugarTable("EG_WMS_InAndOutBound", "出入库主表")]
public class EG_WMS_InAndOutBound : EntityBase
{
    /// <summary>
    /// 出入库编号
    /// </summary>
    [Required(ErrorMessage = "出入库编号不能为空")]
    [SugarColumn(ColumnDescription = "出入库编号")]
    public string InAndOutBoundNum { get; set; }

    /// <summary>
    /// 出入库类型（0-入库，1-出库）
    /// 需要更改
    /// </summary>
    [SugarColumn(ColumnDescription = "出入库类型（0-入库，1-出库）")]
    public int? InAndOutBoundType { get; set; }

    /// <summary>
    /// 出入库状态（出入库状态（0-未入库，1-已入库，2-未出库，3-已出库，4-入库中，5-出库中）
    /// </summary>
    [SugarColumn(ColumnDescription = "（出入库状态（0-未入库，1-已入库，2-未出库，3-已出库，4-入库中，5-出库中）")]
    public int? InAndOutBoundStatus { get; set; }

    /// <summary>
    /// 出入库数量
    /// </summary>
    [SugarColumn(ColumnDescription = "出入库数量")]
    public int? InAndOutBoundCount { get; set; }

    /// <summary>
    /// 出入库人员
    /// </summary>
    [SugarColumn(ColumnDescription = "出入库人员")]
    public string? InAndOutBoundUser { get; set; }

    /// <summary>
    /// 出入库时间
    /// </summary>
    [SugarColumn(ColumnDescription = "出入库时间")]
    public DateTime? InAndOutBoundTime { get; set; }

    /// <summary>
    /// 出入库备注
    /// </summary>
    [SugarColumn(ColumnDescription = "出入库备注")]
    public string? InAndOutBoundRemake { get; set; }

    /// <summary>
    /// Agv任务编号
    /// </summary>
    [SugarColumn(ColumnDescription = "Agv任务编号")]
    public string? AgvNum { get; set; }

    /// <summary>
    /// 起始点
    /// </summary>
    [SugarColumn(ColumnDescription = "起始点")]
    public string? StartPoint { get; set; }

    /// <summary>
    /// 目标点
    /// </summary>
    [SugarColumn(ColumnDescription = "目标点")]
    public string? EndPoint { get; set; }

    /// <summary>
    /// 是否成功（0-成功，1-失败）
    /// </summary>
    [SugarColumn(ColumnDescription = "是否成功（0-成功，1-失败）")]
    public int? SuccessOrNot { get; set; }

}
