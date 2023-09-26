using Admin.NET.Core;

namespace Admin.NET.Application.Entity;

/// <summary>
/// 库存主表实体
/// </summary>
[SugarTable("EGInventory", "库存主表")]
public class EGInventory : EntityBase
{
    /// <summary>
    /// 物料主键
    /// </summary>
    //[Required]
    //[SugarColumn(IsIdentity = false, ColumnDescription = "物料主键", IsPrimaryKey = true)]
    //public long InventoryId { get; set; }

    /// <summary>
    /// 库存编号
    /// </summary>

    //[SugarColumn(ColumnDescription = "库存编号", Length = 50)]
    //public string? InventoryNum { get; set; }

    /// <summary>
    /// 库存总数
    /// </summary>
    [SugarColumn(ColumnDescription = "库存总数")]
    public int? ICountAll { get; set; }

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
    /// 入库编号
    /// </summary>
    [SugarColumn(ColumnDescription = "入库编号", Length = 50)]
    public string JoinBoundNum { get; set; }


    /// <summary>
    /// 仓库编号
    /// </summary>
    [SugarColumn(ColumnDescription = "仓库编号", Length = 50)]
    public string? WHNum { get; set; }

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
    public int? OutboundStatus { get; set; } = 0;


}
