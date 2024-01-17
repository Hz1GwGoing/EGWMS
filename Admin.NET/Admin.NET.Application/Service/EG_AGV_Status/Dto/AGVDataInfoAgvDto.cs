namespace Admin.NET.Application.Service.EG_AGV_Status.Dto;

/// <summary>
/// 返回agv信息
/// </summary>
public class AGVDataInfoAgvDto
{
    /// <summary>
    /// 设备序列号
    /// </summary>
    public string deviceCode { get; set; }

    /// <summary>
    /// 设备负载状态
    /// </summary>
    public string payLoad { get; set; }

    /// <summary>
    /// 当前在执行的三方订单号
    /// </summary>
    public string? orderId { get; set; } = null;

    /// <summary>
    /// 当前在搬运的货架编号
    /// </summary>
    public string? shelfNumber { get; set; } = null;

    /// <summary>
    /// 设备所在二维码的 x,y 坐标，前边的值是x，后边的是y
    /// </summary>
    public string devicePostionRec { get; set; }

    /// <summary>
    /// 设备当前位置
    /// </summary>
    public string devicePosition { get; set; }

    /// <summary>
    /// 电池电量
    /// </summary>
    public string battery { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    public string deviceName { get; set; }

    /// <summary>
    /// 设备状态：0:离线;1:空闲;2:故障;3:初始化中;4:任务中;5:充电中;7:升级中
    /// </summary>
    public string? deviceStatus { get; set; } = null;

    /// <summary>
    /// Idle:空闲;Initializin:初始化中;InTask:任务中;Fault:故障;Offline:离线中;InCharging:充电中;InUpgrading:升级中
    /// </summary>
    public string state { get; set; }

    /// <summary>
    /// 方向,0.001 度
    /// </summary>
    public string? oritation { get; set; } = null;

    /// <summary>
    /// 速度，单位 mm/s
    /// </summary>
    public string? speed { get; set; } = null;


}