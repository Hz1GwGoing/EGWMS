namespace Admin.NET.Application.AGV.AGVEntity;

/// <summary>
/// AGV信息实体
/// </summary>
[SugarTable("EG_AGV_InfoAgvEntity", "AGV信息表")]
public class InfoAgvEntity : EntityBase
{
    /// <summary>
    /// 设备序列号
    /// </summary>
    [SugarColumn(ColumnDescription = "设备序列号")]
    public string deviceCode { get; set; }

    /// <summary>
    /// 设备负载状态
    /// </summary>
    [SugarColumn(ColumnDescription = " 设备负载状态")]
    public string? payLoad { get; set; }

    /// <summary>
    /// 当前在执行的三方订单号
    /// </summary>
    [SugarColumn(ColumnDescription = "当前在执行的三方订单号")]
    public string? orderId { get; set; }

    /// <summary>
    /// 当前在搬运的货架编号
    /// </summary>
    [SugarColumn(ColumnDescription = "当前在搬运的货架编号")]
    public string? shelfNumber { get; set; }

    /// <summary>
    /// 设备所在二维码的 x,y 坐标，前边的值是x，后边的是y
    /// </summary>
    [SugarColumn(ColumnDescription = "设备所在二维码的 x,y 坐标，前边的值是x，后边的是y")]
    public string[]? devicePostionRec { get; set; }

    /// <summary>
    /// 设备当前位置
    /// </summary>
    [SugarColumn(ColumnDescription = "设备当前位置")]
    public string devicePosition { get; set; }

    /// <summary>
    /// 电池电量
    /// </summary>
    [SugarColumn(ColumnDescription = "电池电量")]
    public string battery { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [SugarColumn(ColumnDescription = "设备名称")]
    public string deviceName { get; set; }

    /// <summary>
    /// 设备状态：0:离线;1:空闲;2:故障;3:初始化中;4:任务中;5:充电中;7:升级中
    /// </summary>
    [SugarColumn(ColumnDescription = "设备状态：0:离线;1:空闲;2:故障;3:初始化中;4:任务中;5:充电中;7:升级中")]
    public string deviceStatus { get; set; }

    /// <summary>
    /// Idle:空闲;Initializin:初始化中;InTask:任务中;Fault:故障;Offline:离线中;InCharging:充电中;InUpgrading:升级中
    /// </summary>
    [SugarColumn(ColumnDescription = "Idle:空闲;Initializin:初始化中;InTask:任务中;Fault:故障;Offline:离线中;InCharging:充电中;InUpgrading:升级中")]
    public string state { get; set; }

    /// <summary>
    /// 方向,0.001 度
    /// </summary>
    [SugarColumn(ColumnDescription = "方向,0.001 度")]
    public string? oritation { get; set; }

    /// <summary>
    /// 速度，单位 mm/s
    /// </summary>
    [SugarColumn(ColumnDescription = "速度，单位 mm/s")]
    public string? speed { get; set; }

}
