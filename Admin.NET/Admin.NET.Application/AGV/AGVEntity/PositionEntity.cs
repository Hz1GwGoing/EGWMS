namespace Admin.NET.Application.AGV.AGVEntity;

/// <summary>
/// AGV信息点位实体
/// </summary>
[SugarTable("EG_AGV_Position", "AGV信息点位表")]
public class PositionEntity : EntityBase
{

    /// <summary>
    /// 点位编号
    /// </summary>
    [SugarColumn(ColumnDescription = "点位编号")]
    public string PositionNo { get; set; }

    /// <summary>
    /// 点位名称
    /// </summary>
    [SugarColumn(ColumnDescription = "点位名称")]
    public string PositionName { get; set; }

    /// <summary>
    /// 是否显示提供选择
    /// </summary>
    [SugarColumn(ColumnDescription = "是否显示提供选择")]
    public int IsShow { get; set; }

    /// <summary>
    /// 所属工作中心编号
    /// </summary>
    [SugarColumn(ColumnDescription = "所属工作中心编号")]
    public string WorkLineNo { get; set; }

    /// <summary>
    /// 所属工作中心名称
    /// </summary>
    [SugarColumn(ColumnDescription = "所属工作中心名称")]
    public string WorkLineName { get; set; }


}
