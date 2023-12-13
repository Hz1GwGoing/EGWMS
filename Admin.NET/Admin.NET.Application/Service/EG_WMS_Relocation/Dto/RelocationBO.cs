namespace Admin.NET.Application.Service.EG_WMS_Relocation.Dto;

/// <summary>
/// 密集库移库所需参数
/// </summary>
public class RelocationBO
{
    /// <summary>
    /// 需要移动的库位编号
    /// </summary>
    public string StartPoint { get; set; }

    /// <summary>
    ///  移动到的库位编号
    /// </summary>
    public string GoEndPoint { get; set; }

    /// <summary>
    ///  备注
    /// </summary>
    public string? Remake { get; set; }

}
