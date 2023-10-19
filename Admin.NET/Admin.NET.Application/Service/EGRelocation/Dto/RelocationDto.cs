namespace Admin.NET.Application.Service.EGRelocation.Dto;

/// <summary>
/// 移库所需参数
/// </summary>
public class RelocationDto
{
    /// <summary>
    /// 料箱编号
    /// </summary>
    [Required(ErrorMessage = "料箱编号是必须的")]
    public string WorkBinNum { get; set; }

    /// <summary>
    /// 需要移动到库位的库位编号
    /// </summary>
    [Required(ErrorMessage = "需要移动的库位编号是必须的")]
    public string GOStorageNum { get; set; }

    /// <summary>
    /// 移库备注
    /// </summary>
    public string? RelocationRemake { get; set; }

}