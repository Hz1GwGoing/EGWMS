namespace Admin.NET.Application.Entity;

/// <summary>
/// 盘点数据实体
/// </summary>
[SugarTable("EG_WMS_TakeStockData", "盘点数据表")]

public class EG_WMS_TakeStockData : EntityBase
{
    /// <summary>
    /// 盘点编号
    /// </summary>
    [SugarColumn(ColumnDescription = "盘点编号", Length = 50)]
    public string TakeStockNum { get; set; }

    /// <summary>
    /// 料箱编号
    /// </summary>
    [SugarColumn(ColumnDescription = "料箱编号", Length = 50)]
    public string WorkBinNum { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    [SugarColumn(ColumnDescription = "物料编号", Length = 50)]

    public string MaterielNum { get; set; }

    /// <summary>
    /// 库位编号
    /// </summary>
    [SugarColumn(ColumnDescription = "库位编号", Length = 50)]
    public string StorageNum { get; set; }

    /// <summary>
    ///  仓库编号
    /// </summary>
    [SugarColumn(ColumnDescription = "仓库编号", Length = 50)]
    public string WHNum { get; set; }

    /// <summary>
    /// 区域编号
    /// </summary>
    [SugarColumn(ColumnDescription = "区域编号", Length = 50)]
    public string RegionNum { get; set; }

    /// <summary>
    /// 库存总数
    /// </summary>
    [SugarColumn(ColumnDescription = "库存总数")]
    public int ICountAll { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    [SugarColumn(ColumnDescription = "生产日期")]
    public DateTime ProductionDate { get; set; }

    /// <summary>
    /// 生产批次
    /// </summary>
    [SugarColumn(ColumnDescription = "生产批次")]
    public string ProductionLot { get; set; }

}
