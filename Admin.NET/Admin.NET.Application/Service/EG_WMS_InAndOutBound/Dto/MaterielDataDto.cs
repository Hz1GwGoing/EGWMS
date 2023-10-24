namespace Admin.NET.Application.Service.EG_WMS_InAndOutBound.Dto;
public class MaterielDataDto
{
    /// <summary>
    /// 起始点
    /// </summary>
    [Required(ErrorMessage = "起始点不能为空")]
    public string StartPoint { get; set; }

    /// <summary>
    /// 目标点
    /// </summary>
    public string? EndPoint { get; set; }

    /// <summary>
    /// 绑定料箱，物料
    /// </summary>
    public List<MaterielWorkBin> materielWorkBins { get; set; }
}
