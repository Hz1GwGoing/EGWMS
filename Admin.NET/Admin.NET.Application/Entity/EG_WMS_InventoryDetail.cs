﻿namespace Admin.NET.Application.Entity;

/// <summary>
/// 库存明细表实体
/// </summary>
[SugarTable("EG_WMS_InventoryDetail", "库存明细表")]
public class EG_WMS_InventoryDetail : EntityBase
{
    /// <summary>
    /// 库存编号
    /// </summary>

    [SugarColumn(ColumnDescription = "库存编号", Length = 50)]
    public string? InventoryNum { get; set; }

    /// <summary>
    /// 生产批次
    /// </summary>
    [SugarColumn(ColumnDescription = "生产批次", Length = 50)]
    public string? ProductionLot { get; set; }

    /// <summary>
    /// 库位编号
    /// </summary>
    [SugarColumn(ColumnDescription = "库位编号", Length = 50)]
    public string? StorageNum { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    [SugarColumn(ColumnDescription = "仓库编号", Length = 50)]
    public string? WHNum { get; set; }

    /// <summary>
    /// 区域编号
    /// </summary>
    [SugarColumn(ColumnDescription = "区域编号", Length = 50)]
    public string? RegionNum { get; set; }

    /// <summary>
    /// 货架编号
    /// </summary>
    [SugarColumn(ColumnDescription = "货架编号", Length = 50)]
    public string? ShelfNum { get; set; }

    /// <summary>
    /// 冻结状态 0.未冻结 1.已冻结
    /// </summary>
    [SugarColumn(ColumnDescription = "冻结状态")]
    public int? FrozenState { get; set; }


    /// <summary>
    /// 栈板编号
    /// </summary>
    [SugarColumn(ColumnDescription = "栈板编号", Length = 50)]
    public string? PalletNum { get; set; }

    /// <summary>
    /// 料箱编号
    /// </summary>
    [SugarColumn(ColumnDescription = "料箱编号", Length = 50)]
    public string? WorkBinNum { get; set; }

    /// <summary>
    /// 库存详情表备注
    /// </summary>
    [SugarColumn(ColumnDescription = "库存详细备注")]
    public string? InventoryDetailRemake { get; set; }

}
