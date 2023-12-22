namespace Admin.NET.Application;

/// <summary>
/// EGMateriel输出参数
/// </summary>
public class EGMaterielOutput
{
    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterielNum { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string? MaterielName { get; set; }

    /// <summary>
    /// 物料类别
    /// </summary>
    public string? MaterielType { get; set; }

    /// <summary>
    /// 物料规格
    /// </summary>
    public string? MaterielSpecs { get; set; }

    /// <summary>
    /// 物料描述
    /// </summary>
    public string? MaterielDescribe { get; set; }

    /// <summary>
    /// 物料来源
    /// </summary>
    public string? MaterielSource { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? MaterielReamke { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 主单位
    /// </summary>
    public string? MaterielMainUnit { get; set; }

    /// <summary>
    /// 辅单位
    /// </summary>
    public string? MaterielAssistUnit { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime? CreateTime { get; set; }

    /// <summary>
    /// 需在库数量
    /// </summary>
    public int? QuantityNeedCount { get; set; } = null;

    /// <summary>
    /// 提醒时间/h
    /// </summary>
    public double? InventoryDateTime { get; set; } = null;
}


