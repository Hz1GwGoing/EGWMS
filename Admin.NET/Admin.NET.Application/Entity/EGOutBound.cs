using Admin.NET.Core;

namespace Admin.NET.Application.Entity;

/// <summary>
/// 出库实体
/// </summary>
[SugarTable("egoutbound","出库信息表")]
public class EGOutBound  : EntityBase
{
    /// <summary>
    /// 出库编号
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "出库编号", Length = 50)]
    public string OutboundNum { get; set; }
    
    /// <summary>
    /// 出库类型
    /// </summary>
    [SugarColumn(ColumnDescription = "出库类型")]
    public int? OutboundType { get; set; }
    
    /// <summary>
    /// 出库数量
    /// </summary>
    [SugarColumn(ColumnDescription = "出库数量")]
    public int? OutboundCount { get; set; }

    /// <summary>
    /// 出库人
    /// </summary>
    [SugarColumn(ColumnDescription = "出库人", Length = 50)]
    public string? OutboundUser { get; set; }
    
    /// <summary>
    /// 出库时间
    /// </summary>
    [SugarColumn(ColumnDescription = "出库时间")]
    public DateTime? OutboundTime { get; set; }
    
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
    /// 出库备注
    /// </summary>
    [SugarColumn(ColumnDescription = "出库备注", Length = 500)]
    public string? OutboundRemake { get; set; }
    
}
