namespace Admin.NET.Application.Service.EG_WMS_BaseServer.Dto;


/// <summary>
/// 查询物料总数Dto
/// </summary>
public class MaterielDataSumDto
{

    /// <summary>
    /// 物料编号
    /// </summary>
    public string MaterielNum { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string MaterielName { get; set; }

    /// <summary>
    /// 物料总数
    /// </summary>
    public int SumCount { get; set; }

    /// <summary>
    /// 物料类别
    /// </summary>
    public string MaterielType { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    public string MaterielSpecs { get; set; }

    /// <summary>
    /// 主单位
    /// </summary>
    public string MaterielMainUnit { get; set; }

    /// <summary>
    /// 辅单位
    /// </summary>
    public string MaterielAssistUnit { get; set; }



}