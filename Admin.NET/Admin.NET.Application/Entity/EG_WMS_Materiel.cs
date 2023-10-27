namespace Admin.NET.Application.Entity;

/// <summary>
/// 物料实体
/// </summary>
[SugarTable("EG_WMS_Materiel", "物料信息表")]
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
    /// 栈板编号
    /// </summary>
    [SugarColumn(ColumnDescription = "栈板编号", Length = 50)]
    public string? PalletNum { get; set; }

    /// <summary>
    /// 库存编号
    /// </summary>
    [SugarColumn(ColumnDescription = "库存编号", Length = 50)]
    public string? InventoryNum { get; set; }

}
