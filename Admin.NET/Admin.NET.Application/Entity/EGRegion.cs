using Admin.NET.Core;

namespace Admin.NET.Application.Entity;

/// <summary>
/// 区域实体
/// </summary>
[SugarTable("EGRegion", "区域信息表")]
public class EGRegion : EntityBase
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
    /// 所属仓库
    /// </summary>
    [SugarColumn(ColumnDescription = "所属仓库", Length = 50)]
    public string? WareHouseName { get; set; }

    /// <summary>
    /// 区域状态
    /// </summary>
    [SugarColumn(ColumnDescription = "区域状态")]
    public int? RegionStatus { get; set; }

    /// <summary>
    /// 库位总数
    /// </summary>
    [SugarColumn(ColumnDescription = "库位总数")]
    public int? StoreroomCount { get; set; }

    /// <summary>
    /// 可用库位
    /// </summary>
    [SugarColumn(ColumnDescription = "可用库位")]
    public int? StoreroomUsable { get; set; }

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

}
