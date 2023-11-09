namespace Admin.NET.Application.Entity;

/// <summary>
/// 区域实体
/// </summary>
[SugarTable("EG_WMS_Region", "区域信息表")]
public class EG_WMS_Region : EntityBase
{
    /// <summary>
    /// 区域编号
    /// </summary>
    [SugarColumn(ColumnDescription = "区域编号", Length = 20)]
    public string? RegionNum { get; set; }

    /// <summary>
    /// 区域名称
    /// </summary>
    [SugarColumn(ColumnDescription = "区域名称", Length = 50)]
    public string? RegionName { get; set; }

    /// <summary>
    /// 区域状态 0.正常 1.异常
    /// </summary>
    [SugarColumn(ColumnDescription = "区域状态（0.正常 1.异常）")]
    public int? RegionStatus { get; set; }

    /// <summary>
    /// 库位总数
    /// </summary>
    [SugarColumn(ColumnDescription = "库位总数")]
    public int? StoreroomCount { get; set; } = 0;

    /// <summary>
    /// 可用库位
    /// </summary>
    [SugarColumn(ColumnDescription = "可用库位")]
    public int? StoreroomUsable { get; set; } = 0;

    /// <summary>
    /// 创建者姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者姓名", Length = 20)]
    public string? CreateUserName { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "修改者姓名", Length = 20)]
    public string? UpdateUserName { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 300)]
    public string? RegionRemake { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    [SugarColumn(ColumnDescription = "仓库编号", Length = 50)]
    public string? WHNum { get; set; }

    /// <summary>
    /// 区域绑定物料编号
    /// </summary>
    [SugarColumn(ColumnDescription = "区域绑定物料编号")]
    public string? RegionMaterielNum { get; set; }

    /// <summary>
    /// 区域类型
    /// </summary>
    [SugarColumn(ColumnDescription = "区域类型")]
    public string? RegionType { get; set; }

}
