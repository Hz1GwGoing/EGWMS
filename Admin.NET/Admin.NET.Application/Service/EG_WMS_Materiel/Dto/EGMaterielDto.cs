namespace Admin.NET.Application;

/// <summary>
/// EGMateriel输出参数
/// </summary>
public class EGMaterielDto
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
    /// 页数
    /// </summary>
    public int page { get; set; }

    /// <summary>
    /// 每页容量
    /// </summary>
    public int pageSize { get; set; }

}
