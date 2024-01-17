namespace Admin.NET.Application.Entity;

/// <summary>
/// 物料实体
/// </summary>
[SugarTable("EG_WMS_Materiel", "物料信息表")]
// 唯一索引
[SugarIndex("Index_MaterielNum_Only", nameof(EG_WMS_Materiel.MaterielNum), OrderByType.Desc, true)]
public class EG_WMS_Materiel : EntityBase
{
    /// <summary>
    /// 物料编号
    /// </summary>
    [SugarColumn(ColumnDescription = "物料编号", Length = 50)]
    public string? MaterielNum { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    [SugarColumn(ColumnDescription = "物料名称", Length = 100)]
    public string? MaterielName { get; set; }

    /// <summary>
    /// 物料类别
    /// </summary>
    [SugarColumn(ColumnDescription = "物料类别", Length = 100)]
    public string? MaterielType { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    [SugarColumn(ColumnDescription = "物料规格", Length = 200)]
    public string? MaterielSpecs { get; set; }

    /// <summary>
    /// 物料描述
    /// </summary>
    [SugarColumn(ColumnDescription = "物料描述", Length = 500)]
    public string? MaterielDescribe { get; set; }

    /// <summary>
    /// 物料来源
    /// </summary>
    [SugarColumn(ColumnDescription = "物料来源", Length = 150)]
    public string? MaterielSource { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者姓名", Length = 100)]
    public string? CreateUserName { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "修改者姓名", Length = 100)]
    public string? UpdateUserName { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 500)]
    public string? MaterielReamke { get; set; }

    /// <summary>
    /// 主单位
    /// </summary>
    [SugarColumn(ColumnDescription = "主单位", Length = 50)]
    public string? MaterielMainUnit { get; set; }

    /// <summary>
    /// 辅单位
    /// </summary>
    [SugarColumn(ColumnDescription = "辅单位", Length = 50)]
    public string? MaterielAssistUnit { get; set; }

    /// <summary>
    /// 需在库数量
    /// </summary>
    [SugarColumn(ColumnDescription = "需在库数量", Length = 50)]
    public int? QuantityNeedCount { get; set; }

    /// <summary>
    /// 提醒时间/h
    /// </summary>
    [SugarColumn(ColumnDescription = "提醒时间/h")]
    public double? InventoryDateTime { get; set; } = null;

}
