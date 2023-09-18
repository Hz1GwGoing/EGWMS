using Admin.NET.Core;

namespace Admin.NET.Application.Entity;

/// <summary>
/// 盘点实体
/// </summary>
[SugarTable("EGTakeStock", "盘点信息表")]
public class EGTakeStock : EntityBase
{
    /// <summary>
    /// 盘点编号
    /// </summary>
    [SugarColumn(ColumnDescription = "盘点编号", Length = 50)]
    public string? TakeStockNum { get; set; }

    /// <summary>
    /// 盘点状态
    /// </summary>
    [SugarColumn(ColumnDescription = "盘点状态")]
    public int? TakeStockStatus { get; set; }

    /// <summary>
    /// 盘点时间
    /// </summary>
    [SugarColumn(ColumnDescription = "盘点时间")]
    public DateTime? TakeStockTime { get; set; }

    /// <summary>
    /// 盘点人员
    /// </summary>
    [SugarColumn(ColumnDescription = "盘点人员", Length = 50)]
    public string? TakeStockUser { get; set; }

    /// <summary>
    /// 盘点备注
    /// </summary>
    [SugarColumn(ColumnDescription = "盘点备注", Length = 500)]
    public string? TakeStockRemake { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    [SugarColumn(ColumnDescription = "物料编号", Length = 50)]
    public string? MaterielNum { get; set; }

}
