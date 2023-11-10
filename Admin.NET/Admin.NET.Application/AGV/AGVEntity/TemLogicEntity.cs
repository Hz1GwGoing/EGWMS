namespace Admin.NET.Application.AGV.AGVEntity;
/// <summary>
/// agv任务模板实体
/// </summary>

[SugarTable("EG_AGV_TemLogic", "AGV任务模板表")]

public class TemLogicEntity : EntityBase
{
    /// <summary>
    /// 模板名称
    /// </summary>
    [SugarColumn(ColumnDescription = "模板名称")]
    public string TemLogicName { get; set; }

    /// <summary>
    /// 模版编号
    /// </summary>
    [SugarColumn(ColumnDescription = "模版编号")]
    public string TemLogicNo { get; set; }

    /// <summary>
    /// 任务点位数
    /// </summary>
    [SugarColumn(ColumnDescription = "任务点位数")]
    public int PointNum { get; set; }

    /// <summary>
    /// 是否需要去等待点（0 - 否，1 - 是）
    /// </summary>
    [SugarColumn(ColumnDescription = "是否需要去等待点（0 - 否，1 - 是）")]
    public string HoldingPoint { get; set; }

}
