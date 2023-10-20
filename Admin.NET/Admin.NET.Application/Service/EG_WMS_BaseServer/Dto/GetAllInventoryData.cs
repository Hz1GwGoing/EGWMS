namespace Admin.NET.Application.Service.EG_WMS_BaseServer.Dto;


/// <summary>
/// 返回库存关联详情Dto
/// </summary>
public class GetAllInventoryData
{
    /// <summary>
    /// 料箱名称
    /// </summary>
    public string? WorkBinName { get; set; }

    /// <summary>
    /// 区域名称
    /// </summary>
    public string? RegionName { get; set; }
    /// <summary>
    /// 库存编号
    /// </summary>

    public string? InventoryNum { get; set; }
    /// <summary>
    /// 仓库名称
    /// </summary>
    public string? WHName { get; set; }
    /// <summary>
    /// 库存总数
    /// </summary>
    public int? ICountAll { get; set; }
    /// <summary>
    /// 可用库存
    /// </summary>
    public int? IUsable { get; set; }

    /// <summary>
    /// 冻结数量
    /// </summary>
    public int? IFrostCount { get; set; }

    /// <summary>
    /// 待检数量
    /// </summary>
    public int? IWaitingCount { get; set; }
    /// <summary>
    /// 物料类别
    /// </summary>
    public string? MaterielType { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    public string? MaterielSpecs { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    public string? WHNum { get; set; }

    /// <summary>
    /// 库位名称
    /// </summary>
    public string? StorageName { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterielNum { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterielName { get; set; }

    /// <summary>
    /// 库存主表备注
    /// </summary>
    public string? InventoryRemake { get; set; }

    /// <summary>
    /// 出库状态 0 未出库 1 已出库
    /// </summary>
    public int? OutboundStatus { get; set; } = 0;


}
