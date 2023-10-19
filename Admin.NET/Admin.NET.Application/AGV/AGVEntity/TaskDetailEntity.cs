namespace Admin.NET.Application.AGV.AGVEntity;

/// <summary>
/// agv任务详情实体
/// </summary>

[SugarTable("EG_AGV_TaskDetail", "AGV任务详情表")]
public class TaskDetailEntity : EntityBase
{
    /// <summary>
    /// 任务ID
    /// </summary>
    [SugarColumn(ColumnDescription = "任务Id")]
    public long TaskID { get; set; }

    /// <summary>
    /// 任务路径
    /// </summary>
    [SugarColumn(ColumnDescription = "任务路径")]
    public string TaskPath { get; set; }

    /// <summary>
    /// 货架编号
    /// </summary>
    [SugarColumn(ColumnDescription = "货架编号")]
    public string? ShelfNo { get; set; }

    /// <summary>
    /// 任务路径名称
    /// </summary>
    [SugarColumn(ColumnDescription = "任务路径名称")]
    public string? TaskPathName { get; set; }


}
