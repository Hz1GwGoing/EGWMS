namespace Admin.NET.Application.Service.EG_WMS_InAndOutBound.Dto;

public class EGOutBoundDto
{
    /// <summary>
    /// 库位编号
    /// </summary>
    [Required(ErrorMessage = "库位是必须的")]
    public string StorageNum { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remake { get; set; }

}
