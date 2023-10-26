namespace Admin.NET.Application.Service.EG_WMS_InAndOutBound.Dto;

/// <summary>
/// 人工入库Dto
/// </summary>
public class EGInBoundDto
{
    /// <summary>
    /// 出入库人员
    /// </summary>
    public string? InAndOutBoundUser { get; set; }

    /// <summary>
    /// 出入库备注
    /// </summary>
    public string? InAndOutBoundRemake { get; set; }

    /// <summary>
    /// 目标点
    /// </summary>
    [Required(ErrorMessage = "目标点不能为空")]
    public string EndPoint { get; set; }

    /// <summary>
    /// 绑定料箱，物料
    /// </summary>
    [Required(ErrorMessage = "入库数据不能为空")]
    public List<MaterielWorkBin> materielWorkBins { get; set; }

}

/// <summary>
/// 料箱产品
/// </summary>
public class MaterielWorkBin
{
    
    /// <summary>
    /// 料箱编号
    /// </summary>
    public string WorkBinNum { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string MaterielNum { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int ProductCount { get; set; }

    /// <summary>
    /// 生产时间
    /// </summary>
    public DateTime ProductionDate { get; set; } = DateTime.Now;

    /// <summary>
    /// 生产批次
    /// </summary>
    public string ProductionLot { get; set; }



}