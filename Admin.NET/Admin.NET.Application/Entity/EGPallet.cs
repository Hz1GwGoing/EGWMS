using Admin.NET.Core;

namespace Admin.NET.Application.Entity;

/// <summary>
/// 栈板实体
/// </summary>
[SugarTable("EGPallet", "栈板信息表")]
public class EGPallet : EntityBase
{
    /// <summary>
    /// 栈板编号
    /// </summary>
    [Required]
    [SugarColumn(ColumnDescription = "栈板编号", Length = 50)]
    public string PalletNum { get; set; }

    /// <summary>
    /// 栈板名称
    /// </summary>
    [SugarColumn(ColumnDescription = "栈板名称", Length = 100)]
    public string? PalletName { get; set; }

    /// <summary>
    /// 栈板规格
    /// </summary>
    [SugarColumn(ColumnDescription = "栈板规格")]
    public string? PalletSpecs { get; set; }

    /// <summary>
    /// 有效日期
    /// </summary>
    [SugarColumn(ColumnDescription = "有效日期")]
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// 状态 0.未启用 1.启用
    /// </summary>
    [SugarColumn(ColumnDescription = "栈板状态（0.正常 1.异常）")]
    public int? PalletStatus { get; set; }

    /// <summary>
    /// 新增人
    /// </summary>
    [SugarColumn(ColumnDescription = "新增人", Length = 50)]
    public string? CreateUserName { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 500)]
    public string? PalletRemake { get; set; }

    /// <summary>
    /// 库位编号
    /// </summary>
    [SugarColumn(ColumnDescription = "库位编号", Length = 50)]
    public string? StorageNum { get; set; }

}
