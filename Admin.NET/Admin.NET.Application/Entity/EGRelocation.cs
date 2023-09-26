using Admin.NET.Core;

namespace Admin.NET.Application.Entity;

/// <summary>
/// 移库实体
/// </summary>
[SugarTable("EGRelocation", "移库信息表")]
public class EGRelocation : EntityBase
{
    /// <summary>
    /// 移库编号
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "移库编号", Length = 50)]
    public string RelocatioNum { get; set; }

    /// <summary>
    /// 移库类型
    /// </summary>
    [SugarColumn(ColumnDescription = "移库类型")]
    public int? RelocationType { get; set; }

    /// <summary>
    /// 移库数量
    /// </summary>
    [SugarColumn(ColumnDescription = "移库数量")]
    public int? RelocationCount { get; set; }

    /// <summary>
    /// 移库人
    /// </summary>
    [SugarColumn(ColumnDescription = "移库人", Length = 50)]
    public string? RelocationUser { get; set; }

    /// <summary>
    /// 移库时间
    /// </summary>
    [SugarColumn(ColumnDescription = "移库时间")]
    public DateTime? RelocationTime { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    [SugarColumn(ColumnDescription = "仓库编号", Length = 50)]
    public string? WHNum { get; set; }

    /// <summary>
    /// 栈板编号
    /// </summary>
    /// ！
    [SugarColumn(ColumnDescription = "栈板编号", Length = 50)]
    public string? PalletNum { get; set; }

    /// <summary>
    /// 料箱编号
    /// </summary>
    /// ！
    [SugarColumn(ColumnDescription = "料箱编号", Length = 50)]
    public string? WorkBinNum { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    [SugarColumn(ColumnDescription = "物料编号", Length = 50)]
    public string? MaterielNum { get; set; }

    /// <summary>
    /// 移库备注
    /// </summary>
    [SugarColumn(ColumnDescription = "移库备注", Length = 500)]
    public string? Relocation { get; set; }

}
