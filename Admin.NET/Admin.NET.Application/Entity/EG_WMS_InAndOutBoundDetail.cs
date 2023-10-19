namespace Admin.NET.Application.Entity;

/// <summary>
/// 出入库详细实体
/// </summary>
[SugarTable("EG_WMS_InAndOutBoundDetail", "出入库详细表")]
public class EG_WMS_InAndOutBoundDetail : EntityBase
{
    /// <summary>
    /// 出入库编号
    /// </summary>
    [SugarColumn(ColumnDescription = "出入库编号")]
    public string InAndOutBoundNum { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    [SugarColumn(ColumnDescription = "仓库编号", Length = 50)]
    public string? WHNum { get; set; }

    /// <summary>
    /// 库位编号
    /// </summary>
    [SugarColumn(ColumnDescription = "库位编号", Length = 50)]
    public string? StorageNum { get; set; }

    /// <summary>
    /// 区域编号
    /// </summary>
    public string? RegionNum { get; set; }

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
    /// 物料编号
    /// </summary>
    [SugarColumn(ColumnDescription = "物料编号", Length = 50)]
    public string? MaterielNum { get; set; }


}
