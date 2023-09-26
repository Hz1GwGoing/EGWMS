using Admin.NET.Core;

namespace Admin.NET.Application.Entity;

/// <summary>
/// 料箱实体
/// </summary>
[SugarTable("EGWorkBin", "料箱信息表")]
public class EGWorkBin : EntityBase
{
    /// <summary>
    /// 料箱编号
    /// </summary>
    [SugarColumn(ColumnDescription = "料箱编号", Length = 50)]
    public string? WorkBinNum { get; set; }

    /// <summary>
    /// 料箱名称
    /// </summary>
    [SugarColumn(ColumnDescription = "料箱名称", Length = 100)]
    public string? WorkBinName { get; set; }

    /// <summary>
    /// 科箱规格
    /// </summary>
    [SugarColumn(ColumnDescription = "科箱规格")]
    public int? WorkBinSpecs { get; set; }

    /// <summary>
    /// 机台号
    /// </summary>
    [SugarColumn(ColumnDescription = "机台号")]
    public int? MachineNum { get; set; }

    /// <summary>
    /// 班次
    /// </summary>
    [SugarColumn(ColumnDescription = "班次", Length = 10)]
    public string? Classes { get; set; }

    /// <summary>
    /// 产品数量
    /// </summary>
    [SugarColumn(ColumnDescription = "产品数量")]
    public int ProductCount { get; set; }

    /// <summary>
    /// 生产批次
    /// </summary>
    [SugarColumn(ColumnDescription = "生产批次", Length = 10)]
    public string? ProductionLot { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    [SugarColumn(ColumnDescription = "生产日期")]
    public DateTime? ProductionDate { get; set; }

    /// <summary>
    /// 生产员
    /// </summary>
    [SugarColumn(ColumnDescription = "生产员", Length = 50)]
    public string? ProductionStaff { get; set; }

    /// <summary>
    /// 检验员
    /// </summary>
    [SugarColumn(ColumnDescription = "检验员", Length = 50)]
    public string? Inspector { get; set; }

    /// <summary>
    /// 打印人
    /// </summary>
    [SugarColumn(ColumnDescription = "打印人", Length = 50)]
    public string? Printer { get; set; }

    /// <summary>
    /// 料箱状态 0.正常 1.异常
    /// </summary>
    [SugarColumn(ColumnDescription = "料箱状态（0.正常 1.异常）")]
    public int? WorkBinStatus { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    [SugarColumn(ColumnDescription = "物料编号", Length = 50)]
    public string? MaterielNum { get; set; }

    /// <summary>
    /// 打印时间
    /// </summary>
    [SugarColumn(ColumnDescription = "打印时间")]
    public DateTime? PrintTime { get; set; }

    /// <summary>
    /// 料箱备注
    /// </summary>
    [SugarColumn(ColumnDescription = "料箱备注", Length = 500)]
    public string? WorkBinRemake { get; set; }

    /// <summary>
    /// 库位编号
    /// </summary>
    /// ！
    [SugarColumn(ColumnDescription = "库位编号", Length = 50)]
    public string? StorageNum { get; set; }

    /// <summary>
    /// 栈板编号
    /// </summary>
    [SugarColumn(ColumnDescription = "栈板编号", Length = 50)]
    public string? PalletNum { get; set; }

}
