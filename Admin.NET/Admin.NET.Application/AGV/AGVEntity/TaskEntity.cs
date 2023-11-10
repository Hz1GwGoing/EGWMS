namespace Admin.NET.Application.AGV.AGVEntity;

/// <summary>
/// agv任务实体
/// </summary>

[SugarTable("EG_AGV_Task", "AGV任务表")]
public class TaskEntity : EntityBase
{
    /// <summary>
    /// 任务编号
    /// </summary>
    [SugarColumn(ColumnDescription = "任务编号")]
    public string TaskNo { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    [SugarColumn(ColumnDescription = "任务名称")]
    public string? TaskName { get; set; }

    /// <summary>
    /// AGV编号
    /// </summary>
    [SugarColumn(ColumnDescription = "AGV编号")]
    public string? AGV { get; set; }

    /// <summary>
    /// 任务状态
    /// </summary>
    [SugarColumn(ColumnDescription = "任务状态")]
    public int? TaskState { get; set; }

    /// <summary>
    /// 执行优先级
    /// </summary>
    [SugarColumn(ColumnDescription = "执行优先级")]
    public int Priority { get; set; }

    /// <summary>
    /// 任务模板编号
    /// </summary>
    [SugarColumn(ColumnDescription = "任务模板编号")]
    public string ModelNo { get; set; }

    /// <summary>
    /// 任务点集
    /// </summary>
    [SugarColumn(ColumnDescription = "任务点集")]
    public string? TaskPath { get; set; }

    /// <summary>
    /// 任务来源
    /// </summary>
    [SugarColumn(ColumnDescription = "任务来源")]
    public string? Source { get; set; }

    /// <summary>
    /// 信息
    /// </summary>
    [SugarColumn(ColumnDescription = "信息")]
    public string? Message { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [SugarColumn(ColumnDescription = "任务开始时间")]
    public DateTime? STime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [SugarColumn(ColumnDescription = "任务时间")]
    public DateTime? ETime { get; set; }

    /// <summary>
    /// 子任务序列
    /// </summary>
    [SugarColumn(ColumnDescription = "子任务序列")]
    public string? SubTaskSeq { get; set; }

    /// <summary>
    /// 子任务状态
    /// </summary>
    [SugarColumn(ColumnDescription = "子任务状态")]
    public string? SubTaskStatus { get; set; }

    /// <summary>
    /// 创建者
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者")]
    public string? AddName { get; set; }

    /// <summary>
    /// 是否追加任务（0 - 否 ，1 - 是）
    /// </summary>
    [SugarColumn(ColumnDescription = "是否追加任务（0 - 否 ，1 - 是）")]
    public int IsAdd { get; set; } = 0;

    /// <summary>
    /// 出入库编号
    /// </summary>
    [SugarColumn(ColumnDescription = "出入库编号")]
    public string? InAndOutBoundNum { get; set; }

}
