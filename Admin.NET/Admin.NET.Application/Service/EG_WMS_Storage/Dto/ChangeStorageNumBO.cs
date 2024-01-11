namespace Admin.NET.Application.Service.EG_WMS_Storage.Dto;
public class ChangeStorageNumBO
{
    /// <summary>
    /// 库位编号
    /// </summary>
    [Required]
    public string[] storagenum { get; set; }

    /// <summary>
    /// 区域编号
    /// </summary>
    [Required]
    public string regionnum { get; set; }

}
