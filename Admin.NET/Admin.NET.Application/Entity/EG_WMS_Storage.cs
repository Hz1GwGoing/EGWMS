﻿namespace Admin.NET.Application.Entity;

/// <summary>
/// 库位实体
/// </summary>
[SugarTable("EG_WMS_Storage", "库位信息表")]
public class EG_WMS_Storage : EntityBase
{
    /// <summary>
    /// 库位编号
    /// </summary>
    [SugarColumn(ColumnDescription = "库位编号", Length = 50)]
    public string? StorageNum { get; set; }

    /// <summary>
    /// 库位名称
    /// </summary>
    [SugarColumn(ColumnDescription = "库位名称", Length = 50)]
    public string? StorageName { get; set; }

    /// <summary>
    /// 是否占用（0.未占用 1.已占用 2.预占用）
    /// </summary>
    [SugarColumn(ColumnDescription = "是否占用（0.未占用 1.已占用 2.预占用）", Length = 5)]
    public int? StorageOccupy { get; set; }

    /// <summary>
    /// 库位状态 0.正常 1.异常
    /// </summary>
    [SugarColumn(ColumnDescription = "库位状态（0.正常 1.异常）")]
    public int? StorageStatus { get; set; }

    /// <summary>
    /// 库位地址
    /// </summary>
    [SugarColumn(ColumnDescription = "库位地址", Length = 100)]
    public string? StorageAddress { get; set; }

    /// <summary>
    /// 库位类别（0-密集库 1-立库）
    /// </summary>
    [SugarColumn(ColumnDescription = "库位类别（0-密集库 1-立库）")]
    public int? StorageType { get; set; }

    /// <summary>
    /// 库位长
    /// </summary>
    [SugarColumn(ColumnDescription = "库位长", Length = 10, DecimalDigits = 5)]
    public decimal? StorageLong { get; set; }

    /// <summary>
    /// 库位宽
    /// </summary>
    [SugarColumn(ColumnDescription = "库位宽", Length = 10, DecimalDigits = 5)]
    public decimal? StorageWidth { get; set; }

    /// <summary>
    /// 库位高
    /// </summary>
    [SugarColumn(ColumnDescription = "库位高", Length = 10, DecimalDigits = 5)]
    public decimal? StorageHigh { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 500)]
    public string? StorageRemake { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者姓名", Length = 50)]
    public string? CreateUserName { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "修改者姓名", Length = 50)]
    public string? UpdateUserName { get; set; }

    /// <summary>
    /// 巷道号
    /// </summary>
    [SugarColumn(ColumnDescription = "巷道号")]
    public int? RoadwayNum { get; set; }

    /// <summary>
    /// 货架号
    /// </summary>
    [SugarColumn(ColumnDescription = "货架号")]
    public int? ShelfNum { get; set; }

    /// <summary>
    /// 层号
    /// </summary>
    [SugarColumn(ColumnDescription = "层号")]
    public int? FloorNumber { get; set; }

    /// <summary>
    /// 区域编号
    /// </summary>
    [SugarColumn(ColumnDescription = "区域编号", Length = 20)]
    public string? RegionNum { get; set; }

    /// <summary>
    /// 组别
    /// </summary>
    [SugarColumn(ColumnDescription = "组别", Length = 20)]
    public string? StorageGroup { get; set; }

    /// <summary>
    /// AGV任务编号
    /// </summary>
    [SugarColumn(ColumnDescription = "AGV任务编号", Length = 20)]
    public string? TaskNo { get; set; }

    /// <summary>
    /// 此库位料箱生产日期
    /// </summary>
    [SugarColumn(ColumnDescription = "此库位料箱生产日期")]
    public DateTime? StorageProductionDate { get; set; }

    /// <summary>
    /// 是否为等待点库位（0 - 否，1 - 是）
    /// </summary>
    [SugarColumn(ColumnDescription = "是否为等待点库位（0 - 否，1 - 是）")]
    public int? StorageHoldingPoint { get; set; }
}
