namespace Admin.NET.Application.Service.EG_AGV_Task.DTO;


/// <summary>
/// 获取 AGV 状态Model
/// </summary>
public class ObtainAgvStatusModel
{
    /// <summary>
    /// 区域 Id：集成控制系統提供，该字段主
    /// 要用来区分不同的仓库或区域。默认为 1
    /// </summary>
    [Required(ErrorMessage = "区域 Id不能为空")]
    public string areaId { get; set; } = "1";

    /// <summary>
    /// 固定值：0 
    /// </summary>
    [Required(ErrorMessage = "固定值：0 ")]
    public int deviceType { get; set; } = 0;

    /// <summary>
    /// 设备编号：可用作模糊搜索。多个设备编
    /// 号英文逗号分隔，不穿插所有
    /// </summary>
    public string? deviceCode { get; set; }
}