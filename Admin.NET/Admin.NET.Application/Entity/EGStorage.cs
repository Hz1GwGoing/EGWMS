using Admin.NET.Core;

namespace Admin.NET.Application.Entity;

/// <summary>
/// 库位实体
/// </summary>
[SugarTable("EGStorage", "库位信息表")]
public class EGStorage : EntityBase
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
    /// 库位地址
    /// </summary>
    [SugarColumn(ColumnDescription = "库位地址", Length = 100)]
    public string? StorageAddress { get; set; }

    /// <summary>
    /// 库位类别
    /// </summary>
    [SugarColumn(ColumnDescription = "库位类别", Length = 100)]
    public string? StorageType { get; set; }

    /// <summary>
    /// 仓库名称
    /// </summary>
    [SugarColumn(ColumnDescription = "仓库名称", Length = 100)]
    public string? WareHouseName { get; set; }

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
    /// 是否占用
    /// </summary>
    [SugarColumn(ColumnDescription = "是否占用", Length = 5)]
    public string? StorageOccupy { get; set; }

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
    /// 仓库编号
    /// </summary>
    /// ！

    [SugarColumn(ColumnDescription = "仓库编号", Length = 50)]
    public string? WHNum { get; set; }

    /// <summary>
    /// 区域编号
    /// </summary>
    [SugarColumn(ColumnDescription = "区域编号", Length = 20)]
    public string? RegionNum { get; set; }

}
