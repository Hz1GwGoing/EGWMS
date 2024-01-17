namespace Admin.NET.Application.Service.EG_AGV_Status.Dto;

/// <summary>
/// 按条件查询AGV信息BO
/// </summary>
public class AgvInfoPageBO
{
    /// <summary>
    /// 页码
    /// </summary>
    public int page { get; set; }

    /// <summary>
    /// 每页容量
    /// </summary>
    public int pagesize { get; set; }

    /// <summary>
    ///  设备序列号
    /// </summary>
    public string? deviceCode { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    public string? deviceName { get; set; }

    /// <summary>
    /// 第三方订单号
    /// </summary>
    public string? orderId { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public string? state { get; set; }
}