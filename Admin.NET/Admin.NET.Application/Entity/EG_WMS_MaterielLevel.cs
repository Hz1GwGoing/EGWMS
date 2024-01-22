namespace Admin.NET.Application.Entity;


/// <summary>
/// 物料级别实体
/// </summary>

[SugarTable("EG_WMS_MaterielLevel", "物料级别表")]
public class EG_WMS_MaterielLevel : EntityBase
{

    /// <summary>
    /// 物料级别编号
    /// </summary>
    [SugarColumn(ColumnDescription = "物料级别编号", Length = 50)]
    public string MaterielLevelNum { get; set; }

    /// <summary>
    /// 物料级别名称
    /// </summary>
    [SugarColumn(ColumnDescription = "物料级别名称", Length = 50)]
    public string MaterielLevelName { get; set; }

    /// <summary>
    /// 物料级别描述
    /// </summary>
    [SugarColumn(ColumnDescription = "物料级别描述", Length = 50)]
    public string? MaterielLevelDescription { get; set; }
}
