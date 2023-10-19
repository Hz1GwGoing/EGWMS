namespace Admin.NET.Application.AGV.AGVEntity;

/// <summary>
/// AGV信息实体
/// </summary>
[SugarTable("EG_AGV_Info", "AGV信息表")]
public class InfoEntity : EntityBase
{
    /// <summary>
    /// 设备编号
    /// </summary>
    [SugarColumn(ColumnDescription = "设备编号")]
    public string AgvNo { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [SugarColumn(ColumnDescription = "设备名称")]
    public string AgvName { get; set; }

    /// <summary>
    /// 资产编号
    /// </summary>
    [SugarColumn(ColumnDescription = "资产编号")]
    public string? AssetNumber { get; set; }

    /// <summary>
    /// AGV状态
    /// </summary>
    [SugarColumn(ColumnDescription = "AGV状态")]
    public string AgvStatus { get; set; }

    /// <summary>
    /// 电量
    /// </summary>
    [SugarColumn(ColumnDescription = "电量")]
    public string Battery { get; set; }

    /// <summary>
    /// 电池循环次数
    /// </summary>
    [SugarColumn(ColumnDescription = "电池循环次数")]
    public int BatteryCycle { get; set; }

    /// <summary>
    /// 载荷状态
    /// </summary>
    [SugarColumn(ColumnDescription = "载荷状态")]
    public string PayLoad { get; set; }

    /// <summary>
    /// 离线时间
    /// </summary>
    [SugarColumn(ColumnDescription = "离线时间")]
    public DateTime? OfflineTime { get; set; }

    /// <summary>
    /// 所在点位
    /// </summary>
    [SugarColumn(ColumnDescription = "所在点位")]
    public string Position { get; set; }

    /// <summary>
    /// 所在坐标
    /// 设备所在二维码的x,y 坐标，前边的值 是 x，后边的是 y
    /// </summary>
    [SugarColumn(ColumnDescription = "所在坐标")]
    public string PostionRec { get; set; }
}
