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
    public string? deviceNum { get; set; }

    /// <summary>
    /// 设备序列号，RCS 生成
    /// </summary>
    [SugarColumn(ColumnDescription = "设备序列号")]
    public string? deviceName { get; set; }

    /// <summary>
    /// 报警描述
    /// </summary>
    [SugarColumn(ColumnDescription = "报警描述")]
    public string alarmDesc { get; set; }

    /// <summary>
    /// 报警类型
    /// </summary>
    [SugarColumn(ColumnDescription = "报警类型")]
    public int alarmType { get; set; }

    /// <summary>
    /// 区域 id
    /// </summary>
    [SugarColumn(ColumnDescription = "区域 id")]
    public int areaId { get; set; }

    /// <summary>
    /// 是否已读
    /// </summary>
    [SugarColumn(ColumnDescription = "是否已读")]
    public int alarmReadFlag { get; set; }

    /// <summary>
    /// 报警位置
    /// </summary>
    [SugarColumn(ColumnDescription = "报警位置")]
    public string? channelDeviceId { get; set; }

    /// <summary>
    /// 报警系统
    /// </summary>
    [SugarColumn(ColumnDescription = "报警系统")]
    public string alarmSource { get; set; }

    /// <summary>
    /// 建议处理
    /// </summary>
    [SugarColumn(ColumnDescription = "建议处理")]
    public string? channelName { get; set; }

    /// <summary>
    /// 报警日期
    /// </summary>
    [SugarColumn(ColumnDescription = "报警日期")]
    public DateTime alarmDate { get; set; }

    /// <summary>
    /// 报警等级:0,1,2 数字越高越严重
    /// </summary>
    [SugarColumn(ColumnDescription = "报警等级:0,1,2 数字越高越严重")]
    public int alarmGrade { get; set; }

}

