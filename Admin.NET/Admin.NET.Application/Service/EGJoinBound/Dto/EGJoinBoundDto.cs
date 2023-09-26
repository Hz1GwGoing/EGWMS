namespace Admin.NET.Application;

/// <summary>
/// EGJoinBound输出参数
/// </summary>
public class EGJoinBoundDto
{
    /// <summary>
    /// 入库编号
    /// </summary>
    public string JoinBoundNum { get; set; }

    /// <summary>
    /// 入库类型
    /// </summary>
    public int? JoinBoundType { get; set; }

    /// <summary>
    /// 入库人
    /// </summary>
    public string? JoinBoundUser { get; set; }

    /// <summary>
    /// 入库数量
    /// </summary>
    public int? JoinBoundCount { get; set; } = 0;

    /// <summary>
    /// 入库时间
    /// </summary>
    public DateTime? JoinBoundTime { get; set; }

    /// <summary>
    /// 回库更新时间
    /// </summary>
    public DateTime? JoinBoundOutTime { get; set; }

    /// <summary>
    /// 仓库编号
    /// </summary>
    public string? WHNum { get; set; }

    /// <summary>
    /// 栈板编号
    /// </summary>
    public string? PalletNum { get; set; }

    /// <summary>
    /// 料箱编号
    /// </summary>
    public string? WorkBinNum { get; set; }

    /// <summary>
    /// 入库备注
    /// </summary>
    public string? JoinBoundRemake { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 物料编号
    /// </summary>
    public string? MaterielNum { get; set; }

}
