using Admin.NET.Core;

namespace Admin.NET.Application.Entity;

/// <summary>
/// 仓库实体
/// </summary>
[SugarTable("EG_WMS_WareHouse", "仓库信息表")]
public class EG_WMS_WareHouse : EntityBase
{
    /// <summary>
    /// 仓库编号
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "仓库编号", Length = 50)]
    public string WHNum { get; set; }

    /// <summary>
    /// 仓库名称
    /// </summary>
    [SugarColumn(ColumnDescription = "仓库名称", Length = 50)]
    public string? WHName { get; set; }

    /// <summary>
    /// 仓库类型
    /// </summary>
    [SugarColumn(ColumnDescription = "仓库类型")]
    public int? WHType { get; set; }

    /// <summary>
    /// 仓库地址
    /// </summary>
    [SugarColumn(ColumnDescription = "仓库地址", Length = 200)]
    public string? WHAddress { get; set; }

    /// <summary>
    /// 仓库状态 0.正常 1.异常
    /// </summary>
    [SugarColumn(ColumnDescription = "仓库状态（0.正常 1.异常）")]
    public int? WHStatus { get; set; }

    /// <summary>
    /// 区域数量
    /// </summary>
    [SugarColumn(ColumnDescription = "区域数量")]
    public int? RegionCount { get; set; } = null;

    /// <summary>
    /// 库位总数
    /// </summary>
    [SugarColumn(ColumnDescription = "库位总数")]
    public int? StoreroomCount { get; set; } = null;

    /// <summary>
    /// 可用库位
    /// </summary>
    [SugarColumn(ColumnDescription = "可用库位")]
    public int? StoreroomUsable { get; set; } = null;

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
    public string? WHRemake { get; set; }

}
