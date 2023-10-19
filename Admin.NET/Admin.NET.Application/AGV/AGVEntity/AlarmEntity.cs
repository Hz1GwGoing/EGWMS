namespace Admin.NET.Application.AGV.AGVEntity;

/// <summary>
/// agv报警消息实体
/// </summary>
[SugarTable("EG_AGV_Alarm", "AGV报警消息表")]
public class AlarmEntity : EntityBase
{
    /// <summary>
    /// 设备编号
    /// </summary>
    [SugarColumn(ColumnDescription = "设备编号")]
    public string AgvNo { get; set; }

    /// <summary>
    /// 设备序列号，RCS 生成
    /// </summary>
    [SugarColumn(ColumnDescription = "设备序列号")]
    public string AssetNumber { get; set; }

    /// <summary>
    /// 报警描述
    /// </summary>
    [SugarColumn(ColumnDescription = "报警描述")]
    public string DlarmDesc { get; set; }

    /// <summary>
    /// 报警类型
    /// </summary>
    [SugarColumn(ColumnDescription = "报警类型")]
    public int AlarmType { get; set; }

    /// <summary>
    /// 区域 id
    /// </summary>
    [SugarColumn(ColumnDescription = "区域 id")]
    public int AreaId { get; set; }

    /// <summary>
    /// 是否已读
    /// </summary>
    [SugarColumn(ColumnDescription = "是否已读")]
    public int IsReadFlag { get; set; }

    /// <summary>
    /// 报警位置
    /// </summary>
    [SugarColumn(ColumnDescription = "报警位置")]
    public string AlarmSite { get; set; }

    /// <summary>
    /// 报警系统
    /// </summary>
    [SugarColumn(ColumnDescription = "报警系统")]
    public string AlarmSource { get; set; }

    /// <summary>
    /// 建议处理
    /// </summary>
    [SugarColumn(ColumnDescription = "建议处理")]
    public string Proposal { get; set; }

    /// <summary>
    /// 报警日期
    /// </summary>
    [SugarColumn(ColumnDescription = "报警日期")]
    public DateTime AddDate { get; set; }

    /// <summary>
    /// 报警等级:0,1,2 数字越 高越严重
    /// </summary>
    [SugarColumn(ColumnDescription = "报警等级")]
    public int AlarmGrade { get; set; }

    /// <summary>
    /// 工作中心编号
    /// </summary>
    [SugarColumn(ColumnDescription = "产线编号")]
    public string WorkLineNo { get; set; }

    /// <summary>
    /// 工作中心名称
    /// </summary>
    [SugarColumn(ColumnDescription = "产线名称")]
    public string WorkLineName { get; set; }
}

