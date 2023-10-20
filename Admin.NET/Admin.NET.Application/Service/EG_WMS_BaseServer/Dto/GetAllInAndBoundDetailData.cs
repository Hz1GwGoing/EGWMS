namespace Admin.NET.Application.Service.EG_WMS_BaseServer.Dto;

/// <summary>
/// 返回出入库详情Dto
/// </summary>
public class GetAllInAndBoundDetailData
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
    /// 规格型号
    /// </summary>
    public string MaterieSpecs { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int ICountAll { get; set; }

    /// <summary>
    /// 料箱编号
    /// </summary>
    public string WorkBinNum { get; set; }

    /// <summary>
    /// 料箱名称
    /// </summary>
    public string WorkBinName { get; set; }

    /// <summary>
    /// 仓库名称
    /// </summary>
    public string WHName { get; set; }

    /// <summary>
    /// 区域名称
    /// </summary>
    public string RegionName { get; set; }

    /// <summary>
    /// 库位名称
    /// </summary>
    public string StorageName { get; set; }
}