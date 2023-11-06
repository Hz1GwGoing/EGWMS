namespace Admin.NET.Application.Entity;

/// <summary>
/// 临时库存实体
/// </summary>
[SugarTable("EG_WMS_Tem_Inventory", "临时库存主表")]
public class EG_WMS_Tem_Inventory : EntityBase
{
    /// <summary>
    /// 库存编号
    /// </summary>

    [SugarColumn(ColumnDescription = "库存编号", Length = 50)]
    public string? InventoryNum { get; set; }

    /// <summary>
    /// 库存总数
    /// </summary>
    [SugarColumn(ColumnDescription = "库存总数")]
    public int ICountAll { get; set; }

    /// <summary>
    /// 可用库存
    /// </summary>
    [SugarColumn(ColumnDescription = "可用库存")]
    public int? IUsable { get; set; }

    /// <summary>
    /// 冻结数量
    /// </summary>
    [SugarColumn(ColumnDescription = "冻结数量")]
    public int? IFrostCount { get; set; }

    /// <summary>
    /// 待检数量
    /// </summary>
    [SugarColumn(ColumnDescription = "待检数量")]
    public int? IWaitingCount { get; set; }

    /// <summary>
    /// 出入库编号
    /// </summary>
    [SugarColumn(ColumnDescription = "出入库编号", Length = 50)]
    public string? InAndOutBoundNum { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    [SugarColumn(ColumnDescription = "物料编号", Length = 50)]
    public string? MaterielNum { get; set; }

    /// <summary>
    /// 库存主表备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注")]
    public string? InventoryRemake { get; set; }

    /// <summary>
    /// 出库状态 0 未出库 1 已出库
    /// </summary>
    [SugarColumn(ColumnDescription = "出库状态 0 未出库 1 已出库")]
    public int? OutboundStatus { get; set; } = 0;

}
